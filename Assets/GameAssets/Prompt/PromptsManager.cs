using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(SetupPromptManagement))]
public class PromptsManager : MonoBehaviour
{
    public List<Prompt> prompts;

    private List<Prompt> queuePrompts = new List<Prompt>();
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
        RefreshNotifications();
    }

    void RefreshNotifications()
    {
        if (notificationBar != null)
        {
            notificationBar.RefreshNotifications(queuePrompts);
        }
    }

    void NextPrompt()
    {
        Prompt prompt = PopPromptFromQueue();
        if (prompt != null)
        {
            setupPrompt.SetupPrompt(prompt);
        }
    }

}