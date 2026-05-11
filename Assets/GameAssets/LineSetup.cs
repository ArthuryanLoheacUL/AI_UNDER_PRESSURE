using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LineSetup : MonoBehaviour
{
    public Slider slider;
    public TMP_Text title;
    public TMP_Text titleNumber;

    public void SetupLine(int nb, int nbMax, string name)
    {
        if (nb > 0)
        {
            slider.value = (float)nb / (float)nbMax;
            title.text = name;
            titleNumber.text = nb.ToString() + "/" + nbMax.ToString();
        } else
        {
            slider.value = 0;
            title.text = "????";
            titleNumber.text = "0/??";
        }
    }
}
