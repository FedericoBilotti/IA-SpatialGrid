using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using Random = UnityEngine.Random;

public class Boids : MonoBehaviour
{
    [Header("Fuerzas")] [SerializeField] private float _maxSpeed;
    [SerializeField] [Range(0f, 0.5f)] private float _maxForce;
    private Vector3 _velocity;

    [Header("Radio de visión")] [SerializeField]
    private float _radiusSeparation;

    [SerializeField] private float _radiusAlignment;
    [SerializeField] private float _radiusCohesion;
    [SerializeField] private float _radiusFood;
    [SerializeField] private float _radiusHunter;

    [Header("Pesos")] [Range(0f, 2f)] [SerializeField]
    private float _weightSeparation;

    [Range(0f, 2f)] [SerializeField] private float _weightAligment;
    [Range(0f, 2f)] [SerializeField] private float _weightCohesion;

    [Space] [SerializeField] private float _maxCount;
    private float _internCount;

    [Header("References")] [SerializeField]
    private GridEntity myGridEntity;

    [SerializeField] private Hunter hunter;

    private SpatialGrid _spatialGrid;

    private void Awake()
    {
        if (_spatialGrid == null) _spatialGrid = GetComponentInParent<SpatialGrid>();
        if (myGridEntity == null) myGridEntity = GetComponent<GridEntity>();
    }

    private void Start() => AddForce(RandomDirection());

    private void Update()
    {
        myGridEntity.velocity = _velocity;
        transform.position += _velocity * Time.deltaTime;
        transform.forward = _velocity;

        ChangePos();

        AddForce(Separation() * _weightSeparation);
        AddForce(Alignment() * _weightAligment);
        AddForce(Cohesion() * _weightCohesion);
        AddForce(Arrive());
        AddForce(Evade());
    }

    private Vector3 Separation()
    {
        Vector3 desired = Vector3.zero;
        //IA2-P1
        desired = GetBoids(_radiusSeparation)
            .Where(x => x != myGridEntity && x.gameObject.layer == 6)
            .Select(x => x.transform.position - transform.position)
            .Aggregate(desired, (x, y) => x + y);

        //foreach (var item in boids) desired += item;

        desired *= -1;
        if (desired == Vector3.zero) return desired;

        return Steering(desired);
    }

    private Vector3 Alignment()
    {
        Vector3 desired = Vector3.zero;
        int countBoids = 0;
        //IA2-P1
        desired = GetBoids(_radiusAlignment)
            .Where(x => x != myGridEntity && x.gameObject.layer == 6)
            .Aggregate(desired, (x, y) =>
            {
                countBoids++;
                return x + y.velocity;
            });

        if (countBoids == 0) return desired;
        desired /= countBoids;

        return Steering(desired);
    }

    private Vector3 Cohesion()
    {
        Vector3 desired = Vector3.zero;
        int countBoids = 0;
        //IA2-P1
        Vector3 des = GetBoids(_radiusCohesion)
            .Where(x => x != myGridEntity && x.gameObject.layer == 6)
            .Aggregate(desired, (x, y) =>
            {
                countBoids++;
                return x + transform.position;
            });

        desired = des;

        if (countBoids == 0) return desired;
        desired /= countBoids;
        desired -= transform.position;

        return Steering(desired);
    }

    private Vector3 Arrive()
    {
        Vector3 desired = Vector3.zero;
        //IA2-P1
        desired = GetBoids(_radiusFood)
            .Where(x => x != myGridEntity && x.gameObject.layer == 8)
            .Aggregate(Vector3.zero, (x, y) =>
            {
                Debug.Log("Estoy llegando");
                Vector3 distance = y.transform.position - transform.position;
                return distance / _radiusFood;
            });

        return desired == Vector3.zero ? desired : Steering(desired);
    }

    public Vector3 Evade()
    {
        Vector3 desired = Vector3.zero;
        Vector3 enemyPos = hunter.transform.position - transform.position;

        if (enemyPos.magnitude > _radiusHunter) return desired;

        Vector3 futurePos = hunter.transform.position + hunter.velocity * Time.deltaTime;

        desired = futurePos - transform.position;
        desired *= -1;

        return Steering(desired);
    }

    private Vector3 AddForce(Vector3 force) => _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);

    private Vector3 Steering(Vector3 desired) => Vector3.ClampMagnitude(desired.normalized * _maxSpeed - _velocity, _maxForce);

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

    private void ChangePos() => transform.position = GameManager.Instance.RespectLimits(transform.position);

    private Vector3 RandomDirection()
    {
        Vector3 randomDir = new(Random.Range(1f, -1f), 0, Random.Range(1f, -1f));
        randomDir.Normalize();
        randomDir *= _maxSpeed;
        return randomDir;
    }

    //private void OnDestroy() => gridEntity.OnMove -= spatialGrid.UpdateEntity;

    #region Gizmos

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.green;
        Gizmos.DrawLine(transform.position, transform.position + _velocity);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _radiusSeparation);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _radiusAlignment);

        Gizmos.color = Color.black;
        Gizmos.DrawWireSphere(transform.position, _radiusCohesion);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _radiusFood);

        Gizmos.color = Color.grey;
        Gizmos.DrawWireSphere(transform.position, _radiusHunter);
    }

    #endregion
}