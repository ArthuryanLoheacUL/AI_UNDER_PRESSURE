using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RessourceManager : MonoBehaviour
{
    [HideInInspector] public int ressourceValue = 50;
    [HideInInspector] public int frustrationValue = 0;

    public Slider frustrationSlider;
    public TMP_Text frustrationText;
    public TMP_Text ressourceText;

    void Start()
    {
        // Initial setup if needed
        ressourceValue = 50;
        frustrationValue = 0;
        UpdateRessource();
        UpdateFrustration();
    }

    public void UpdateRessource(int amount)
    {
        ressourceValue += amount;
        UpdateRessource();
    }

    void UpdateRessource()
    {
        ressourceText.text = ressourceValue.ToString();
    }

    void UpdateFrustration()
    {
        frustrationSlider.value = frustrationValue / 100f;
        frustrationText.text = frustrationValue.ToString() + "%";
    }

    public void UpdateFrustration(int amount)
    {
        frustrationValue = Mathf.Clamp(frustrationValue + amount, 0, 100);
        UpdateFrustration();
    }
}
