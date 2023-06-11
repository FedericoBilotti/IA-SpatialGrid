using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Serialization;

public class SpawnerFood : MonoBehaviour
{
    [SerializeField] private GameObject[] foods;
    [SerializeField] private float radiusSpawn;

    private void Start()
    {
        SpawnerFoodRange();
    }

    private void SpawnerFoodRange()
    {
        foreach (GameObject item in foods)
        {
            Vector3 randomCircle = Random.insideUnitCircle * radiusSpawn;
            Vector3 range = new(randomCircle.x, 0, randomCircle.y);
            Instantiate(item, transform.position + range, Quaternion.identity);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusSpawn);
    }
}
