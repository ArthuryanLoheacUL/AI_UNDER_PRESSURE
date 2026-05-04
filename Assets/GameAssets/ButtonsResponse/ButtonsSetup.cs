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

        // Create new buttons based on the prompt's response options
        foreach (var responseOption in prompt.responseOptions)
        {
            GameObject buttonGO = Instantiate(buttonPrefab, parentButtons);
            ButtonResponseUI buttonResponseUI = buttonGO.GetComponent<ButtonResponseUI>();
            buttonResponseUI.SetResponse(responseOption.optionText, responseOption.ressourceGain);
            buttonGO.GetComponent<Button>().onClick.AddListener(() => onResponseSelected(Array.IndexOf(prompt.responseOptions, responseOption)));
            if (responseOption.ressourceGain > 0)
                buttonGO.GetComponent<Button>().interactable = true;
            else
                buttonGO.GetComponent<Button>().interactable = RessourceManager.Instance.ressourceValue >= -responseOption.ressourceGain;
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
