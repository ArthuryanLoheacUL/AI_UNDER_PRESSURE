using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class RessourceManager : MonoBehaviour
{
    public static RessourceManager Instance { get; private set; }

    [Header("Starting Values")]
    public int startingRessource = 60;
    public int startingBonheur = 60;

    [HideInInspector] public int ressourceValue = 60;

    // bonheurValue: 0..100. Game over when it drops to 0.
    [HideInInspector]
    [FormerlySerializedAs("frustrationValue")]
    public int bonheurValue = 60;

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

    void Start()
    {
        ressourceValue = startingRessource;
        bonheurValue = startingBonheur;
        UpdateRessource();
        UpdateBonheur();
    }

    public void UpdateRessource(int amount)
    {
        ressourceValue = Mathf.Clamp(ressourceValue + amount, 0, 100);
        UpdateRessource();
    }

    void UpdateRessource()
    {
        if (ressourceText != null)
            ressourceText.text = ressourceValue.ToString();
    }

    // Positive amount = gain bonheur (crew happier).
    // Negative amount = lose bonheur (game over closer).
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

    // -- Compatibility shim so older code paths don't immediately break.
    // GetFrustration() now mirrors the inverted value (100 - bonheur), so any
    // legacy reference that "boosts on high frustration" still behaves the same.
    public int GetFrustration()
    {
        return 100 - bonheurValue;
    }

    // Same idea for legacy frustration writers: a positive frustration delta
    // is interpreted as a bonheur LOSS.
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

    public bool IsBonheurDead()
    {
        return bonheurValue <= 0;
    }

    public bool IsRessourceDead()
    {
        return ressourceValue <= 0;
    }
}
