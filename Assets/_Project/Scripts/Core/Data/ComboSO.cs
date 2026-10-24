using UnityEngine;

namespace Studio.Core
{
    [CreateAssetMenu(menuName = "Studio/ComboSO", fileName = "Combo")]
    public class ComboSO : ScriptableObject
    {
        public CardSO a;
        public CardSO b;
        public ThreadKind kind;          // Match or Conflict
        public string title;
        public int[] deltas = new int[4];
    }
}
