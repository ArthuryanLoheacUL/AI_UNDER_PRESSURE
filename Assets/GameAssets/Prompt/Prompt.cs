using UnityEngine;
using UnityEngine.Serialization;

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
        // Positive bonheurGain INCREASES bonheur (good for crew morale).
        // Negative bonheurGain DECREASES bonheur (bad for crew morale, brings game over closer).
        [FormerlySerializedAs("frustrationGain")]
        public int bonheurGain;

        [Header("Prompts - Added")]
        public PromptsManager.WeightedPrompt[] addedPrompts; // Prompts added to the pool of possible prompts (random selection)
        public Prompt[] addedDirectPrompts; // Prompts injected directly into the queue (immediate)

        [Header("Prompts - Removed")]
        public Prompt[] removedPrompts; // Specific prompts to remove permanently from the pool
        public string[] removedChainTags; // Whole chain tags to remove from the pool (e.g. "Parker", "Signal")

        [Header("Game Over")]
        public bool isGameOverResponse; // If true, selecting this response will end the game
        public string gameOverMessage; // Message to display when the game is over due to this response
    }

    [Header("Identity")]
    public string senderName; // Name of the sender of the prompt, displayed in the UI
    [TextArea] public string message; // Text of the prompt

    [Header("Behavior Flags")]
    public bool isUnique; // If true, this prompt will be removed from the pool of possible prompts after being picked
    public bool isUrgent; // If true, this prompt will be added to the front of the queue and have a timer

    [Header("Tags & Pacing")]
    public string chainTag;     // Ex: "Parker", "Signal", "Intruse" - groups prompts in a narrative chain (used for batch removal & weighting)
    public bool isCrisis;       // Crisis prompt - usually drains both gauges, escalates pressure
    public bool isRelief;       // Relief prompt - boosts bonheur or ressources, used to break tension after crisis runs

    [Header("Response Options")]
    public ResponseOption[] responseOptions; // Array of response options for this prompt, displayed as buttons in the UI

    [HideInInspector] public float timer = 0f; // Timer for urgent prompts
    [HideInInspector] public float timerMax = 0f;

    public Prompt(Prompt other)
    {
        // Carry the asset name through clones so duplicate detection in
        // PromptsManager can compare on a stable identity, not the message
        // text (which can drift with rewrites).
        name = other.name;
        senderName = other.senderName;
        message = other.message;
        responseOptions = new ResponseOption[other.responseOptions.Length];
        for (int i = 0; i < other.responseOptions.Length; i++)
        {
            responseOptions[i] = other.responseOptions[i];
            responseOptions[i].addedPrompts = other.responseOptions[i].addedPrompts;
            responseOptions[i].addedDirectPrompts = other.responseOptions[i].addedDirectPrompts;
            responseOptions[i].removedPrompts = other.responseOptions[i].removedPrompts;
            responseOptions[i].removedChainTags = other.responseOptions[i].removedChainTags;
            responseOptions[i].isGameOverResponse = other.responseOptions[i].isGameOverResponse;
            responseOptions[i].gameOverMessage = other.responseOptions[i].gameOverMessage;
        }
        timer = 0f;
        timerMax = 0f;
        isUnique = other.isUnique;
        isUrgent = other.isUrgent;
        chainTag = other.chainTag;
        isCrisis = other.isCrisis;
        isRelief = other.isRelief;
    }
}
