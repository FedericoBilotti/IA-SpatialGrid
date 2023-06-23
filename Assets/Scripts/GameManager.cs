using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class GameManager : MonoBehaviour
{
    public float height;
    public float width;
    public static GameManager Instance { get; private set; }
    [SerializeField] private SpatialGrid spatialGrid;
    public List<Color> _foodColors;


    private void Awake() 
    { 
        Instance = this;

        for (int i = 0; i < 20; i++)
        {
            _foodColors.Add(new Color (Random.value, Random.value, Random.value));
        }
    } 

    public Vector3 RespectLimits(Vector3 pos)
    {
        if (pos.x < -width / 2) pos.x = width / 2;
        if (pos.x > width / 2) pos.x = -width / 2;
        if (pos.z < -height / 2) pos.z = height / 2;
        if (pos.z > height / 2) pos.z = -height / 2;

        return pos;
    }

    private void OnDrawGizmos()
    {
        Vector3 topLeft = new Vector3(-width / 2, 0, height / 2);
        Vector3 topRight = new Vector3(width / 2, 0, height / 2);
        Vector3 botLeft = new Vector3(-width / 2, 0, -height / 2);
        Vector3 botRight = new Vector3(width / 2, 0, -height / 2);

        Gizmos.color = Color.blue;
        var position = transform.position;
        Gizmos.DrawLine(position + topLeft, position + topRight);
        Gizmos.DrawLine(position + topRight, position + botRight);
        Gizmos.DrawLine(position + botRight, position + botLeft);
        Gizmos.DrawLine(position + botLeft, position + topLeft);
    }

    
}