using TMPro;
using UnityEngine;

public class TimeIndicatorUI : MonoBehaviour
{
    public TMP_Text timeIndicatorText;

    public void SetTimeIndicator(string text)
    {
        if (timeIndicatorText != null)
        {
            timeIndicatorText.text = text;
        }
    }
}
