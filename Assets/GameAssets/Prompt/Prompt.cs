using UnityEngine;
using UnityEngine.Serialization;

[CreateAssetMenu(fileName = "Prompt", menuName = "Scriptable Objects/Prompt")]
public class Prompt : ScriptableObject
{
    // The four balancing archetypes from the design spec.
    // DILEMME : OUI = R cost, NON = B cost. No safe path.
    // DOUBLE_PEINE : both options drain (R, B, or both).
    // GAIN_MASQUE : OUI pays R to reduce B slightly. Conscious investment.
    // FAUSSE_RESP : OUI gains R but raises B. NON neutral.
    // NARRATIF : pivot scene with 0/0 costs, excluded from quota balancing.
    public enum Archetype { DILEMME, DOUBLE_PEINE, GAIN_MASQUE, FAUSSE_RESP, NARRATIF }

    [System.Serializable]
    public struct ResponseOption
    {
        [Header("Texts")]
        public string optionText; // Text to display in the UI Button for this response option
        [TextArea] public string responseUserText; // Text that the user will say when selecting this response, displayed in the conversation history

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

        [Header("Game Over")]
        public bool isGameOverResponse; // If true, selecting this response will end the game
        [TextArea] public string gameOverMessage; // Message to display when the game is over due to this response
    }

    [Header("Identity")]
    public string senderName; // Name of the sender of the prompt, displayed in the UI
    [TextArea] public string message; // Text of the prompt

    [Header("Behavior Flags")]
    public bool isUnique; // If true, this prompt will be removed from the pool of possible prompts after being picked

    [Header("Balance Archetype")]
    public Archetype archetype = Archetype.DILEMME; // Balancing class - drives garde-fou logic below

    [Header("Garde-fous (contextual gating)")]
    public bool gainBlockIfHighR;   // [GAIN_BLOCK]  - do not spawn if R > 70 (typically GAIN_MASQUE)
    public bool gainForceIfLowR;    // [GAIN_FORCE]  - priority boost if R < 25
    public bool fausseBlockIfHighF; // [FAUSSE_BLOCK]- do not spawn if F > 65 (typically FAUSSE_RESP)
    public bool doubleBlockIfHighF; // [DOUBLE_BLOCK]- do not spawn if F > 80 (typically DOUBLE_PEINE)

    [Header("Response Options")]
    public ResponseOption[] responseOptions; // Array of response options for this prompt, displayed as buttons in the UI

    [HideInInspector] public float timer = 0f; // Timer for urgent prompts
    [HideInInspector] public float timerMax = 0f;

    public Prompt(Prompt other)
    {
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
            responseOptions[i].isGameOverResponse = other.responseOptions[i].isGameOverResponse;
            responseOptions[i].gameOverMessage = other.responseOptions[i].gameOverMessage;
        }
        timer = 0f;
        timerMax = 0f;
        isUnique = other.isUnique;
        archetype = other.archetype;
        gainBlockIfHighR = other.gainBlockIfHighR;
        gainForceIfLowR = other.gainForceIfLowR;
        fausseBlockIfHighF = other.fausseBlockIfHighF;
        doubleBlockIfHighF = other.doubleBlockIfHighF;
    }
}
