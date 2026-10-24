using UnityEngine;

namespace Studio.Core
{
    [CreateAssetMenu(menuName = "Studio/MessageSO", fileName = "Message")]
    public class MessageSO : ScriptableObject
    {
        public string id;
        public Sender sender;
        public Stage stage;
        public int day;
        [TextArea] public string text;
        public Sprite[] images = new Sprite[0];
        public OptionSO[] options = new OptionSO[0];
    }
}
