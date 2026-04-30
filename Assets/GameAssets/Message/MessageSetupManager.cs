using System.Linq;
using UnityEngine;

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

    public void SetupMessage(Prompt prompt)
    {
        // Clear existing messages
        foreach (Transform child in messageParent)
        {
            Destroy(child.gameObject);
        }

        SetupMessageWainting(prompt);
    }

    public void AddResponseAI(Prompt prompt, int selectedResponseIndex)
    {
        CreateMessageAICore(prompt, selectedResponseIndex);
        if (prevActiveOverText != null)
        {
            prevActiveOverText.SetNormalTextOverMessage();
        }
    }

    public void AddResponseUser(Prompt prompt, int selectedResponseIndex)
    {
        CreateMessageUserCore(prompt);
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
        CreateTimeIndicatorCore(prompt);
        CreateOverMessageCore(prompt);
        CreateMessageCurrentUserCore(prompt);
    }

    void CreateOverMessageCore(Prompt prompt)
    {
        // OVER NAME SENDER INDICATOR
        GameObject nameSenderGO = Instantiate(nameSenderPrefab, messageParent);
        TextOverMessageUI nameSenderUI = nameSenderGO.GetComponent<TextOverMessageUI>();
        if (nameSenderUI != null)
        {
            prevActiveOverText = nameSenderUI;
            nameSenderUI.SetTextOverMessage(prompt.senderName, true);
        }
    }

    void CreateTimeIndicatorCore(Prompt prompt)
    {
        // TIME INDICATOR
        GameObject timeIndicatorGO = Instantiate(timeIndicatorPrefab, messageParent);
        TimeIndicatorUI timeIndicatorUI = timeIndicatorGO.GetComponent<TimeIndicatorUI>();
        if (timeIndicatorUI != null)
        {
            timeIndicatorUI.SetTimeIndicator();
        }
    }

    void CreateMessageCurrentUserCore(Prompt prompt)
    {
        // Create new message based on the prompt's sender
        GameObject messageGO  = Instantiate(messageUserCurrentPrefab, messageParent);

        MessageUI messageUI = messageGO.GetComponent<MessageUI>();
        if (messageUI != null)
        {
            prevActiveMessageUser = messageGO;
            messageUI.SetMessage(prompt.message);
        }
    }
    void CreateMessageAICore(Prompt prompt, int selectedResponseIndex)
    {
        // Create new message based on the prompt's sender
        GameObject messageGO  = Instantiate(messageAIPrefab, messageParent);

        MessageUI messageUI = messageGO.GetComponent<MessageUI>();
        if (messageUI != null)
        {
            messageUI.SetMessage(prompt.responseOptions[selectedResponseIndex].responseAIText);
        }
    }

    void CreateMessageUserCore(Prompt prompt)
    {
        // Create new message based on the prompt's sender
        GameObject messageGO  = Instantiate(messageUserPrefab, messageParent);

        MessageUI messageUI = messageGO.GetComponent<MessageUI>();
        if (messageUI != null)
        {
            messageUI.SetMessage(prompt.responseOptions.Last().responseUserText);
        }
    }
    
}
