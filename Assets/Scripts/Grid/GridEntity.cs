using System;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    public event Action<GridEntity> OnMove = delegate { };
    public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;

    private void Update() => OnMove(this);
}