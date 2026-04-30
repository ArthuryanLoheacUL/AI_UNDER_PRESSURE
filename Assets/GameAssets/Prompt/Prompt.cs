using UnityEngine;

[CreateAssetMenu(fileName = "Prompt", menuName = "Scriptable Objects/Prompt")]
public class Prompt : ScriptableObject
{
    [System.Serializable]
    public struct ResponseOption
    {
        [Header("Texts")]
        public string optionText;
        public string responseAIText;
        public string responseUserText;

        [Header("Values")]
        public int ressourceGain;
        public int frustrationGain;
    }

    public string senderName;
    public string message;
    public ResponseOption[] responseOptions;
}
