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

    public void SetFrustration(float frustration)
    {
        targetFrustration = frustration;
        if (targetFrustration > sliderFG.value)
        {
            isUpping = true;
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
    }
}
