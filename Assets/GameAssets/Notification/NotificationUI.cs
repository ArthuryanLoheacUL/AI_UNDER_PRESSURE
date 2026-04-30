using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    public TMP_Text senderNameText;
    public TMP_Text messageText;

    public Slider timerSlider;

    float timer;
    float timerMax;

    public void SetNotification(string senderName, string message, float _timer = 0f, float _timerMax = 60f)
    {
        senderNameText.text = senderName;
        timer = _timer;
        timerMax = _timerMax;
        timerSlider.value = timer / timerMax;

        string croppedMessage = message.Length > 17 ? message.Substring(0, 17) + "..." : message;

        messageText.text = "// " + croppedMessage;
    }

    void Update()
    {
        if (timer > 0f)
        {
            timer -= Time.deltaTime;
            timerSlider.value = timer / timerMax;
        }
    }
}
