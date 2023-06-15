using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Enemy;
using Unity.VisualScripting;
using UnityEditor.Playables;
using UnityEngine;
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

    [Space] [SerializeField] private float _maxCount = 2f;
    private float _internCount;

    [Header("References")] [SerializeField]
    private GridEntity myGridEntity;

    [SerializeField] private Hunter hunter;
    [SerializeField] private GridEntity foodPrefab;

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
        Vector3 desired = GetAgents(_radiusSeparation)
        //IA2-P1
            .Where(x => x != myGridEntity && x.gameObject.layer == 6)
            .Select(x => x.transform.position - transform.position)
            .Aggregate(Vector3.zero, (x, y) => x + y);

        desired *= -1;
        return desired == Vector3.zero ? desired : NewSteering(desired.normalized);
    }

    private Vector3 Alignment()
    {
        int countBoids = 0;
        //IA2-P1
        Vector3 desired = GetAgents(_radiusAlignment)
            .Where(x => x != myGridEntity && x.gameObject.layer == 6)
            .Aggregate(Vector3.zero, (x, y) =>
            {
                countBoids++;
                return x + y.velocity;
            });

        if (countBoids == 0 || desired == Vector3.zero) return desired;
        desired /= countBoids;

        return desired == Vector3.zero ? desired : NewSteering(desired.normalized);
    }

    private Vector3 Cohesion()
    {
        int countBoids = 0;
        //IA2-P1
        Vector3 desired = GetAgents(_radiusCohesion)
            .Where(x => x != myGridEntity && x.gameObject.layer == 6)
            .Aggregate(Vector3.zero, (x, y) =>
            {
                countBoids++;
                return x + transform.position;
            });;

        if (countBoids == 0 || desired == Vector3.zero) return desired;
        desired /= countBoids;
        desired -= transform.position;

        return desired == Vector3.zero ? desired : NewSteering(desired.normalized);
    }

    private Vector3 Arrive()
    {
        GridEntity foodDestroy = default;
        //IA2-P1
        Vector3 desired = GetAgents(_radiusFood)
            .Where(x => x != myGridEntity && x.gameObject.layer == 8)
            .Aggregate(Vector3.zero, (x, y) =>
            {
                Vector3 distance = y.transform.position - transform.position;
                _internCount += Time.deltaTime;
                if (_internCount >= _maxCount) foodDestroy = y;
                return distance / _radiusFood;
            });

        if (foodDestroy != null)
        {
            _internCount = 0;
            foodDestroy.OnMove -= _spatialGrid.UpdateEntity;
            StartCoroutine(SpawnFood(foodDestroy.gameObject.transform.position));
            Destroy(foodDestroy.gameObject);
            AddForce(RandomDirection());
        }   

        return desired == Vector3.zero ? desired : NewSteering(desired);

    }

    private Vector3 Evade()
    {
        //IA2-P1
        Vector3 desired = GetAgents(_radiusHunter)
            .Where(x => x != myGridEntity && x.gameObject.layer == 7)
            .Aggregate(Vector3.zero, (x, y) =>
            {
                Vector3 futuresPos = y.transform.position + y.velocity * Time.deltaTime;
                Debug.Log("Escapando");
                return (futuresPos - transform.position) * -1;
            });

        return desired == Vector3.zero ? desired : NewSteering(desired.normalized);
    }

    private void AddForce(Vector3 force) => _velocity = Vector3.ClampMagnitude(_velocity + force, _maxSpeed);

    private Vector3 Steering(Vector3 desired) => Vector3.ClampMagnitude(desired.normalized * _maxSpeed - _velocity, _maxForce);

    private Vector3 NewSteering(Vector3 desired) => Vector3.ClampMagnitude(desired * _maxSpeed - _velocity, _maxForce);

    private IEnumerable<GridEntity> GetAgents(float radius)
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

    private IEnumerator SpawnFood(Vector3 foodDestroy)
    {
        yield return new WaitForSeconds(4f);
        GridEntity newFoodObject = Instantiate(foodPrefab, foodDestroy, Quaternion.identity, GameObject.FindGameObjectWithTag("Grid").transform);
        newFoodObject.OnMove += newFoodObject.spatialGrid.UpdateEntity;
        newFoodObject.spatialGrid.UpdateEntity(newFoodObject);
    }

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