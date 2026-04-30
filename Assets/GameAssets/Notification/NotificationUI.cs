using TMPro;
using UnityEngine;

public class NotificationUI : MonoBehaviour
{
    public TMP_Text senderNameText;
    public TMP_Text messageText;
    
    public void SetNotification(string senderName, string message)
    {
        senderNameText.text = senderName;

        string croppedMessage = message.Length > 17 ? message.Substring(0, 17) + "..." : message;

        messageText.text = "// " + croppedMessage;
    }
}
