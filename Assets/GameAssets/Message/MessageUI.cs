using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    public TMP_Text messageText;

    public void SetMessage(string message)
    {
        messageText.text = message;
    }

    public string GetMessage()
    {
        return messageText.text;
    }
}
