using System;
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

    public void SetupPrompt(Prompt prompt, Action<int> onResponseSelected)
    {
        if (buttonsSetup != null)
            buttonsSetup.SetupButtons(prompt, onResponseSelected);

        if (messageSetupManager != null)
            messageSetupManager.SetupMessage(prompt);
    }

    public void NextAIPrompt(Prompt prompt, int selectedResponseIndex)
    {
        if (buttonsSetup != null)
            buttonsSetup.Desactivates();

        if (messageSetupManager != null)
            messageSetupManager.AddResponseAI(prompt, selectedResponseIndex);
    }

    public void NextUserPrompt(Prompt prompt, int selectedResponseIndex)
    {
        if (buttonsSetup != null)
            buttonsSetup.Desactivates();

        if (messageSetupManager != null)
            messageSetupManager.AddResponseUser(prompt, selectedResponseIndex);
    }
}
