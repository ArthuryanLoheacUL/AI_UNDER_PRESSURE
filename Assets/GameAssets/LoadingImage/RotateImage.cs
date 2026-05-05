using Microsoft.Unity.VisualStudio.Editor;
using UnityEngine;

public class RotateImage : MonoBehaviour
{

    void Update()
    {
        GetComponent<RectTransform>().Rotate(0f, 0f, -100f * Time.deltaTime);
    }
}
