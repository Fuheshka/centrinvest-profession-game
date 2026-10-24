using UnityEngine;

namespace Studio.Core
{
    [CreateAssetMenu(menuName = "Studio/CardSO", fileName = "Card")]
    public class CardSO : ScriptableObject
    {
        public string id;
        public Slot slot;
        public string title;
        public string[] tags = new string[0];
        public Cost productionCost;
        public int[] goalDeltas = new int[4];
        [Range(0, 100)] public int popularity;
    }
}
