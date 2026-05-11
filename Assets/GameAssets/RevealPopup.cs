using UnityEngine;
using UnityEngine.UI;

public class RevealPopup : MonoBehaviour
{
    bool active = false;

    bool isInTransition = false;
    float durationAnimation = 0;
    public float durationAnimationMax = 1f;

    public float start = -350;
    public float end = 60;

    RectTransform verticalLayoutGroup;

    void Awake()
    {
        verticalLayoutGroup = GetComponent<RectTransform>();
        verticalLayoutGroup.sizeDelta = new Vector2(verticalLayoutGroup.sizeDelta.x, start);
    }
    public void Active()
    {
        active = !active;
        isInTransition = true;
        durationAnimation = 0;
    }

    void Update()
    {
        if (isInTransition)
        {
            durationAnimation += Time.deltaTime;
            if (durationAnimation > durationAnimationMax)
            {
                isInTransition = false;
            } else
            {
                float t = durationAnimation / durationAnimationMax;

                if (active)
                {
                    verticalLayoutGroup.sizeDelta = new Vector2(
                        verticalLayoutGroup.sizeDelta.x,
                        Mathf.Lerp(start, end, t)
                    );
                }
                else
                {
                    verticalLayoutGroup.sizeDelta = new Vector2(
                        verticalLayoutGroup.sizeDelta.x,
                        Mathf.Lerp(end, start, t)
                    );
                }
            }
        }
    }
}
