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

    public Color bgColor = Color.white;
    public Color outlineColor = Color.white;
    public Color textColor = Color.white;


    void Awake()
    {
        layoutGroup = GetComponent<HorizontalLayoutGroup>();
        animationMax = animationTime;
    }

    public void SetMessage(string message, float time = -1f, bool isUrgent = false, bool skipAnimation = false)
    {
        messageText.text = message;
        if (skipAnimation)
        {
            animationTime = 0f;
        }
        isUrgent = false;
        if (timerContainer != null)
            timerContainer.SetActive(isUrgent);
        if (lineContainer != null)
            lineContainer.SetActive(isUrgent);
    }

    public void SetGameOver()
    {
        transform.GetChild(0).GetComponent<Outline>().effectColor = outlineColor;
        transform.GetChild(0).GetComponent<Image>().color = bgColor;
        messageText.color = textColor;
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
    }
}
