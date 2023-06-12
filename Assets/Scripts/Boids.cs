using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boids : MonoBehaviour
{
    [Header("Fuerzas")]
    [SerializeField] private float _maxSpeed;
    [SerializeField][Range(0f, 0.5f)] private float _maxForce;

    [Header("Radio de visión")]
    [SerializeField] private float _radiusSeparation;
    [SerializeField] private float _radiusAlignment;
    [SerializeField] private float _radiusCohesion;
    [SerializeField] private float _radiusFood;
    [SerializeField] private float _radiusHunter;

    [Header("Pesos")]
    [Range(0f, 2f)]
    [SerializeField] private float _weightSeparation;
    [Range(0f, 2f)]
    [SerializeField] private float _weightAligment;
    [Range(0f, 2f)]
    [SerializeField] private float _weightCohesion;

    [Space]
    [SerializeField] private float _maxCount;
    private float _internCount;

    public Vector3 velocity { get; private set; }

    private void Start() => AddForce(RandomDirection());
    //
    // private void Update()
    // {
    //     transform.position += velocity * Time.deltaTime;
    //     transform.forward = velocity;
    //
    //     ChangePos();
    //
    //     AddForce(Separation() * _weightSeparation);
    //     AddForce(Alignment() * _weightAligment);
    //     AddForce(Cohesion() * _weightCohesion);
    //     AddForce(Arrive());
    //     AddForce(Evade());
    // }
    //
    // private Vector3 Separation()
    // {
    //     Vector3 desired = Vector3.zero;
    //
    //     foreach (var item in GameManager.instance.boids)
    //     {
    //         Vector3 distBoids = item.transform.position - transform.position;
    //
    //         if (Vector3.Distance(transform.position, item.transform.position) <= _radiusSeparation)
    //         {
    //             desired += distBoids;
    //         }
    //     }
    //     desired *= -1;
    //     if (desired == Vector3.zero) return desired;
    //
    //     return Steering(desired);
    // }
    //
    // private Vector3 Alignment()
    // {
    //     Vector3 desired = Vector3.zero;
    //     int countBoids = 0;
    //
    //     foreach (var item in GameManager.instance.boids)
    //     {
    //         if (item == this) continue;
    //
    //         if (Vector3.Distance(transform.position, item.transform.position) <= _radiusAlignment)
    //         {
    //             desired += item.velocity;
    //             countBoids++;
    //         }
    //     }
    //     if (countBoids == 0) return desired;
    //     desired /= countBoids;
    //
    //     return Steering(desired);
    // }
    //
    // private Vector3 Cohesion()
    // {
    //     Vector3 desired = Vector3.zero;
    //     int countBoids = 0;
    //
    //     foreach (var item in GameManager.instance.boids)
    //     {
    //         if (item == this) continue;
    //
    //         if (Vector3.Distance(transform.position, item.transform.position) <= _radiusCohesion)
    //         {
    //             desired += transform.position;
    //             countBoids++;
    //         }
    //     }
    //     if (countBoids == 0) return desired;
    //     desired /= countBoids;
    //     desired -= transform.position;
    //
    //     return Steering(desired);
    // }
    //
    // private Vector3 Arrive()
    // {
    //     Vector3 desired = velocity;
    //
    //     Food foodDestroy = default;
    //
    //     foreach (var item in GameManager.instance.food)
    //     {
    //         Vector3 desired3d = item.transform.position - transform.position;
    //         float dist = desired3d.magnitude;
    //
    //         if (dist <= _radiusFood)
    //         {
    //             desired = desired3d / _radiusFood;
    //
    //             _internCount += Time.deltaTime;
    //
    //             if (_internCount >= _maxCount) foodDestroy = item;
    //         }
    //     }
    //     if (foodDestroy != null)
    //     {
    //         GameManager.instance.food.Remove(foodDestroy);
    //         Destroy(foodDestroy.gameObject);
    //     }
    //
    //     if (_internCount >= _maxCount)
    //     {
    //         AddForce(RandomDirection());
    //         _internCount = 0;
    //     }
    //
    //     if (desired == Vector3.zero) return desired;
    //
    //     return Steering(desired);
    // }
    //
    // public Vector3 Evade()
    // {
    //     Vector3 desired = Vector3.zero;
    //
    //     Vector3 futurePosition = GameManager.instance.hunter.transform.position + GameManager.instance.hunter.GetVelocity() * Time.deltaTime;
    //     Vector3 positionEnemy = GameManager.instance.hunter.transform.position - transform.position;
    //     float dist = positionEnemy.magnitude;
    //
    //     if (dist < _radiusHunter)
    //     {
    //         desired = futurePosition - transform.position;
    //         desired *= -1;
    //     }
    //
    //     return Steering(desired);
    // }

    private Vector3 AddForce(Vector3 force) => velocity = Vector3.ClampMagnitude(velocity + force, _maxSpeed);

    private Vector3 Steering(Vector3 desired) => Vector3.ClampMagnitude(desired.normalized * _maxSpeed - velocity, _maxForce);

    //private Vector3 ChangePos() => transform.position = GameManager.instance.RespectLimits(transform.position);

    private Vector3 RandomDirection()
    {
        Vector3 randomDir = new(Random.Range(1f, -1f), 0, Random.Range(1f, -1f));
        randomDir.Normalize();
        randomDir *= _maxSpeed;
        return randomDir;
    }

    #region Gizmos

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
