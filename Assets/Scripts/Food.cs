using UnityEngine;
using System.Linq;
using Random = UnityEngine.Random;

public class Food : MonoBehaviour
{
    public bool isAvailable;
    public Color myColor;

    private void Start()
    {
        var randon = Random.Range(0, GameManager.Instance._foodColors.Count() - 1);
        //IA2-P1
        myColor = this.GetComponentInChildren<Renderer>().material.color = GameManager.Instance._foodColors.Skip(Random.Range(0, randon))
                                                                                                 .First();
    }
}
