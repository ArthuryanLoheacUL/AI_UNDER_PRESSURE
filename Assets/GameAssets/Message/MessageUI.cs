using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MessageUI : MonoBehaviour
{
    public TMP_Text messageText;
    public TMP_Text timerText;
    public GameObject timerContainer;
    public GameObject lineContainer;
    [Header("Animation")]
    private HorizontalLayoutGroup layoutGroup;
    public AnimationCurve animationMessagePopup;
    public float animationTime = 0.5f;
    float animationMax = 0;
    public float deltaY = 20f;


    float timer = -1f;
    bool isUrgent = false;
    int lastSecondTicked = -1;

    void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        animationMax = animationTime;
    }

    public void SetMessage(string message, float time = -1f, bool isUrgent = false, bool skipAnimation = false)
    {
        messageText.text = message;
        timer = time;
        if (skipAnimation)
        {
            animationTime = 0f;
        }
        isUrgent = false;
        //        this.isUrgent = isUrgent;
        if (timerContainer != null)
            timerContainer.SetActive(isUrgent);
        if (lineContainer != null)
            lineContainer.SetActive(isUrgent);
    }

    public string GetMessage()
    {
        return messageText.text;
    }

    void Update()
    {
        if (animationTime > 0f)
        {
            float animatedY = deltaY * animationMessagePopup.Evaluate(1f - animationTime / animationMax);
            layoutGroup.padding.bottom = Mathf.RoundToInt(animatedY);
            LayoutRebuilder.ForceRebuildLayoutImmediate(layoutGroup.GetComponent<RectTransform>());
            animationTime -= Time.deltaTime;
        } else
        {
            layoutGroup.padding.bottom = 0;
        }
        return;
        if (!isUrgent)
            return;

        if (timer > 0f && timerText != null)
        {
            timer -= Time.deltaTime;
            int minutes = Mathf.FloorToInt(timer / 60);
            int seconds = Mathf.CeilToInt(timer % 60);
            timerText.text = string.Format($"{minutes:0}:{seconds:00}");
            if ((lastSecondTicked == -1 && seconds <= 10) || (seconds <= lastSecondTicked - 1 && lastSecondTicked > 0))
            {
                lastSecondTicked = seconds;
                SoundEffectManager.Instance.PlaySoundEffectRandomPitch("Bip");
            }
        }
    }
}
