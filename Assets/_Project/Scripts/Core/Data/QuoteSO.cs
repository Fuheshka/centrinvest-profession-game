using UnityEngine;

namespace Studio.Core
{
    [CreateAssetMenu(menuName = "Studio/QuoteSO", fileName = "Quote")]
    public class QuoteSO : ScriptableObject
    {
        public string id;
        public Axis axis;
        [TextArea] public string text;
        public string author;
        public bool praise;              // compliment for an axis at or above its goal
    }
}
