using UnityEngine;

[RequireComponent(typeof(TMPro.TMP_Text))]
public class DynamicSizeFont : MonoBehaviour
{
    private TMPro.TMP_Text tmpText;

    void Awake()
    {
        tmpText = GetComponent<TMPro.TMP_Text>();
    }

    void Update()
    {
        if (tmpText != null)
        {
            // Adjust font size based on the length of the text
            int length = tmpText.text.Length;
            if (length > 12)
            {
                // Decrease from 40 to 6 as length goes from 12 to 36 (arbitrary max length for scaling)
                tmpText.fontSize = Mathf.Lerp(40, 6, (length - 12) / 24f);
            }
            else
            {
                tmpText.fontSize = 40; // Default font size for short text
            }
        }
    }
}
