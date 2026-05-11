using UnityEngine;

public class RotateImageButton : MonoBehaviour
{
    public Transform transformButton;

    void Start()
    {
    }

    public void RotateImage()
    {
        transformButton.Rotate(0, 0, 180);
    }
}
