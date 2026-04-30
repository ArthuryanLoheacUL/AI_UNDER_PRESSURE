using UnityEngine;
using System.Collections.Generic;
using System.Collections;

[RequireComponent(typeof(SetupPromptManagement))]
public class PromptsManager : MonoBehaviour
{
    public List<Prompt> prompts;

    private List<Prompt> queuePrompts = new List<Prompt>();
    private Prompt currentPrompt;

    private NotificationsManager notificationBar;
    private SetupPromptManagement setupPrompt;

    void Awake()
    {
        setupPrompt = GetComponent<SetupPromptManagement>();
        notificationBar = GameObject.FindGameObjectWithTag("NotificationsBar").GetComponent<NotificationsManager>();
    }

    public void AddPromptToQueue(Prompt prompt)
    {
        queuePrompts.Add(prompt);
    }

    public Prompt PopPromptFromQueue()
    {
        if (queuePrompts.Count > 0)
        {
            Prompt prompt = queuePrompts[0];
            queuePrompts.RemoveAt(0);
            return prompt;
        }
        return null;
    }

    public bool HasPromptsInQueue()
    {
        return queuePrompts.Count > 0;
    }

    public Prompt GetRandomPrompt()
    {
        if (prompts.Count > 0)
        {
            int randomIndex = Random.Range(0, prompts.Count);
            return prompts[randomIndex];
        }
        return null;
    }

    void Start()
    {
        for (int i = 0; i < 5; i++)
        {
            Prompt randomPrompt = GetRandomPrompt();
            if (randomPrompt != null)
            {
                AddPromptToQueue(randomPrompt);
            }
        }
        NextPrompt();
    }

    void RefreshNotifications(Prompt currentPrompt = null)
    {
        if (notificationBar != null)
        {
            List<Prompt> currentQueuePrompts = new List<Prompt>(queuePrompts);
            if (currentPrompt != null)
            {
                currentQueuePrompts.Insert(0, currentPrompt);
            }

            notificationBar.RefreshNotifications(currentQueuePrompts);
        }
    }

    void NextPrompt()
    {
        Prompt prompt = PopPromptFromQueue();
        if (prompt != null)
        {
            currentPrompt = prompt;
            setupPrompt.SetupPrompt(prompt, ResponseToCurrentPrompt);
        } else
        {
            prompt = GetRandomPrompt();
            if (prompt != null)
            {
                currentPrompt = prompt;
                setupPrompt.SetupPrompt(prompt, ResponseToCurrentPrompt);
            }
        }
        RefreshNotifications(prompt);
    }

    public void ResponseToCurrentPrompt(int selectedResponseIndex)
    {
        RessourceManager.Instance.UpdateRessource(currentPrompt.responseOptions[selectedResponseIndex].ressourceGain);
        RessourceManager.Instance.UpdateFrustration(currentPrompt.responseOptions[selectedResponseIndex].frustrationGain);

        StartCoroutine(NextPromptWithDelay(selectedResponseIndex)); // Delay before showing the next prompt
    }

    IEnumerator NextPromptWithDelay(int selectedResponseIndex = 0)
    {
        bool hasAIResponse = currentPrompt.responseOptions[selectedResponseIndex].responseAIText != "";
        bool hasUserResponse = currentPrompt.responseOptions[selectedResponseIndex].responseUserText != "";

        float _delay = 1.25f; // Adjust the delay as needed

        if (hasAIResponse)
        {
            setupPrompt.NextAIPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay);        
        }
        if (hasUserResponse)
        {
            setupPrompt.NextUserPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay);        
        }
        NextPrompt();
    }

}