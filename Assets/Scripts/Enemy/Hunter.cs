using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using UnityEngine;
using UnityEngine.Serialization;
using Random = System.Random;
using Vector3 = UnityEngine.Vector3;

namespace Enemy
{
    public class Hunter : MonoBehaviour
    {
        [Header("Fuerzas")] [SerializeField] private float maxSpeed;
        [Range(0, 0.5f)] [SerializeField] private float maxForce;
        public Vector3 velocity;

        [Header("Radius")] [Range(0f, 10f)] [SerializeField]
        private float radiusVision;

        [Range(0f, 0.5f)] [SerializeField] private float radiusKill;

        [Header("References")] [SerializeField]
        private Transform[] waypoints;

        [SerializeField] private Stamina stamina;

        private GridEntity _myGridEntity;
        private SpatialGrid _spatialGrid;
        private int _currentWaypoint;

        private enum StatesHunter
        {
            Idle,
            Attack,
            Patrol
        }

        private EventFSM<StatesHunter> _myFsm;

        private void Awake()
        {
            _spatialGrid = GetComponentInParent<SpatialGrid>();
            _myGridEntity = GetComponent<GridEntity>();

            //IA2-P3

            #region CreateStates

            var idle = new State<StatesHunter>("Idle");
            var patrol = new State<StatesHunter>("Patrol");
            var attack = new State<StatesHunter>("Attack");

            #endregion

            #region ConfigStates

            StateConfigurer.Create(idle)
                .SetTransition(StatesHunter.Attack, attack)
                .SetTransition(StatesHunter.Patrol, patrol)
                .Done();

            StateConfigurer.Create(attack)
                .SetTransition(StatesHunter.Idle, idle)
                .SetTransition(StatesHunter.Patrol, patrol)
                .Done();

            StateConfigurer.Create(patrol)
                .SetTransition(StatesHunter.Idle, idle)
                .SetTransition(StatesHunter.Attack, attack)
                .Done();

            #endregion

            #region Events

            idle.OnEnter += x => GetComponentInChildren<Renderer>().material.color = Color.white;

            idle.OnUpdate += () =>
            {
                if (stamina.Energy >= stamina.MaxEnergy) _myFsm.SendInput(StatesHunter.Patrol);
                else stamina.AddEnergy();
            };

            patrol.OnEnter += x => GetComponentInChildren<Renderer>().material.color = Color.yellow;

            patrol.OnUpdate += () =>
            {
                AddForce(Patrol());
                transform.position += velocity * Time.deltaTime;
                transform.forward = velocity;
            };
            
            attack.OnEnter += x => GetComponentInChildren<Renderer>().material.color = Color.red;

            attack.OnUpdate += () =>
            {
                AddForce(Pursuit());
                transform.position += velocity * Time.deltaTime;
                transform.forward = velocity;
            };

            #endregion

            _myFsm = new EventFSM<StatesHunter>(patrol);
        }

        private void Update()
        {
            _myGridEntity.velocity = velocity;

            _myFsm.Update();
        }

        #region PatrolState

        private Vector3 Patrol()
        {
            Transform wp = waypoints[_currentWaypoint];
            Vector3 desired = wp.position - transform.position;
            desired.y = 0;

            if (desired.magnitude <= 0.2f)
            {
                _currentWaypoint++;

                if (_currentWaypoint >= waypoints.Length) _currentWaypoint = 0;
            }

            if (stamina.Energy <= 0) _myFsm.SendInput(StatesHunter.Idle);
            else stamina.RestEnergy();

            var boids = GetBoids(radiusVision).Where(x => x != _myGridEntity && (x.transform.position - transform.position).magnitude <= radiusVision);

            if (boids.Any()) _myFsm.SendInput(StatesHunter.Attack);

            return desired == Vector3.zero ? desired : Steering(desired);
        }

        #endregion

        #region AttackState

        Vector3 Pursuit()
        {
            Vector3 desired = Vector3.zero;
            //IA2-P1
            var closerEnemy = GetBoids(radiusVision)
                .Where(x => x != _myGridEntity && x.gameObject.layer == 6)
                .Select(x => Tuple.Create(x, Vector3.Distance(transform.position, x.transform.position)))
                .OrderBy(x => x.Item2)
                .FirstOrDefault();

            if (closerEnemy != null)
            {
                Vector3 futurePos = closerEnemy.Item1.transform.position + closerEnemy.Item1.velocity * Time.deltaTime;
                desired = futurePos - transform.position;

                if (closerEnemy.Item2 <= radiusKill)
                {
                    Debug.Log("Lo mato");
                    GridEntity boidsDestroy = closerEnemy.Item1;
                    boidsDestroy.OnMove -= _spatialGrid.UpdateEntity;
                    Destroy(boidsDestroy.gameObject);
                }
            }

            stamina.RestEnergy();

            if (stamina.Energy <= 0) _myFsm.SendInput(StatesHunter.Idle);
            if (desired == Vector3.zero) _myFsm.SendInput(StatesHunter.Patrol);

            return desired == Vector3.zero ? desired : Steering(desired);
        }

        #endregion

        private IEnumerable<GridEntity> GetBoids(float radius)
        {
            return _spatialGrid.Query(
                transform.position + new Vector3(-radius, 0, -radius),
                transform.position + new Vector3(radius, 0, radius),
                x =>
                {
                    Vector3 position2d = x - transform.position;
                    position2d.y = 0;
                    return position2d.sqrMagnitude < radius * radius;
                });
        }

        private void AddForce(Vector3 force) => velocity = Vector3.ClampMagnitude(velocity + force, maxSpeed);
        private Vector3 Steering(Vector3 desired) => Vector3.ClampMagnitude(desired.normalized * maxSpeed - velocity, maxForce);

        #region Gizmos

        private void OnDrawGizmos()
        {
            Gizmos.color = Color.red;
            var position = transform.position;
            Gizmos.DrawLine(position, position + transform.forward);
        }

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var position = transform.position;
            Gizmos.DrawWireSphere(position, radiusVision);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, radiusKill);
        }

        #endregion
    }
}