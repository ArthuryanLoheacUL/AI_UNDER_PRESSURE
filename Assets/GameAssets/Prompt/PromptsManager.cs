using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.VisualScripting.FullSerializer;
using System.Linq;

[RequireComponent(typeof(SetupPromptManagement))]
public class PromptsManager : MonoBehaviour
{
    [System.Serializable]
    public struct WeightedPrompt
    {
        public Prompt prompt;
        public float weight;

        public WeightedPrompt(Prompt prompt, float weight = 5f)
        {
            this.prompt = prompt;
            this.weight = weight;
        }
    }

    public struct PromptSaved
    {
        public string time;
        public string senderMessage;
        public string IaMessage;
        public string userResponse;
        public string senderName;
    }

    public List<Prompt> prompts;
    private List<WeightedPrompt> promptsWithWeights = new List<WeightedPrompt>();
    private Dictionary<string, List<PromptSaved>> savedPrompts = new Dictionary<string, List<PromptSaved>>();

    private List<Prompt> queuePrompts = new List<Prompt>();
    private List<Prompt> prevPrompts = new List<Prompt>();

    private Prompt currentPrompt;

    private NotificationsManager notificationBar;
    private SetupPromptManagement setupPrompt;

    private float timeNextPrompt = 0f;
    private float timeBetweenPrompts = 10f;
    public int startPrompts = 1;

    bool isGameOver = false;

    void Awake()
    {
        setupPrompt = GetComponent<SetupPromptManagement>();
        notificationBar = GameObject.FindGameObjectWithTag("NotificationsBar").GetComponent<NotificationsManager>();
        Time.timeScale = 1f; // Ensure the game is not paused at the start
    }

    void NotifyNewPrompt(Prompt prompt)
    {
        SoundEffectManager.Instance.PlaySoundEffectRandomPitch("Notif");
        if (prompt.isUrgent)
        {
            SoundEffectManager.Instance.PlaySoundEffectRandomPitch("TimerClicks");
        }
    }

    void ShufflePromptResponseOptions(Prompt prompt)
    {
        if (prompt == null || prompt.responseOptions == null || prompt.responseOptions.Count() <= 1)
            return;

        for (int i = prompt.responseOptions.Count() - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            var temp = prompt.responseOptions[i];
            prompt.responseOptions[i] = prompt.responseOptions[swapIndex];
            prompt.responseOptions[swapIndex] = temp;
        }
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
            promptsWithWeights.RemoveAll(p => p.prompt.message == prompt.message && p.prompt.senderName == prompt.senderName);
        }
        prevPrompts.Add(prompt);
        if (prevPrompts.Count > 5)        {
            prevPrompts.RemoveAt(0);
        }
        ShufflePromptResponseOptions(prompt);
        NotifyNewPrompt(prompt);
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
        if (promptsWithWeights.Count > 0)
        {
            // Filter out prompts from the last 5 played prompts
            List<Prompt> availablePrompts = new List<Prompt>();
            foreach (var weightedPrompt in promptsWithWeights)
            {
                bool isInRecent = false;
                foreach (var recentPrompt in prevPrompts)
                {
                    if (weightedPrompt.prompt.message == recentPrompt.message && weightedPrompt.prompt.senderName == recentPrompt.senderName)
                    {
                        isInRecent = true;
                        break;
                    }
                }
                if (!isInRecent)
                {
                    for (int i = 0; i < Mathf.RoundToInt(weightedPrompt.weight); i++)
                    {
                        availablePrompts.Add(weightedPrompt.prompt);
                    }
                }
            }

            // If all prompts are in recent, allow all prompts
            if (availablePrompts.Count == 0)
            {
                foreach (var weightedPrompt in promptsWithWeights)
                {
                    for (int i = 0; i < Mathf.RoundToInt(weightedPrompt.weight); i++)
                    {
                        availablePrompts.Add(weightedPrompt.prompt);
                    }
                }
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

            ShufflePromptResponseOptions(randomPrompt);
            return randomPrompt;
        }
        return null;
    }

    void Start()
    {
        foreach (var prompt in prompts)
        {
            promptsWithWeights.Add(new WeightedPrompt(prompt, 1));
        }

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

    void SaveCurrentPrompt()
    {
        if (currentPrompt != null)
        {
            string key = currentPrompt.senderName;
            if (!savedPrompts.ContainsKey(key))
            {
                savedPrompts[key] = new List<PromptSaved>();
            }
            PromptSaved promptSaved = new PromptSaved
            {
                time = System.DateTime.Now.ToString("HH:mm:ss"),
                senderName = currentPrompt.senderName,
                senderMessage = currentPrompt.message,
                IaMessage = currentPrompt.responseOptions.Length > 0 ? currentPrompt.responseOptions[0].optionText : "",
                userResponse = currentPrompt.responseOptions.Length > 0 ? currentPrompt.responseOptions[0].responseUserText : ""
            };
            savedPrompts[key].Add(promptSaved);
        }
    }

    List<PromptSaved> GetSavedPromptsForSender(string senderName)
    {
        if (savedPrompts.ContainsKey(senderName))
        {
            return savedPrompts[senderName];
        }
        return null;
    }

    void NextPrompt()
    {
        // Save current prompt to saves
        SaveCurrentPrompt();

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
            setupPrompt.SetupPrompt(prompt, ResponseToCurrentPrompt, GetSavedPromptsForSender(prompt.senderName));
        } else
        {
            prompt = GetRandomPrompt();
            if (prompt != null)
            {
                currentPrompt = prompt;
                setupPrompt.SetupPrompt(prompt, ResponseToCurrentPrompt, GetSavedPromptsForSender(prompt.senderName));
            }
        }
        RefreshNotifications();
    }

    public void ResponseToCurrentPrompt(int selectedResponseIndex)
    {
        RessourceManager.Instance.UpdateRessource(currentPrompt.responseOptions[selectedResponseIndex].ressourceGain);
        RessourceManager.Instance.UpdateFrustration(currentPrompt.responseOptions[selectedResponseIndex].bonheurGain);

        if (currentPrompt.responseOptions[selectedResponseIndex].addedPrompts != null)
        {
            foreach (var addedPrompt in currentPrompt.responseOptions[selectedResponseIndex].addedPrompts)
            {
                promptsWithWeights.Add(addedPrompt);
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
        if (RessourceManager.Instance.GetFrustration() >= 100 || currentPrompt.responseOptions[selectedResponseIndex].isGameOverResponse)
        {
            isGameOver = true;
            Time.timeScale = 0f; // Freeze the game
            setupPrompt.SetupGameOverButtons();
            yield break;
        } else
        {
            NextPrompt();
        }
    }

    void Update()
    {
        if (isGameOver)
            return;
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