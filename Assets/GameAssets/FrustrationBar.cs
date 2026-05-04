using UnityEngine;
using UnityEngine.UI;

public class FrustrationBar : MonoBehaviour
{
    public Slider sliderBG;
    public Slider sliderFG;
    float targetFrustration = 0f;
    float animationSpeed = 0.2f;
    float animationSpeedBG = 1f;

    bool isUpping = false;

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

    public void SetFrustration(float frustration)
    {
        targetFrustration = frustration;
        if (targetFrustration > sliderFG.value)
        {
            isUpping = true;
            TriggerShake(); // Shake when frustration increases
        }
        else
        {
            isUpping = false;
        }
    }

    void Update()
    {
        if (isUpping)
        {
            if (sliderFG.value < targetFrustration)
            {
                sliderFG.value += Time.deltaTime * animationSpeed;
                if (sliderFG.value > targetFrustration)
                    sliderFG.value = targetFrustration;
            }
            else if (sliderFG.value > targetFrustration)
            {
                sliderFG.value -= Time.deltaTime * animationSpeed;
                if (sliderFG.value < targetFrustration)
                    sliderFG.value = targetFrustration;
            }
            if (sliderBG.value < targetFrustration)
            {
                sliderBG.value += Time.deltaTime * animationSpeedBG;
                if (sliderBG.value > targetFrustration)
                    sliderBG.value = targetFrustration;
            }
            else if (sliderBG.value > targetFrustration)
            {
                sliderBG.value -= Time.deltaTime * animationSpeedBG;
                if (sliderBG.value < targetFrustration)
                    sliderBG.value = targetFrustration;
            }   
        } else
        {
            if (sliderFG.value < targetFrustration)
            {
                sliderFG.value += Time.deltaTime * animationSpeedBG;
                if (sliderFG.value > targetFrustration)
                    sliderFG.value = targetFrustration;
            }
            else if (sliderFG.value > targetFrustration)
            {
                sliderFG.value -= Time.deltaTime * animationSpeedBG;
                if (sliderFG.value < targetFrustration)
                    sliderFG.value = targetFrustration;
            }
            if (sliderBG.value < targetFrustration)
            {
                sliderBG.value += Time.deltaTime * animationSpeed;
                if (sliderBG.value > targetFrustration)
                    sliderBG.value = targetFrustration;
            }
            else if (sliderBG.value > targetFrustration)
            {
                sliderBG.value -= Time.deltaTime * animationSpeed;
                if (sliderBG.value < targetFrustration)
                    sliderBG.value = targetFrustration;
            }  
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
}
