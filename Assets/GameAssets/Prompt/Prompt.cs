using UnityEngine;

[CreateAssetMenu(fileName = "Prompt", menuName = "Scriptable Objects/Prompt")]
public class Prompt : ScriptableObject
{
    [System.Serializable]
    public struct ResponseOption
    {
        [Header("Texts")]
        public string optionText;
        public string responseUserText;

        [Header("Values")]
        public int ressourceGain;
        public int frustrationGain;
        public PromptsManager.WeightedPrompt[] addedPrompts;
        public Prompt[] addedDirectPrompts;
    }

    public string senderName;
    public string message;
    public bool isUnique;
    public bool isUrgent;
    public ResponseOption[] responseOptions;

    [HideInInspector] public float timer = 0f;
    [HideInInspector] public float timerMax = 0f;

    public Prompt(Prompt other)
    {
        senderName = other.senderName;
        message = other.message;
        responseOptions = new ResponseOption[other.responseOptions.Length];
        for (int i = 0; i < other.responseOptions.Length; i++)
        {
            responseOptions[i] = other.responseOptions[i];
            responseOptions[i].addedPrompts = other.responseOptions[i].addedPrompts;
            responseOptions[i].addedDirectPrompts = other.responseOptions[i].addedDirectPrompts;
        }
        timer = 0f;
        timerMax = 0f;
        isUnique = other.isUnique;
        isUrgent = other.isUrgent;
    }
}
