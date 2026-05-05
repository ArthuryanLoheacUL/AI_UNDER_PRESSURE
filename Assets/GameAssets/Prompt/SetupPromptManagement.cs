using System;
using System.Collections.Generic;
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

    public void SetupPrompt(Prompt prompt, Action<int> onResponseSelected, List<PromptsManager.PromptSaved> savedPrompts = null)
    {
        if (buttonsSetup != null)
            buttonsSetup.SetupButtons(prompt, onResponseSelected);

        if (messageSetupManager != null)
            messageSetupManager.SetupMessage(prompt, savedPrompts);
        SoundEffectManager.Instance.PlaySoundEffectRandomPitch("MessageIn");
    }

    public void NextAIPrompt(Prompt prompt, int selectedResponseIndex)
    {
        if (buttonsSetup != null)
            buttonsSetup.Desactivates();

        if (messageSetupManager != null)
            messageSetupManager.AddResponseAI(prompt, selectedResponseIndex);
        SoundEffectManager.Instance.PlaySoundEffectRandomPitch("MessageOut");
    }

    public void NextUserPrompt(Prompt prompt, int selectedResponseIndex)
    {
        if (buttonsSetup != null)
            buttonsSetup.Desactivates();

        if (messageSetupManager != null)
            messageSetupManager.AddResponseUser(prompt, selectedResponseIndex);
        SoundEffectManager.Instance.PlaySoundEffectRandomPitch("MessageIn");
    }

    public void ClearPrompt()
    {
        if (buttonsSetup != null)
            buttonsSetup.ClearButtons();

        if (messageSetupManager != null)
            messageSetupManager.ClearMessages();
    }

    public void SetupGameOverButtons()
    {
        if (buttonsSetup != null)
            buttonsSetup.SetupGameOverButtons();
    }
}
