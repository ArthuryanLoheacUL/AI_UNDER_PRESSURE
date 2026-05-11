using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RessourceManager : MonoBehaviour
{
    public static RessourceManager Instance { get; private set; }

    [Header("Starting Values")]
    public int startingRessource = 30;
    public int startingBonheur = 50;

    [HideInInspector] public int ressourceValue = 30;

    // bonheurValue: 0..100. Game over when it drops to 0.
    [HideInInspector]
    [FormerlySerializedAs("frustrationValue")]
    public int bonheurValue = 50;

    [Header("UI Refs")]
    [FormerlySerializedAs("frustrationSlider")]
    public FrustrationBar bonheurSlider;
    [FormerlySerializedAs("frustrationText")]
    public TMP_Text bonheurText;
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

    public void SetDefault()
    {
        bonheurValue = startingBonheur;
        ressourceValue = startingRessource;
        UpdateRessource();
        UpdateBonheur();
    }

    void Start()
    {
        UpdateRessource();
        UpdateBonheur();
    }

    public void UpdateRessource(int amount)
    {
        ressourceValue = Mathf.Clamp(ressourceValue + amount, 0, 100);
        UpdateRessource();
        ressourceText.transform.parent.parent.GetComponent<ShakeRessourceBar>().TriggerShake(amount);
    }

    void UpdateRessource()
    {
        if (ressourceText != null)
            ressourceText.text = ressourceValue.ToString();
    }

    public void UpdateBonheur(int amount)
    {
        bonheurValue = Mathf.Clamp(bonheurValue + amount, 0, 100);
        UpdateBonheur();
    }

    void UpdateBonheur()
    {
        if (bonheurSlider != null)
            bonheurSlider.SetBonheur(bonheurValue / 100f);
        if (bonheurText != null)
            bonheurText.text = bonheurValue.ToString() + "%";
    }

    public int GetFrustration()
    {
        return 100 - bonheurValue;
    }

    public void UpdateFrustration(int amount)
    {
        UpdateBonheur(-amount);
    }

    public int GetBonheur()
    {
        return bonheurValue;
    }

    public int GetRessource()
    {
        return ressourceValue;
    }

    public void SetBonheur(int b)
    {
        bonheurValue = b;
        UpdateBonheur();
    }

    public void SetRessource(int r)
    {
        ressourceValue = r;
        UpdateRessource();
    }

    public bool IsBonheurDead()
    {
        return bonheurValue <= 0;
    }

    public bool IsRessourceDead()
    {
        return ressourceValue <= 0;
    }
}
