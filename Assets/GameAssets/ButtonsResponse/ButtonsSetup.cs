using System;
using UnityEngine;
using UnityEngine.UI;

public class ButtonsSetup : MonoBehaviour
{
    public Transform parentButtons;
    public GameObject buttonPrefab;

    public void SetupButtons(Prompt prompt, Action<int> onResponseSelected)
    {
        // Clear existing buttons
        foreach (Transform child in parentButtons)
        {
            Destroy(child.gameObject);
        }

        int _indexResponse = 0;
        // Create new buttons based on the prompt's response options
        foreach (var responseOption in prompt.responseOptions)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, parentButtons);
            ButtonResponseUI buttonResponseUI = buttonGO.GetComponent<ButtonResponseUI>();
            buttonResponseUI.SetResponse(responseOption.optionText, 0);
            buttonGO.GetComponent<Button>().onClick.AddListener(() => onResponseSelected(_indexResponse));
            _indexResponse++;
        }
    }

    public void Desactivates()
    {
        foreach (Transform child in parentButtons)
        {
            child.gameObject.GetComponent<Button>().interactable = false;
        }
    }
}
