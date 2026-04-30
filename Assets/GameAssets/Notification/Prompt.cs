using UnityEngine;

[CreateAssetMenu(fileName = "Prompt", menuName = "Scriptable Objects/Prompt")]
public class Prompt : ScriptableObject
{
    [System.Serializable]
    public struct ResponseOption
    {
        public string optionText;
    }

    public string senderName;
    public string message;
    public ResponseOption[] responseOptions;
}
