using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    public TMP_Text senderNameText;
    public TMP_Text messageText;

    public Slider timerSlider;



    public void SetNotification(Prompt prompt)
    {
        senderNameText.text = prompt.senderName;
        timerSlider.gameObject.SetActive(false);

        string croppedMessage = prompt.message.Length > 17 ? prompt.message.Substring(0, 17) + "..." : prompt.message;

        messageText.text = "// " + croppedMessage;
    }
}
