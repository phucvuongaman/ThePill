using UnityEngine;

namespace TheProject
{
    [CreateAssetMenu(fileName = "PlayerData", menuName = "SO/Player Data")]
    public class PlayerDataSO : ScriptableObject
    {
        public float Health = 100f;
        public float Sanity = 100f;
        public void ResetData()
        {
            Health = 100f;
            Sanity = 100f;
        }
    }
}
