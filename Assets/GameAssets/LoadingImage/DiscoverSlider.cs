using System.Linq;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class DiscoverSlider : MonoBehaviour
{
    public Slider slider;
    public TMP_Text text;

    public Prompt[] allCards; 

    void Start()
    {
        int tt = allCards.Count();
        int get = 0;
        foreach(Prompt p in allCards)
        {
            get += PlayerPrefs.GetInt(p.senderName + p.message.Truncate(5) + p.message.Count().ToString(), 0);
        }
        slider.value = (float)get / (float)tt;
        text.text = $"{get}/{tt}";
    }
}
