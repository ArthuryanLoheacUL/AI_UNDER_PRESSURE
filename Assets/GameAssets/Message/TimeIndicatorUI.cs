using TMPro;
using UnityEngine;

public class TimeIndicatorUI : MonoBehaviour
{
    public TMP_Text timeIndicatorText;

    public void SetTimeIndicator()
    {
        var now = System.DateTime.Now;
        timeIndicatorText.text = now.Hour.ToString("D2") + ":" + now.Minute.ToString("D2") + ":" + now.Second.ToString("D2");
    }
}
