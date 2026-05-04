using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class NotificationUI : MonoBehaviour
{
    public TMP_Text senderNameText;
    public TMP_Text messageText;

    public Slider timerSlider;

    [Header("Colors Time Close")]
    public Image backgroundImage;
    public Color BackColorTimeClose;
    public Outline outline;
    public Color outlineColorTimeClose;
    public TMP_Text[] textsTimeClose;
    public Color senderNameTextColorTimeClose;
    public Color fillColorTimeClose;
    public Image timerBackgroundImage;
    public Color backgroundColorTimeClose;


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
        if (timer <= timerMax * 0.25f)
        {
            backgroundImage.color = BackColorTimeClose;
            outline.effectColor = outlineColorTimeClose;
            senderNameText.color = senderNameTextColorTimeClose;
            foreach (TMP_Text text in textsTimeClose)
            {
                text.color = senderNameTextColorTimeClose;
            }
            timerSlider.fillRect.GetComponent<Image>().color = fillColorTimeClose;
            timerBackgroundImage.color = backgroundColorTimeClose;
        }
    }
}
