using UnityEngine;

public class SetupPromptManagement : MonoBehaviour
{
    private ButtonsSetup buttonsSetup;

    void Awake()
    {
        buttonsSetup = GameObject.FindGameObjectWithTag("ButtonsBar")?.GetComponent<ButtonsSetup>();
    }

    public void SetupPrompt(Prompt prompt)
    {
        if (buttonsSetup != null)
        {
            buttonsSetup.SetupButtons(prompt);
        } else {
            Debug.LogWarning("ButtonsSetup component not found. Cannot setup buttons for the prompt.");
        }
    }
}
