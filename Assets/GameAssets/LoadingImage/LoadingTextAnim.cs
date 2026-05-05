using UnityEngine;

public class LoadingTextAnim : MonoBehaviour
{
    float timer = 0f;
    int dotCount = 0;

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= 0.5f)
        {
            timer = 0f;
            dotCount = (dotCount + 1) % 4; // Cycle through 0, 1, 2, 3
            GetComponent<TMPro.TMP_Text>().text = "Loading" + new string('.', dotCount);
        }
    }
}
