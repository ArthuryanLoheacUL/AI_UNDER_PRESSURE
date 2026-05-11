using TMPro;
using UnityEngine;

public class ContinuerButton : MonoBehaviour
{
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        bool isInGame = PlayerPrefs.GetInt("InGame", 0) == 1;
        if (isInGame)
        {
            transform.GetChild(0).GetComponent<TMP_Text>().text = "[ CONTINUER ]";
        }
    }
}
