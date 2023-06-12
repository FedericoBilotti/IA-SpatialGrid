using UnityEngine;
using Random = UnityEngine.Random;

public class Spawner : MonoBehaviour
{
    [SerializeField] private GameObject[] whatToSpawn;
    [SerializeField] private float radiusSpawn;

    private void Awake()
    {
        SpawnerFoodRange();
    }

    private void SpawnerFoodRange()
    {
        foreach (GameObject item in whatToSpawn)
        {
            Vector3 randomCircle = Random.insideUnitCircle * radiusSpawn;
            Vector3 range = new(randomCircle.x, 0, randomCircle.y);
            Instantiate(item, transform.position + range, Quaternion.identity, GameObject.FindGameObjectWithTag("Grid").transform);
        }
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, radiusSpawn);
    }
}
