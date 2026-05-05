using UnityEngine;
using UnityEngine.UI;

// NOTE: kept the class name FrustrationBar for compatibility with existing
// scene references (Game.unity references this script's GUID). It now drives a
// "bonheur" bar where 1.0 = full bonheur (good) and 0.0 = empty bonheur (game
// over). The shake triggers when bonheur DROPS (bad news).
public class FrustrationBar : MonoBehaviour
{
    public Slider sliderBG;
    public Slider sliderFG;
    float targetValue = 1f;
    float animationSpeed = 0.2f;
    float animationSpeedBG = 1f;

    bool isDropping = false;

    // Shake variables
    private bool isShaking = false;
    private float shakeDuration = 0.3f;
    private float shakeIntensity = 5f;
    private float shakeTimer = 0f;
    private Vector3 originalPosition;

    void Start()
    {
        originalPosition = transform.localPosition;
    }

    public void TriggerShake()
    {
        isShaking = true;
        shakeTimer = shakeDuration;
    }

    public void SetBonheur(float bonheur01)
    {
        // bonheur01: 0..1 where 1 is full bonheur.
        if (bonheur01 < targetValue)
        {
            // bonheur is decreasing -> shake (bad news for the AI).
            isDropping = true;
            TriggerShake();
        }
        else
        {
            isDropping = false;
        }
        targetValue = bonheur01;
    }

    // Backwards-compatible shim. Old callers passed a "frustration" 0..1 where
    // 1 was BAD. Convert to bonheur and forward.
    public void SetFrustration(float frustration01)
    {
        SetBonheur(1f - frustration01);
    }

    void Update()
    {
        // Pick which slider leads / trails based on direction of change.
        if (isDropping)
        {
            // FG snaps quickly down, BG drains slowly.
            MoveSlider(sliderFG, targetValue, animationSpeedBG);
            MoveSlider(sliderBG, targetValue, animationSpeed);
        }
        else
        {
            // BG fills quickly up, FG fills slowly behind it.
            MoveSlider(sliderFG, targetValue, animationSpeed);
            MoveSlider(sliderBG, targetValue, animationSpeedBG);
        }

        // Apply shake effect
        if (isShaking)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                isShaking = false;
                transform.localPosition = originalPosition;
            }
            else
            {
                float shakeAmount = shakeIntensity * (shakeTimer / shakeDuration);
                float shakeX = Random.Range(-shakeAmount, shakeAmount);
                float shakeY = Random.Range(-shakeAmount, shakeAmount);
                transform.localPosition = originalPosition + new Vector3(shakeX, shakeY, 0f);
            }
        }
    }

    void MoveSlider(Slider s, float target, float speed)
    {
        if (s == null) return;
        if (Mathf.Abs(s.value - target) < 0.0001f) return;
        if (s.value < target)
        {
            s.value = Mathf.Min(target, s.value + Time.deltaTime * speed);
        }
        else
        {
            s.value = Mathf.Max(target, s.value - Time.deltaTime * speed);
        }
    }
}
