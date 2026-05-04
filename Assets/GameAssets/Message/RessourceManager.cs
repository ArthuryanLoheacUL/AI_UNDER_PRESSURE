using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class RessourceManager : MonoBehaviour
{
    public static RessourceManager Instance { get; private set; }

    [HideInInspector] public int ressourceValue = 50;
    [HideInInspector] public int frustrationValue = 0;

    public FrustrationBar frustrationSlider;
    public TMP_Text frustrationText;
    public TMP_Text ressourceText;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
    }

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

    public void UpdateFrustration(int amount)
    {
        frustrationValue = Mathf.Clamp(frustrationValue + amount, 0, 100);
        UpdateFrustration();
    }

    void UpdateFrustration()
    {
        frustrationSlider.SetFrustration(frustrationValue / 100f);
        frustrationText.text = frustrationValue.ToString() + "%";
    }

    public int GetFrustration()
    {
        return frustrationValue;
    }

    public int GetRessource()
    {
        return ressourceValue;
    }
}
