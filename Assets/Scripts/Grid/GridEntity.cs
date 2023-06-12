using System;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.Serialization;

public class GridEntity : MonoBehaviour
{
    public event Action<GridEntity> OnMove = delegate { };
    public SpatialGrid spatialGrid;
    public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;

    private void Awake()
    {
        spatialGrid = GetComponentInParent<SpatialGrid>();
    }

    private void Update() => OnMove(this);
    
}