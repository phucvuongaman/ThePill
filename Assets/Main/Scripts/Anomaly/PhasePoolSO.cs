using System.Collections.Generic;
using UnityEngine;

namespace TheProject
{
    // Tạo: chuột phải trong Project -> Create -> SO -> Phase Pool
    // Mỗi SO là 1 phase. Điền eventID trực tiếp trong Inspector, rồi kéo SO vào AnomalyManager.
    [CreateAssetMenu(fileName = "PhasePool_", menuName = "SO/Phase Pool")]
    public class PhasePoolSO : ScriptableObject
    {
        public List<AnomalyEventData> events = new();
    }
}
