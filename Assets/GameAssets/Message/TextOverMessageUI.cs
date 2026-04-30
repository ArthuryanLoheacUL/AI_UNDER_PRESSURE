using TMPro;
using UnityEngine;

public class TextOverMessageUI : MonoBehaviour
{
    public TMP_Text textOverMessageText;

    string normalText = "AAA >> ";

    public void SetTextOverMessage(string sender, bool isWaiting)
    {
        normalText = sender.ToUpper() + " >> ";
        textOverMessageText.text = normalText + (isWaiting ? "[EN ATTENTE]" : "");
    }

    public void SetNormalTextOverMessage()
    {
        textOverMessageText.text = normalText;
    }
}
