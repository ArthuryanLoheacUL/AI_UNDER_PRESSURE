using UnityEngine;

public class ShakeRessourceBar : MonoBehaviour
{
    // Shake variables
    private bool isShaking = false;
    private float shakeDuration = 0.3f;
    private float shakeIntensity = 5f;
    private float shakeTimer = 0f;
    private Vector3 originalPosition;
    public void TriggerShake()
    {
        isShaking = true;
        shakeTimer = shakeDuration;
    }

    void Start()
    {
        originalPosition = transform.GetChild(0).localPosition;
    }

    
    void Update()
    {
        // Apply shake effect
        if (isShaking)
        {
            shakeTimer -= Time.deltaTime;
            if (shakeTimer <= 0f)
            {
                isShaking = false;
                transform.GetChild(0).localPosition = originalPosition;
            }
            else
            {
                float shakeAmount = shakeIntensity * (shakeTimer / shakeDuration);
                float shakeX = Random.Range(-shakeAmount, shakeAmount);
                float shakeY = Random.Range(-shakeAmount, shakeAmount);
                transform.GetChild(0).localPosition = originalPosition + new Vector3(shakeX, shakeY, 0f);
            }
        }
    }
}
