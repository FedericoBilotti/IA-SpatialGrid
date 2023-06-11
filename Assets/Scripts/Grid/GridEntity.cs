using System;
using UnityEngine;

public class GridEntity : MonoBehaviour
{
    public event Action<GridEntity> OnMove = delegate { };
    //public Vector3 velocity = new Vector3(0, 0, 0);
    public bool onGrid;
    private Renderer _rend;

    private void Awake() => _rend = GetComponent<Renderer>();

    private void Update()
    {
        _rend.material.color = onGrid ? Color.red : Color.gray;

        //transform.position += velocity * Time.deltaTime;
        OnMove(this);
    }
}