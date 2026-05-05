using UnityEngine;

[CreateAssetMenu(fileName = "Prompt", menuName = "Scriptable Objects/Prompt")]
public class Prompt : ScriptableObject
{
    [System.Serializable]
    public struct ResponseOption
    {
        [Header("Texts")]
        public string optionText; // Text to display in the UI Button for this response option
        public string responseUserText; // Text that the user will say when selecting this response, displayed in the conversation history

        [Header("Values")]
        public int ressourceGain; // Value added to the resource level when this response is selected
        public int frustrationGain; // Value added to the frustration level when this response is selected
        [Header("Prompts")]
        public PromptsManager.WeightedPrompt[] addedPrompts; // Prompts added to the pool of possible prompts
        public Prompt[] addedDirectPrompts; // Prompts added directly to the queue, without going through the normal random selection process
        [Header("Game Over")]
        public bool isGameOverResponse; // If true, selecting this response will end the game
        public string gameOverMessage; // Message to display when the game is over due to this response
    }

    public string senderName; // Name of the sender of the prompt, displayed in the UI
    [TextArea] public string message; // Text of the prompt
    public bool isUnique; // If true, this prompt will be removed from the pool of possible prompts after being picked
    public bool isUrgent; // If true, this prompt will be added to the front of the queue and have a timer
    public ResponseOption[] responseOptions; // Array of response options for this prompt, displayed as buttons in the UI

    [HideInInspector] public float timer = 0f; // Timer for urgent prompts
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
            responseOptions[i].isGameOverResponse = other.responseOptions[i].isGameOverResponse;
            responseOptions[i].gameOverMessage = other.responseOptions[i].gameOverMessage;
        }
        timer = 0f;
        timerMax = 0f;
        isUnique = other.isUnique;
        isUrgent = other.isUrgent;
    }
}
