using TMPro;
using UnityEngine;

public class TextOverMessageUI : MonoBehaviour
{
    public TMP_Text textOverMessageText;

    public void SetTextOverMessage(string sender, bool isWaiting)
    {
        textOverMessageText.text = sender.ToUpper() + " >> " + (isWaiting ? "[EN ATTENTE]" : "");
    }
}
