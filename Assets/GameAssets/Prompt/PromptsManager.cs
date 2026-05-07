using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;

[RequireComponent(typeof(SetupPromptManagement))]
public class PromptsManager : MonoBehaviour
{
    [System.Serializable]
    public struct WeightedPrompt
    {
        public Prompt prompt;
        public float weight;

        public WeightedPrompt(Prompt prompt, float weight = 1f)
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

    [Header("Initial pool (assigned via Inspector)")]
    public List<Prompt> prompts;

    [Header("Pacing")]
    public int startPrompts = 1;
    public float timeBetweenPrompts = 10f;

    private List<WeightedPrompt> promptsWithWeights = new List<WeightedPrompt>();
    private Dictionary<string, List<PromptSaved>> savedPrompts = new Dictionary<string, List<PromptSaved>>();

    private List<Prompt> queuePrompts = new List<Prompt>();
    private List<Prompt> prevPrompts = new List<Prompt>();
    private List<Prompt> removedPrompts = new List<Prompt>();

    // Tracks how many bonheur-negative-only prompts we've stacked in a row,
    // used to inject relief when the player has been hammered too long.
    private int consecutiveNegativePrompts = 0;
    // Cap on consecutive prompts that drain BOTH gauges (rule: never > 3 in a row).
    private int consecutiveDoubleDrainPrompts = 0;

    // Active narrative arcs - prompts whose chainTag has been pulled at least once.
    private HashSet<string> activeChains = new HashSet<string>();

    private Prompt currentPrompt;
    private NotificationsManager notificationBar;
    private SetupPromptManagement setupPrompt;

    public GameObject loadingScreen;
    public float loadingScreenDuration = 1f;

    private float timeNextPrompt = 0f;
    private bool isGameOver = false;

    void Awake()
    {
        setupPrompt = GetComponent<SetupPromptManagement>();
        var notifGO = GameObject.FindGameObjectWithTag("NotificationsBar");
        if (notifGO != null)
            notificationBar = notifGO.GetComponent<NotificationsManager>();
        Time.timeScale = 1f;
    }

    void NotifyNewPrompt(Prompt prompt)
    {
        SoundEffectManager.Instance.PlaySoundEffectRandomPitch("Notif");
        // if (prompt.isUrgent)
        //     SoundEffectManager.Instance.PlaySoundEffectRandomPitch("TimerClicks");
    }

    void ShufflePromptResponseOptions(Prompt prompt)
    {
        if (prompt == null || prompt.responseOptions == null || prompt.responseOptions.Length <= 1)
            return;

        for (int i = prompt.responseOptions.Length - 1; i > 0; i--)
        {
            int swapIndex = Random.Range(0, i + 1);
            var temp = prompt.responseOptions[i];
            prompt.responseOptions[i] = prompt.responseOptions[swapIndex];
            prompt.responseOptions[swapIndex] = temp;
        }
    }

    static bool SameIdentity(Prompt a, Prompt b)
    {
        if (a == null || b == null) return false;
        return a.senderName == b.senderName && a.message == b.message;
    }

    public bool AddPromptToQueue(Prompt prompt, bool directAdd = false)
    {
        foreach (Prompt p in removedPrompts)
        {
            if (SameIdentity(p, prompt))
                return false;
        }

        if (prompt.isUrgent || directAdd)
            queuePrompts.Insert(0, prompt);
        else
            queuePrompts.Add(prompt);

        if (prompt.isUnique)
        {
            promptsWithWeights.RemoveAll(p => SameIdentity(p.prompt, prompt));
            removedPrompts.Add(prompt);
        }

        prevPrompts.Add(prompt);
        if (prevPrompts.Count > 5)
            prevPrompts.RemoveAt(0);

        ShufflePromptResponseOptions(prompt);
        NotifyNewPrompt(prompt);
        return true;
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

    // -------------------- Removal API --------------------
    public void RemovePrompts(Prompt[] toRemove)
    {
        if (toRemove == null) return;
        foreach (var p in toRemove)
        {
            if (p == null) continue;
            promptsWithWeights.RemoveAll(wp => SameIdentity(wp.prompt, p));
            queuePrompts.RemoveAll(q => SameIdentity(q, p));
            removedPrompts.Add(p);
        }
    }

    // Contextual weighting based on the current state of the gauges and
    // recent pacing.
    float GetContextualWeight(WeightedPrompt wp)
    {
        if (wp.prompt == null) return 0f;
        float weight = wp.weight <= 0f ? 1f : wp.weight;

        int ressources = RessourceManager.Instance != null
            ? RessourceManager.Instance.GetRessource() : 60;
        int bonheur = RessourceManager.Instance != null
            ? RessourceManager.Instance.GetBonheur() : 60;

        // Boost urgent prompts when something is on fire.
        if (wp.prompt.isUrgent && (ressources < 30 || bonheur < 35))
            weight *= 2.5f;

        // Damp crisis prompts when everything is fine.
        if (wp.prompt.isCrisis && ressources > 60 && bonheur > 55)
            weight *= 0.3f;

        // After three consecutive negative prompts, push relief HARD.
        if (consecutiveNegativePrompts >= 3 && wp.prompt.isRelief)
            weight *= 3f;

        // Hard cap: never serve a 4th double-drain in a row.
        if (consecutiveDoubleDrainPrompts >= 3 && IsDoubleDrain(wp.prompt))
            weight *= 0.05f;

        // if cant choose an option
        int doable = wp.prompt.responseOptions.Count();
        foreach (Prompt.ResponseOption r in wp.prompt.responseOptions)
            if (ressources < -r.ressourceGain) doable--;
        if (doable == 0)
            weight = 0;

        return weight;
    }

    bool IsDoubleDrain(Prompt p)
    {
        if (p == null || p.responseOptions == null || p.responseOptions.Length == 0) return false;
        foreach (var opt in p.responseOptions)
        {
            if (opt.ressourceGain >= 0)
                return false;
        }
        return true;
    }

    public Prompt GetRandomPrompt()
    {
        if (promptsWithWeights.Count == 0) return null;

        // Filter out prompts from the recent buffer to avoid immediate repeats.
        List<WeightedPrompt> candidates = new List<WeightedPrompt>();
        foreach (var wp in promptsWithWeights)
        {
            if (wp.prompt == null) continue;
            bool inRecent = false;
            foreach (var recent in prevPrompts)
            {
                if (wp.prompt.message == recent.message)
                {
                    inRecent = true;
                    break;
                }
            }
            if (!inRecent) candidates.Add(wp);
        }
        // If everything is recent, fall back to the full pool.
        if (candidates.Count == 0)
            candidates = new List<WeightedPrompt>(promptsWithWeights);

        // Weighted roulette selection.
        float total = 0f;
        float[] effective = new float[candidates.Count];
        for (int i = 0; i < candidates.Count; i++)
        {
            effective[i] = GetContextualWeight(candidates[i]);
            total += effective[i];
        }

        Prompt picked = null;
        if (total <= 0f)
        {
            picked = candidates[Random.Range(0, candidates.Count)].prompt;
        }
        else
        {
            float roll = Random.value * total;
            float cumulative = 0f;
            for (int i = 0; i < candidates.Count; i++)
            {
                cumulative += effective[i];
                if (roll <= cumulative)
                {
                    picked = candidates[i].prompt;
                    break;
                }
            }
            if (picked == null)
                picked = candidates[candidates.Count - 1].prompt;
        }

        Prompt instance = new Prompt(picked);
        instance.timerMax = 30f + Random.Range(-5f, 10f);
        instance.timer = instance.timerMax;

        // Chance to upgrade a prompt to urgent if the situation is already tense.
        float pressureChance = (100 - RessourceManager.Instance.bonheurValue) / 100f;
        if (Random.value < pressureChance) instance.isUrgent = false;

        ShufflePromptResponseOptions(instance);
        return instance;
    }

    void Start()
    {
        promptsWithWeights.Clear();
        foreach (var prompt in prompts)
        {
            if (prompt == null) continue;
            promptsWithWeights.Add(new WeightedPrompt(prompt, 1));
        }

        for (int i = 0; i < startPrompts; i++)
        {
            Prompt randomPrompt = GetRandomPrompt();
            if (randomPrompt != null) AddPromptToQueue(randomPrompt);
        }
        NextPrompt();
        timeNextPrompt = timeBetweenPrompts;
    }

    void RefreshNotifications()
    {
        if (notificationBar == null) return;
        List<Prompt> currentQueuePrompts = new List<Prompt>(queuePrompts);
        if (currentPrompt != null)
            currentQueuePrompts.Insert(0, currentPrompt);
        notificationBar.RefreshNotifications(currentQueuePrompts);
    }

    void SaveCurrentPrompt()
    {
        if (currentPrompt == null) return;
        string key = currentPrompt.senderName;
        if (!savedPrompts.ContainsKey(key))
            savedPrompts[key] = new List<PromptSaved>();
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

    List<PromptSaved> GetSavedPromptsForSender(string senderName)
    {
        if (savedPrompts.ContainsKey(senderName)) return savedPrompts[senderName];
        return null;
    }

    IEnumerator NextPromptWithDelay(Prompt prompt)
    {
        if (currentPrompt && currentPrompt.senderName == prompt.senderName)
        {
            // If the next prompt is from the same sender, we skip the loading screen and just update the message.
            setupPrompt.SetupPrompt(prompt, ResponseToCurrentPrompt, GetSavedPromptsForSender(prompt.senderName));
            RefreshNotifications();
            yield break;
        }
        setupPrompt.ClearPrompt();
        loadingScreen.SetActive(true);
        yield return new WaitForSeconds(loadingScreenDuration);
        loadingScreen.SetActive(false);
        setupPrompt.SetupPrompt(prompt, ResponseToCurrentPrompt, GetSavedPromptsForSender(prompt.senderName));
        RefreshNotifications();
    }

    void NextPrompt()
    {
        SaveCurrentPrompt();

        if (currentPrompt != null)
        {
            prevPrompts.Insert(0, currentPrompt);
            if (prevPrompts.Count > 5) prevPrompts.RemoveAt(prevPrompts.Count - 1);
        }

        Prompt prompt = PopPromptFromQueue();
        if (prompt == null) prompt = GetRandomPrompt();

        if (prompt != null)
        {
            StartCoroutine(NextPromptWithDelay(prompt));
            currentPrompt = prompt;
            PlayerPrefs.SetInt(prompt.senderName + prompt.message.Truncate(5) + prompt.message.Count().ToString(), 1);
        }
        RefreshNotifications();
    }

    bool IsContainedInPromptList(List<Prompt> l, Prompt p)
    {
        foreach(Prompt r in l)
        {
            if (SameIdentity(r, p))
            {
                return true;
            }
        }
        return false;
    }

    public void ResponseToCurrentPrompt(int selectedResponseIndex)
    {
        var opt = currentPrompt.responseOptions[selectedResponseIndex];

        RessourceManager.Instance.UpdateRessource(opt.ressourceGain);
        RessourceManager.Instance.UpdateBonheur(opt.bonheurGain);

        bool spawnsFollowUp =
            (opt.addedPrompts != null && opt.addedPrompts.Length > 0) ||
            (opt.addedDirectPrompts != null && opt.addedDirectPrompts.Length > 0);
        if (spawnsFollowUp)
            promptsWithWeights.RemoveAll(wp => SameIdentity(wp.prompt, currentPrompt));

        // Pacing trackers.
        if (opt.bonheurGain < 0) consecutiveNegativePrompts++;
        else if (opt.bonheurGain > 0) consecutiveNegativePrompts = 0;

        // Add follow-up prompts.
        if (opt.addedPrompts != null)
        {
            foreach (var addedPrompt in opt.addedPrompts)
            {
                if (addedPrompt.prompt != null && !IsContainedInPromptList(removedPrompts, addedPrompt.prompt)) promptsWithWeights.Add(addedPrompt);
            }
        }
        List<Prompt> promptsAdded = new List<Prompt>();
        if (opt.addedDirectPrompts != null)
        {
            foreach (var addedDirect in opt.addedDirectPrompts)
            {
                if (addedDirect != null && !IsContainedInPromptList(removedPrompts, addedDirect)) promptsAdded.Add(addedDirect);
            }
        }

        StartCoroutine(delayAddPrompts(promptsAdded));
        if (opt.removedPrompts != null && opt.removedPrompts.Length > 0)
            RemovePrompts(opt.removedPrompts);
        StartCoroutine(NextPromptWithDelay(selectedResponseIndex));
    }

    IEnumerator delayAddPrompts(List<Prompt> promptsAdded)
    {
        foreach (Prompt p in promptsAdded)
        {
            AddPromptToQueue(p, true);
            yield return new WaitForSeconds(.2f);
        }
    }

    IEnumerator NextPromptWithDelay(int selectedResponseIndex = 0)
    {
        bool hasAIResponse = currentPrompt.responseOptions[selectedResponseIndex].optionText != "";
        bool hasUserResponse = currentPrompt.responseOptions[selectedResponseIndex].responseUserText != "";
        bool hasBoth = hasAIResponse && hasUserResponse;

        float _delay = 1.25f;

        if (hasAIResponse)
        {
            setupPrompt.NextAIPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay - (hasBoth ? 0.4f : 0f));
        }

        bool gameOverByGauges = RessourceManager.Instance.IsBonheurDead();
        if (gameOverByGauges || currentPrompt.responseOptions[selectedResponseIndex].isGameOverResponse)
        {
            isGameOver = true;
            Time.timeScale = 0f;
            setupPrompt.SetupGameOverButtons();
            yield break;
        }

        if (hasUserResponse)
        {
            setupPrompt.NextUserPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay + (hasBoth ? 0.4f : 0f) + (currentPrompt.responseOptions[selectedResponseIndex].responseUserText.Length >= 20 ? 1 : 0));
        }
    
        NextPrompt();
    }
}
