using UnityEngine;

public class SetupPromptManagement : MonoBehaviour
{
    private ButtonsSetup buttonsSetup;
    private MessageSetupManager messageSetupManager;

    void Awake()
    {
        buttonsSetup = GameObject.FindGameObjectWithTag("ButtonsBar")?.GetComponent<ButtonsSetup>();
        messageSetupManager = GameObject.FindGameObjectWithTag("MessagesBar")?.GetComponent<MessageSetupManager>();
    }

    public void SetupPrompt(Prompt prompt)
    {
        if (buttonsSetup != null)
        {
            buttonsSetup.SetupButtons(prompt);
        } else {
            Debug.LogWarning("ButtonsSetup component not found. Cannot setup buttons for the prompt.");
        }

        if (messageSetupManager != null)
        {
            messageSetupManager.SetupMessage(prompt);
        } else {
            Debug.LogWarning("MessageSetupManager component not found. Cannot setup message for the prompt.");
        }
    }
}
