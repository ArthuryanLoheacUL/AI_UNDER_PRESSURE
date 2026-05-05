using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;

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
        if (prompt.isUrgent)
            SoundEffectManager.Instance.PlaySoundEffectRandomPitch("TimerClicks");
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

    // Stable identity: use the ScriptableObject's asset name (carried through
    // clones in the Prompt copy constructor). Falls back to senderName+message
    // for safety on any prompt that has lost its name.
    static bool SameIdentity(Prompt a, Prompt b)
    {
        if (a == null || b == null) return false;
        if (!string.IsNullOrEmpty(a.name) && !string.IsNullOrEmpty(b.name))
            return a.name == b.name;
        return a.senderName == b.senderName && a.message == b.message;
    }

    public void AddPromptToQueue(Prompt prompt)
    {
        if (prompt.isUrgent)
            queuePrompts.Insert(0, prompt);
        else
            queuePrompts.Add(prompt);

        // A unique prompt is removed from the pool the moment it's queued so
        // it can never come back through random selection or another addedPrompt.
        if (prompt.isUnique)
            promptsWithWeights.RemoveAll(p => SameIdentity(p.prompt, prompt));

        if (!string.IsNullOrEmpty(prompt.chainTag))
            activeChains.Add(prompt.chainTag);

        prevPrompts.Add(prompt);
        if (prevPrompts.Count > 5)
            prevPrompts.RemoveAt(0);

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

    // -------------------- Removal API --------------------
    public void RemovePrompts(Prompt[] toRemove)
    {
        if (toRemove == null) return;
        foreach (var p in toRemove)
        {
            if (p == null) continue;
            promptsWithWeights.RemoveAll(wp => SameIdentity(wp.prompt, p));
            queuePrompts.RemoveAll(q => SameIdentity(q, p));
        }
    }

    public void RemoveChainByTag(string tag)
    {
        if (string.IsNullOrEmpty(tag)) return;
        promptsWithWeights.RemoveAll(wp =>
            wp.prompt != null && wp.prompt.chainTag == tag);
        queuePrompts.RemoveAll(q => q.chainTag == tag);
        activeChains.Remove(tag);
    }
    // ----------------------------------------------------

    // Contextual weighting based on the current state of the gauges and
    // recent pacing. Mirrors the design spec.
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

        // Boost prompts that belong to a chain we're already in.
        if (!string.IsNullOrEmpty(wp.prompt.chainTag) && activeChains.Contains(wp.prompt.chainTag))
            weight *= 2f;

        // Damp crisis prompts when everything is fine.
        if (wp.prompt.isCrisis && ressources > 60 && bonheur > 55)
            weight *= 0.3f;

        // After three consecutive negative prompts, push relief HARD.
        if (consecutiveNegativePrompts >= 3 && wp.prompt.isRelief)
            weight *= 3f;

        // Hard cap: never serve a 4th double-drain in a row.
        if (consecutiveDoubleDrainPrompts >= 3 && IsDoubleDrain(wp.prompt))
            weight *= 0.05f;

        return weight;
    }

    bool IsDoubleDrain(Prompt p)
    {
        if (p == null || p.responseOptions == null || p.responseOptions.Length == 0) return false;
        // A "double drain" prompt is one where EVERY response option pushes
        // both gauges down. We just check if there's any safe option available.
        foreach (var opt in p.responseOptions)
        {
            if (opt.ressourceGain >= 0 && opt.bonheurGain >= 0)
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
                if (SameIdentity(wp.prompt, recent))
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
        // NOTE: this used to read GetFrustration() / 100, which scaled with HOW
        // BAD things were. Equivalent now: invert bonheur.
        float pressureChance = (100 - RessourceManager.Instance.GetBonheur()) / 100f;
        if (Random.value < pressureChance) instance.isUrgent = true;

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
            currentPrompt = prompt;
            if (!string.IsNullOrEmpty(prompt.chainTag)) activeChains.Add(prompt.chainTag);
            setupPrompt.SetupPrompt(prompt, ResponseToCurrentPrompt, GetSavedPromptsForSender(prompt.senderName));
        }
        RefreshNotifications();
    }

    public void ResponseToCurrentPrompt(int selectedResponseIndex)
    {
        var opt = currentPrompt.responseOptions[selectedResponseIndex];

        RessourceManager.Instance.UpdateRessource(opt.ressourceGain);
        RessourceManager.Instance.UpdateBonheur(opt.bonheurGain);

        // If this prompt spawns ANY follow-up (added to pool or queued direct),
        // remove the current prompt from the pool defensively. This prevents
        // a prompt from being re-played and re-injecting its sequel later,
        // independently of its isUnique flag.
        bool spawnsFollowUp =
            (opt.addedPrompts != null && opt.addedPrompts.Length > 0) ||
            (opt.addedDirectPrompts != null && opt.addedDirectPrompts.Length > 0);
        if (spawnsFollowUp)
            promptsWithWeights.RemoveAll(wp => SameIdentity(wp.prompt, currentPrompt));

        // Pacing trackers.
        if (opt.bonheurGain < 0) consecutiveNegativePrompts++;
        else if (opt.bonheurGain > 0) consecutiveNegativePrompts = 0;

        if (opt.bonheurGain < 0 && opt.ressourceGain < 0) consecutiveDoubleDrainPrompts++;
        else consecutiveDoubleDrainPrompts = 0;

        // Add follow-up prompts.
        if (opt.addedPrompts != null)
        {
            foreach (var addedPrompt in opt.addedPrompts)
            {
                if (addedPrompt.prompt != null) promptsWithWeights.Add(addedPrompt);
            }
        }
        if (opt.addedDirectPrompts != null)
        {
            foreach (var addedDirect in opt.addedDirectPrompts)
            {
                if (addedDirect != null) AddPromptToQueue(addedDirect);
            }
        }

        // Remove specific prompts (e.g. character death).
        if (opt.removedPrompts != null && opt.removedPrompts.Length > 0)
            RemovePrompts(opt.removedPrompts);
        // Remove whole chains by tag (e.g. "Parker", "Signal").
        if (opt.removedChainTags != null)
        {
            foreach (var tag in opt.removedChainTags)
                RemoveChainByTag(tag);
        }

        StartCoroutine(NextPromptWithDelay(selectedResponseIndex));
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
        if (hasUserResponse)
        {
            setupPrompt.NextUserPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay + (hasBoth ? 0.4f : 0f));
        }

        // Game over is driven by bonheur only. Ressources can be 0 (terrible
        // for the crew) but it's a "drained-not-dead" state, not a hard end.
        bool gameOverByGauges = RessourceManager.Instance.IsBonheurDead();

        if (gameOverByGauges || currentPrompt.responseOptions[selectedResponseIndex].isGameOverResponse)
        {
            isGameOver = true;
            Time.timeScale = 0f;
            setupPrompt.SetupGameOverButtons();
            yield break;
        }
        else
        {
            NextPrompt();
        }
    }

    void Update()
    {
        if (isGameOver) return;
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
                // Ignored urgent prompt: small bonheur penalty.
                RessourceManager.Instance.UpdateBonheur(-10);
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
                RessourceManager.Instance.UpdateBonheur(-10);
                edited = true;
            }
        }
        if (currentPrompt == null) NextPrompt();
        if (edited) RefreshNotifications();
    }

    void SetMinimumQueuePrompts(int minimum)
    {
        for (int i = queuePrompts.Count; i < minimum; i++)
        {
            Prompt randomPrompt = GetRandomPrompt();
            if (randomPrompt != null) AddPromptToQueue(randomPrompt);
        }
    }
}
