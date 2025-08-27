using System.Text;

namespace AtelierResleriana.Unity
{
    public class TextAsset : UnityObject
    {
        [SerializedFileObjectPropertyName("m_Text")]
        public string Text { get; private set; }
        [SerializedFileObjectPropertyName("m_Script")]
        public byte[] Script { get; private set; }

        protected override void Deserialize(SerializedObject serializedObject)
        {
            // Get the script bytes
            Script = serializedObject.GetValue<byte[]>("m_Script");

            // Check for explicit text field first
            if (serializedObject.HasValue("m_Text"))
            {
                Text = serializedObject.GetValue<string>("m_Text");
            }
            // Otherwise decode the script bytes as UTF8
            else if (Script != null)
            {
                Text = Encoding.UTF8.GetString(Script);
            }
        }
    }
}
