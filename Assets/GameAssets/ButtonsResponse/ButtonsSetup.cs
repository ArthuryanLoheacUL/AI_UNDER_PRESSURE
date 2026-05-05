using System;
using Unity.VectorGraphics;
using UnityEngine;
using UnityEngine.SceneManagement;
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

    public void SetupGameOverButtons()
    {
        // Clear existing buttons
        foreach (Transform child in parentButtons)
        {
            Destroy(child.gameObject);
        }

        // Create a single "Menu" button
        GameObject buttonGO1 = Instantiate(buttonPrefab, parentButtons);
        ButtonResponseUI buttonResponseUI1 = buttonGO1.GetComponent<ButtonResponseUI>();
        buttonResponseUI1.SetResponse("Menu", 0);
        buttonGO1.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene("Menu"));
    
        // Create a single "Restart" button
        GameObject buttonGO2 = Instantiate(buttonPrefab, parentButtons);
        ButtonResponseUI buttonResponseUI2 = buttonGO2.GetComponent<ButtonResponseUI>();
        buttonResponseUI2.SetResponse("Restart", 0);
        buttonGO2.GetComponent<Button>().onClick.AddListener(() => SceneManager.LoadScene("Game"));
    }
}
