using TMPro;
using UnityEngine;

public class HighScore : MonoBehaviour
{
    public TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    public void SetHighScore()
    {
        text.text = PlayerPrefs.GetInt("HighScore", 0).ToString();   
    }
    void Start()
    {
        SetHighScore();
    }
}
