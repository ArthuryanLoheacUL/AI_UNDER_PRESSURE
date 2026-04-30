using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting.FullSerializer;

[RequireComponent(typeof(SetupPromptManagement))]
public class PromptsManager : MonoBehaviour
{
    public List<Prompt> prompts;

    private List<Prompt> queuePrompts = new List<Prompt>();
    private Prompt currentPrompt;

    private NotificationsManager notificationBar;
    private SetupPromptManagement setupPrompt;

    private float timeNextPrompt = 0f;
    private float timeBetweenPrompts = 10f;

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
            Prompt randomPrompt = new Prompt(prompts[randomIndex]);
            randomPrompt.timerMax = 30f + Random.Range(-5f, 10f);
            randomPrompt.timer = randomPrompt.timerMax;
            return randomPrompt;
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
        timeNextPrompt = timeBetweenPrompts;
    }

    void RefreshNotifications()
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
        RefreshNotifications();
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

    void Update()
    {
        CheckOutdatedPrompts();
        timeNextPrompt -= Time.deltaTime;
        if (timeNextPrompt <= 0f)
        {
            timeNextPrompt = timeBetweenPrompts;
            Prompt randomPrompt = GetRandomPrompt();
            if (randomPrompt != null)
            {
                AddPromptToQueue(randomPrompt);
                RefreshNotifications();
            }
        }
    }

    void CheckOutdatedPrompts()
    {
        bool edited = false;
        for (int i = 0; i < queuePrompts.Count; i++)
        {
            queuePrompts[i].timer -= Time.deltaTime;
            if (queuePrompts[i].timer <= 0f)
            {
                queuePrompts.RemoveAt(i);
                RessourceManager.Instance.UpdateFrustration(10);
                i--;
                edited = true;
            }
        }
        if (currentPrompt != null)
        {
            currentPrompt.timer -= Time.deltaTime;
            if (currentPrompt.timer <= 0f)
            {
                currentPrompt = null;
                RessourceManager.Instance.UpdateFrustration(10);
                edited = true;
            }
        }
        if (currentPrompt == null)
        {
            NextPrompt();
        }
        if (edited)
            RefreshNotifications();
    }

    void SetMinimumQueuePrompts(int minimum)
    {
        for (int i = queuePrompts.Count; i < minimum; i++)
        {
            Prompt randomPrompt = GetRandomPrompt();
            if (randomPrompt != null)
            {
                AddPromptToQueue(randomPrompt);
            }
        }
    }

}