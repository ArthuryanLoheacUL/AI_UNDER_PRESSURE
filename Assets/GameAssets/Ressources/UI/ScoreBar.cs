using TMPro;
using UnityEngine;

public class ScoreBar : MonoBehaviour
{
    public TMP_Text text;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        setScore(0);
    }

    public void setScore(int score)
    {
        text.text = score.ToString(); 
    }
}
