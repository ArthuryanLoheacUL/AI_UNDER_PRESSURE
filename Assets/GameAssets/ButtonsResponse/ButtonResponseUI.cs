using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ButtonResponseUI : MonoBehaviour
{
    public TMP_Text responseText;
    public GameObject valueIndicator;
    public RawImage valueIndicatorSprite;
    public TMP_Text valueIndicatorText;
    public Color colorNegative;

    public void SetResponse(string response, int value)
    {
        responseText.text = "[ " + response + " ]";

        if (value != 0)
        {
            valueIndicator.SetActive(true);
            valueIndicatorText.text = value > 0 ? $"+{value}" : value.ToString();
            if (value < 0)
            {
                valueIndicatorText.color = colorNegative;
                valueIndicatorSprite.color = colorNegative;
            }
        }
        else
        {
            valueIndicator.SetActive(false);
        }
    }
}
