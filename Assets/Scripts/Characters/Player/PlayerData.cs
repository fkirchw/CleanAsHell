using UnityEngine;

namespace Characters.Player
{
    public class PlayerData : MonoBehaviour
    {
        private PlayerCombat combat;
        private PlayerCleaningSystem cleaning;
    
        private void Awake()
        {
            combat = GetComponent<PlayerCombat>();
            cleaning = GetComponent<PlayerCleaningSystem>();
        }
    
        public float HealthPercent => combat.HealthPercent;
        public bool IsCleaning => cleaning.IsCleaning;
        public bool IsDead => combat.IsDead;
        public Vector3 Position => this.transform.position;
        public bool IsLookingDown { get; set; }
    }
}