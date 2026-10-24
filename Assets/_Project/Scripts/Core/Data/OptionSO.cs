using UnityEngine;

namespace Studio.Core
{
    [CreateAssetMenu(menuName = "Studio/OptionSO", fileName = "Option")]
    public class OptionSO : ScriptableObject
    {
        public string id;
        [TextArea] public string text;
        public Cost cost;
        public int[] deltas = new int[4];
        public string[] tags = new string[0];
        public MessageSO followUp;
        public int followUpDelayDays;
    }
}
