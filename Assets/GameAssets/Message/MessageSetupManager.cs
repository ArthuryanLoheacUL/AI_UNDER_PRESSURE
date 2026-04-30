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

    public void SetupMessage(Prompt prompt)
    {
        // Clear existing messages
        foreach (Transform child in messageParent)
        {
            Destroy(child.gameObject);
        }

        SetupMessageWainting(prompt);
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
            messageUI.SetMessage(prompt.message);
        }
    }
}
