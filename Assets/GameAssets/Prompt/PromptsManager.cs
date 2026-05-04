using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting.FullSerializer;

[RequireComponent(typeof(SetupPromptManagement))]
public class PromptsManager : MonoBehaviour
{
    public List<Prompt> prompts;

    private List<Prompt> queuePrompts = new List<Prompt>();
    private List<Prompt> prevPrompts = new List<Prompt>();

    private Prompt currentPrompt;

    private NotificationsManager notificationBar;
    private SetupPromptManagement setupPrompt;

    private float timeNextPrompt = 0f;
    private float timeBetweenPrompts = 10f;
    public int startPrompts = 1;

    void Awake()
    {
        setupPrompt = GetComponent<SetupPromptManagement>();
        notificationBar = GameObject.FindGameObjectWithTag("NotificationsBar").GetComponent<NotificationsManager>();
    }

    void NotifyNewPrompt()
    {
        SoundEffectManager.Instance.PlaySoundEffectRandomPitch("Notif");
    }

    public void AddPromptToQueue(Prompt prompt)
    {
        if (prompt.isUrgent)
        {
            queuePrompts.Insert(0, prompt);
        } else
        {
            queuePrompts.Add(prompt);        
        }
        if (prompt.isUnique)
        {
            prompts.RemoveAll(p => p.message == prompt.message && p.senderName == prompt.senderName);
        }
        NotifyNewPrompt();
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
            // Filter out prompts from the last 5 played prompts
            List<Prompt> availablePrompts = new List<Prompt>();
            foreach (var prompt in prompts)
            {
                bool isInRecent = false;
                foreach (var recentPrompt in prevPrompts)
                {
                    if (prompt.message == recentPrompt.message && prompt.senderName == recentPrompt.senderName)
                    {
                        isInRecent = true;
                        break;
                    }
                }
                if (!isInRecent)
                {
                    availablePrompts.Add(prompt);
                }
            }

            // If all prompts are in recent, allow all prompts
            if (availablePrompts.Count == 0)
            {
                availablePrompts = new List<Prompt>(prompts);
            }

            int randomIndex = Random.Range(0, availablePrompts.Count);
            Prompt randomPrompt = new Prompt(availablePrompts[randomIndex]);
            randomPrompt.timerMax = 30f + Random.Range(-5f, 10f);
            randomPrompt.timer = randomPrompt.timerMax;

            // Chance to make prompt urgent based on current frustration level
            float frustrationChance = RessourceManager.Instance.GetFrustration() / 100f; // Assuming frustration is 0-100
            if (Random.value < frustrationChance)
            {
                randomPrompt.isUrgent = true;
            }

            return randomPrompt;
        }
        return null;
    }

    void Start()
    {
        for (int i = 0; i < startPrompts; i++)
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
        // Add current prompt to recent prompts history (max 5)
        if (currentPrompt != null)
        {
            prevPrompts.Insert(0, currentPrompt);
            if (prevPrompts.Count > 5)
            {
                prevPrompts.RemoveAt(prevPrompts.Count - 1);
            }
        }

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

        if (currentPrompt.responseOptions[selectedResponseIndex].addedPrompts != null)
        {
            foreach (var addedPrompt in currentPrompt.responseOptions[selectedResponseIndex].addedPrompts)
            {
                prompts.Add(addedPrompt);
            }
        }
        if (currentPrompt.responseOptions[selectedResponseIndex].addedDirectPrompts != null)
        {
            foreach (var addedDirectPrompt in currentPrompt.responseOptions[selectedResponseIndex].addedDirectPrompts)
            {
                AddPromptToQueue(addedDirectPrompt);
            }
        }

        StartCoroutine(NextPromptWithDelay(selectedResponseIndex)); // Delay before showing the next prompt
    }

    IEnumerator NextPromptWithDelay(int selectedResponseIndex = 0)
    {
        bool hasAIResponse = currentPrompt.responseOptions[selectedResponseIndex].optionText != "";
        bool hasUserResponse = currentPrompt.responseOptions[selectedResponseIndex].responseUserText != "";
        bool hasBoth = hasAIResponse && hasUserResponse;

        float _delay = 1.25f; // Adjust the delay as needed

        if (hasAIResponse)
        {
            setupPrompt.NextAIPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay - (hasBoth ? 0.4f : 0f));        
        }
        if (hasUserResponse)
        {
            setupPrompt.NextUserPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay + (hasBoth ? 0.4f : 0f));        
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
            if (queuePrompts[i].timer <= 0f && queuePrompts[i].isUrgent)
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
            if (currentPrompt.timer <= 0f && currentPrompt.isUrgent)
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