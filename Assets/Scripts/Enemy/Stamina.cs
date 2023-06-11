using UnityEngine;

namespace Enemy
{
    public class Stamina : MonoBehaviour
    {
        [SerializeField] private float maxEnergy;
        [SerializeField] private float addEnergy;
        [SerializeField] private float restEnergy;

        public float MaxEnergy => maxEnergy;
        public float Energy { get; private set; }

        private void Awake() => Energy = maxEnergy;

        public void AddEnergy() => Energy += addEnergy * Time.deltaTime;
        public void RestEnergy() => Energy -= restEnergy * Time.deltaTime;
    }
}