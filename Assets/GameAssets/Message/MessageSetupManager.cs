using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class MessageSetupManager : MonoBehaviour
{
    [Header("Message Setup")]
    public Transform messageParent; 
    [Header("Message Prefabs")]
    public GameObject messageUserPrefab;
    public GameObject messageUserCurrentPrefab;
    public GameObject messageAIPrefab;
    [Header("Indicators Prefabs")]
    public GameObject timeIndicatorPrefab;
    public GameObject nameSenderPrefab;

    [Header("Debug Previous Active")]
    private TextOverMessageUI prevActiveOverText;
    private GameObject prevActiveMessageUser;

    public void SetupMessage(Prompt prompt, List<PromptsManager.PromptSaved> savedPrompts = null)
    {
        // Clear existing messages
        foreach (Transform child in messageParent)
        {
            Destroy(child.gameObject);
        }

        if (savedPrompts != null)
        {
            foreach (var savedPrompt in savedPrompts)
            {
                CreateTimeIndicatorCore(savedPrompt.time);
                CreateOverMessageCore(savedPrompt.senderName);
                CreateMessageUserCore(savedPrompt.senderMessage, 0f, false, true);
                CreateMessageAICore(savedPrompt.IaMessage, true);
                CreateMessageUserCore(savedPrompt.userResponse, 0f, false, true);
            }
        }

        SetupMessageWainting(prompt);
    }

    public void ClearMessages()
    {
        foreach (Transform child in messageParent)
        {
            Destroy(child.gameObject);
        }
    }

    public void AddResponseAI(Prompt prompt, int selectedResponseIndex)
    {
        CreateMessageAICore(prompt.responseOptions[selectedResponseIndex].optionText);
        if (prevActiveOverText != null)
        {
            prevActiveOverText.SetNormalTextOverMessage();
        }
    }

    public void AddResponseUser(Prompt prompt, int selectedResponseIndex)
    {
        CreateMessageUserCore(prompt, selectedResponseIndex);
        if (prevActiveOverText != null)
        {
            prevActiveOverText.SetNormalTextOverMessage();
        }
    }

    void ReplaceMessageUserCurrentWithMessageUser()
    {
        if (prevActiveMessageUser != null)
        {
            string message = prevActiveMessageUser.GetComponent<MessageUI>().GetMessage();
            Vector3 position = prevActiveMessageUser.transform.position;
            Quaternion rotation = prevActiveMessageUser.transform.rotation;
            int siblingIndex = prevActiveMessageUser.transform.GetSiblingIndex();
            Destroy(prevActiveMessageUser);
            GameObject messageGO  = Instantiate(messageUserPrefab, position, rotation, messageParent);
            messageGO.transform.SetSiblingIndex(siblingIndex);
            MessageUI messageUI = messageGO.GetComponent<MessageUI>();
            if (messageUI != null)
            {
                messageUI.SetMessage(message);
            }
            prevActiveMessageUser = null;
        }
    }

    void SetupMessageWainting(Prompt prompt)
    {
        CreateTimeIndicatorCore(System.DateTime.Now.ToString("HH:mm:ss"));
        CreateOverMessageCore(prompt.senderName);
        CreateMessageCurrentUserCore(prompt.message, prompt.timer, false);//prompt.isUrgent);
    }

    void CreateOverMessageCore(string senderName)
    {
        // OVER NAME SENDER INDICATOR
        GameObject nameSenderGO = Instantiate(nameSenderPrefab, messageParent);
        TextOverMessageUI nameSenderUI = nameSenderGO.GetComponent<TextOverMessageUI>();
        if (nameSenderUI != null)
        {
            prevActiveOverText = nameSenderUI;
            nameSenderUI.SetTextOverMessage(senderName, true);
        }
    }

    void CreateTimeIndicatorCore(string text)
    {
        // TIME INDICATOR
        GameObject timeIndicatorGO = Instantiate(timeIndicatorPrefab, messageParent);
        TimeIndicatorUI timeIndicatorUI = timeIndicatorGO.GetComponent<TimeIndicatorUI>();
        if (timeIndicatorUI != null)
        {
            timeIndicatorUI.SetTimeIndicator(text);
        }
    }

    void CreateMessageCurrentUserCore(string message, float timer = 0f, bool isUrgent = false)
    {
        // Create new message based on the prompt's sender
        GameObject messageGO  = Instantiate(messageUserCurrentPrefab, messageParent);

        MessageUI messageUI = messageGO.GetComponent<MessageUI>();
        if (messageUI != null)
        {
            prevActiveMessageUser = messageGO;
            messageUI.SetMessage(message, timer, isUrgent);
        }
    }
    void CreateMessageAICore(string text, bool skip = false)
    {
        // Create new message based on the prompt's sender
        GameObject messageGO  = Instantiate(messageAIPrefab, messageParent);

        MessageUI messageUI = messageGO.GetComponent<MessageUI>();
        if (messageUI != null)
        {
            messageUI.SetMessage(text, -1, false, skip);
        }
    }

    void CreateMessageUserCore(Prompt prompt, int selectedResponseIndex)
    {
        // Create new message based on the prompt's sender
        GameObject messageGO  = Instantiate(messageUserPrefab, messageParent);

        MessageUI messageUI = messageGO.GetComponent<MessageUI>();
        if (messageUI != null)
        {
            messageUI.SetMessage(prompt.responseOptions[selectedResponseIndex].responseUserText);
        }
        if (RessourceManager.Instance.GetBonheur() <= 0 || prompt.responseOptions[selectedResponseIndex].isGameOverResponse)
        {
            GameObject messageGO2  = Instantiate(messageUserPrefab, messageParent);

            MessageUI messageUI2 = messageGO2.GetComponent<MessageUI>();
            if (messageUI2 != null)
            {
                string userResponse = "A cause de vos résultats médiocres l'équipage à décidé de vous débrancher ...";
                messageUI2.SetGameOver();
                if (prompt.responseOptions[selectedResponseIndex].isGameOverResponse)
                {
                    userResponse = prompt.responseOptions[selectedResponseIndex].gameOverMessage;
                }

                messageUI2.SetMessage(userResponse);
            }
        }
    }

    void CreateMessageUserCore(string userResponse, float timer = 0f, bool isUrgent = false, bool skip = false)
    {
        // Create new message based on the prompt's sender
        GameObject messageGO  = Instantiate(messageUserPrefab, messageParent);

        MessageUI messageUI = messageGO.GetComponent<MessageUI>();
        if (messageUI != null)
        {
            messageUI.SetMessage(userResponse, timer, isUrgent, skip);
        }
    }
    
}
