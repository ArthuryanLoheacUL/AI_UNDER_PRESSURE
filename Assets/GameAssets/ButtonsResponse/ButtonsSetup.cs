using UnityEngine;

public class ButtonsSetup : MonoBehaviour
{
    public Transform parentButtons;
    public GameObject buttonPrefab;

    public void SetupButtons(Prompt prompt)
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
            buttonResponseUI.SetResponse(responseOption.optionText, 0);
        }
    }
}
