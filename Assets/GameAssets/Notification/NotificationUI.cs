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
    bool isUrgent;

    public void SetNotification(Prompt prompt)
    {
        senderNameText.text = prompt.senderName;
        timer = prompt.timer;
        timerMax = prompt.timerMax;
        timerSlider.value = timer / timerMax;
        if (!prompt.isUrgent)
        {
            timerSlider.gameObject.SetActive(false);
        }
        isUrgent = prompt.isUrgent;

        string croppedMessage = prompt.message.Length > 17 ? prompt.message.Substring(0, 17) + "..." : prompt.message;

        messageText.text = "// " + croppedMessage;
    }

    void Update()
    {
        if (!isUrgent)
            return;
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
