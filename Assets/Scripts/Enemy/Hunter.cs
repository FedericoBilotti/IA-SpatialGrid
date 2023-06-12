using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.Hardware;
using UnityEngine;

namespace Enemy
{
    public class Hunter : MonoBehaviour
    {
        [Header("Fuerzas")] [SerializeField] private float maxSpeed;
        [Range(0, 0.5f)] [SerializeField] private float maxForce;
        private Vector3 _velocity;

        [Header("Radius")] [Range(0f, 10f)] [SerializeField]
        private float radiusVision;

        [Range(0f, 0.5f)] [SerializeField] private float radiusKill;

        [Header("References")] [SerializeField]
        private Transform[] waypoints;

        [SerializeField] private Stamina stamina;

        private SpatialGrid _targetGrid;
        private GridEntity _myGridEntity;
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
            _myGridEntity = GetComponent<GridEntity>();
            _targetGrid = GetComponentInParent<SpatialGrid>();
            
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

            idle.OnUpdate += () =>
            {
                if (stamina.Energy >= stamina.MaxEnergy) _myFsm.SendInput(StatesHunter.Patrol);
                else stamina.AddEnergy();
            };

            patrol.OnUpdate += () =>
            {
                AddForce(Patrol());
                transform.position += _velocity * Time.deltaTime;
                transform.forward = _velocity;
            };

            attack.OnUpdate += () =>
            {
                AddForce(Pursuit());
                transform.position += _velocity * Time.deltaTime;
                transform.forward = _velocity;
            };
            
            #endregion

            _myFsm = new EventFSM<StatesHunter>(idle);
        }

        private void Update() => _myFsm.Update();

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

            if (boids.Any())
            {
                Debug.Log("Cambio a ataque");
                _myFsm.SendInput(StatesHunter.Attack);
            }

            return Steering(desired);
        }

        #endregion

        #region AttackState

        Vector3 Pursuit()
        {
            Vector3 desired = Vector3.zero;
            GridEntity boidsDestroy = null;
            GridEntity closerEnemy = GetBoids(radiusVision).OrderBy(x => Vector3.Distance(transform.position, x.transform.position)).FirstOrDefault();

            Debug.Log("Ataco 1");

            if (closerEnemy)
            {
                Vector3 futurePos = closerEnemy.transform.position * Time.deltaTime;
                //+enemyCloser.velocity
                desired = futurePos - transform.position;
                Debug.Log("Ataco 2");

                if (closerEnemy.transform.position.magnitude <= radiusKill)
                {
                    Debug.Log("Lo mato");
                    boidsDestroy = closerEnemy;
                }
            }

            // var enemyCloser = closerEnemy[0];
            // float distEnemy = Vector3.Distance(transform.position, enemyCloser.transform.position);
            //
            // foreach (var item in closerEnemy)
            // {
            //     float distEnemy2 = Vector3.Distance(transform.position, item.transform.position);
            //
            //     if (distEnemy2 <= distEnemy && distEnemy2 <= radiusVision)
            //     {
            //         enemyCloser = item;
            //         distEnemy = distEnemy2;
            //         Vector3 futurePos = enemyCloser.transform.position * Time.deltaTime;
            //         // + enemyCloser.velocity 
            //         desired = futurePos - transform.position;
            //     }
            // }

            if (boidsDestroy != null) Destroy(boidsDestroy.gameObject);

            stamina.RestEnergy();

            if (stamina.Energy <= 0) _myFsm.SendInput(StatesHunter.Idle);
            if (desired == Vector3.zero) _myFsm.SendInput(StatesHunter.Patrol);

            return Steering(desired);
        }

        #endregion

        private IEnumerable<GridEntity> GetBoids(float radius)
        {
            return _targetGrid.Query(
                transform.position + new Vector3(-radius, 0, -radius),
                transform.position + new Vector3(radius, 0, radius),
                x =>
                {
                    Vector3 position2d = x - transform.position;
                    position2d.y = 0;
                    return position2d.sqrMagnitude < radius * radius;
                });
            ;
        }

        private void AddForce(Vector3 force) => _velocity = Vector3.ClampMagnitude(_velocity + force, maxSpeed);
        private Vector3 Steering(Vector3 desired) => Vector3.ClampMagnitude(desired.normalized * maxSpeed - _velocity, maxForce);

        private void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.yellow;
            var position = transform.position;
            Gizmos.DrawWireSphere(position, radiusVision);

            Gizmos.color = Color.cyan;
            Gizmos.DrawWireSphere(position, radiusKill);
        }
    }
}