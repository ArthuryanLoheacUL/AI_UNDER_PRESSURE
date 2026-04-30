using TMPro;
using UnityEngine;

public class MessageUI : MonoBehaviour
{
    public TMP_Text messageText;
    public TMP_Text timerText;

    float timer = -1f;

    public void SetMessage(string message, float time = -1f)
    {
        messageText.text = message;
        timer = time;
    }

    public string GetMessage()
    {
        return messageText.text;
    }

    void Update()
    {
        if (timer > 0f && timerText != null)
        {
            timer -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.CeilToInt(timer % 60);
            timerText.text = string.Format($"{minutes:0}:{seconds:00}");
        }
    }
}
