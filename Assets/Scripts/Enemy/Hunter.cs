using System;
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
            var idle = new State<StatesHunter>("Idle");
            var patrol = new State<StatesHunter>("Patrol");
            var attack = new State<StatesHunter>("Attack");

            ConfigStates(idle, attack, patrol);

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

            _myFsm = new EventFSM<StatesHunter>(idle);
        }

        private void Update() => _myFsm.Update();

        private void ConfigStates(State<StatesHunter> idle, State<StatesHunter> attack, State<StatesHunter> patrol)
        {
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
                .SetTransition(StatesHunter.Patrol, patrol)
                .Done();
        }

        #region PatrolState

        private Vector3 Patrol()
        {
            Transform wp = waypoints[_currentWaypoint];
            Vector3 desired = wp.position - transform.position;
            desired.y = 0;

            if (desired.magnitude <= 0.2f) _currentWaypoint++;

            if (stamina.Energy <= 0) _myFsm.SendInput(StatesHunter.Idle);
            else stamina.RestEnergy();

            ChangeToAttackState();

            return Steering(desired);
        }

        private void ChangeToAttackState()
        {
            // foreach (var item in GameManager.instance.boids)
            // {
            //     Vector3 dist = item.transform.position - _hunter.transform.position;
            //
            //     if (dist.magnitude <= _hunter.radiusVision)
            //         _fsm.ChangeState(HunterStates.Attack);
            // }
        }

        #endregion

        #region AttackState

        Vector3 Pursuit()
        {
            Vector3 desired = Vector3.zero;
            Boids boidsDestroy = null;

            // var enemyCloser = GameManager.instance.boids[0];
            // float distEnemy = Vector3.Distance(_hunter.transform.position, enemyCloser.transform.position);

            // foreach (var item in GameManager.instance.boids)
            // {
            //     float distEnemy2 = Vector3.Distance(_hunter.transform.position, item.transform.position);
            //
            //     if (distEnemy2 <= distEnemy && distEnemy2 <= _hunter.radiusVision)
            //     {
            //         enemyCloser = item;
            //         distEnemy = distEnemy2;
            //         //Vector3 futurePos = enemyCloser.transform.position + enemyCloser.velocity * Time.deltaTime;
            //         //desired = futurePos - _hunter.transform.position;
            //     }
            //             
            //     if (distEnemy <= radiusKill)
            //     {
            //         boidsDestroy = enemyCloser;
            //     }            
            // }

            if (boidsDestroy != null)
            {
                //GameManager.instance.boids.Remove(boidsDestroy);
                //GameObject.Destroy(boidsDestroy.gameObject);
            }

            stamina.RestEnergy();

            if (stamina.Energy <= 0) _myFsm.SendInput(StatesHunter.Idle);
            if (desired == Vector3.zero) _myFsm.SendInput(StatesHunter.Patrol);

            return Steering(desired);
        }

        #endregion

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