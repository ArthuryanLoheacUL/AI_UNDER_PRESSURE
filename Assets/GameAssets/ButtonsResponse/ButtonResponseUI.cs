using TMPro;
using UnityEngine;

public class ButtonResponseUI : MonoBehaviour
{
    public TMP_Text responseText;
    public GameObject valueIndicator;
    public TMP_Text valueIndicatorText;

    public void SetResponse(string response, int value)
    {
        responseText.text = "[ " + response + " ]";

        if (value != 0)
        {
            valueIndicator.SetActive(true);
            valueIndicatorText.text = value > 0 ? $"+{value}" : value.ToString();
        }
        else
        {
            valueIndicator.SetActive(false);
        }
    }
}
