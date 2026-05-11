using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine.SocialPlatforms.Impl;

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

    private Prompt currentPrompt;
    private NotificationsManager notificationBar;
    private SetupPromptManagement setupPrompt;

    public GameObject loadingScreen;
    public float loadingScreenDuration = 1f;

    public ScoreBar scoreBar;

    private int totalPrompt = 0;
    [HideInInspector] public bool gameOver = false;

    #region GET_PROMPTS

    public List<Prompt> getPrompts()
    {
        return prompts;
    }

    public List<WeightedPrompt> GetPromptsWithWeights()
    {
        return promptsWithWeights;
    }
    public List<Prompt> GetQueuePrompts()
    {
        return queuePrompts;
    }
    public List<Prompt> GetPrevPrompts()
    {
        return prevPrompts;
    }
    public List<Prompt> GetRemovedPrompts()
    {
        return removedPrompts;
    }

    public int GetTotalPrompts()
    {
        return totalPrompt;
    }
    public void SetTotalPrompts(int t)
    {
        totalPrompt = t;
        scoreBar.setScore(totalPrompt);
    }

    public void RefreshScoreBar()
    {
        scoreBar.setScore(totalPrompt);
    }

    public Prompt GetCurrentPrompt()
    {
        return currentPrompt;
    }

    #endregion



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

        if (directAdd)
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
        // F is the inverse of bonheur in design notation (0 = calm, 100 = defeat).
        int frustration = 100 - bonheur;

        // Garde-fous: hard gating based on archetype + current gauges.
        // [GAIN_BLOCK]   - GAIN_MASQUE prompts are wasteful when R is already high.
        if (wp.prompt.gainBlockIfHighR && ressources > 70) return 0f;
        // [FAUSSE_BLOCK] - never push a fausse-respiration when F is already critical.
        if (wp.prompt.fausseBlockIfHighF && frustration > 65) return 0f;
        // [DOUBLE_BLOCK] - never push a double-drain when F is near defeat.
        if (wp.prompt.doubleBlockIfHighF && frustration > 80) return 0f;
        // [GAIN_FORCE]   - priority boost when R is critically low.
        if (wp.prompt.gainForceIfLowR && ressources < 25)
            weight *= 3f;

        bool isCrisis = true;
        foreach (Prompt.ResponseOption r in wp.prompt.responseOptions)
            if (r.bonheurGain >= 0 && r.ressourceGain >= 0) isCrisis = false;

        // Damp crisis prompts when everything is fine.
        if (isCrisis && ressources > 60 && bonheur > 55)
            weight *= 0.3f;

        bool isRelief = false;
        foreach (Prompt.ResponseOption r in wp.prompt.responseOptions)
            if (r.bonheurGain >= 0 || r.ressourceGain >= 0) isRelief = true;

        // After three consecutive negative prompts, push relief HARD.
        if (consecutiveNegativePrompts >= 5 && isRelief)
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

        ShufflePromptResponseOptions(instance);
        return instance;
    }

    void Start()
    {
        if (!GetComponent<SavePrompts>().LoadPrompts())
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
        } else
        {
            if (promptsWithWeights.Count == 0)
            {
                foreach (var prompt in prompts)
                {
                    if (prompt == null) continue;
                    promptsWithWeights.Add(new WeightedPrompt(prompt, 1));
                }
            }

            if (queuePrompts.Count == 0)
            {
                for (int i = 0; i < startPrompts; i++)
                {
                    Prompt randomPrompt = GetRandomPrompt();
                    if (randomPrompt != null) AddPromptToQueue(randomPrompt);
                }
            }

            if (currentPrompt == null)
            {
                NextPrompt();        
            }
        }
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
        if (gameOver) return;
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
            int minId = 0;
            int min = -99999;
            bool affordableOne = false;

            for (int u = 0; u < prompt.responseOptions.Count(); u++)
            {
                if (prompt.responseOptions[u].ressourceGain > min)
                {
                    minId = u;
                    min = prompt.responseOptions[u].ressourceGain;
                    if (-prompt.responseOptions[u].ressourceGain <= RessourceManager.Instance.GetRessource())
                    {
                        affordableOne = true;
                    }
                }
            }
            if (!affordableOne)
            {
                prompt.responseOptions[minId].ressourceGain = 0;
            }

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
        totalPrompt++;
        scoreBar.setScore(totalPrompt);
        if (PlayerPrefs.GetInt("HighScore", 0) < totalPrompt)
        {
            PlayerPrefs.SetInt("HighScore", totalPrompt);
            scoreBar.GetComponent<HighScore>().SetHighScore();
        }
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
        if (opt.addedDirectPrompts != null)
        {
            foreach (var addedDirect in opt.addedDirectPrompts)
            {
                if (addedDirect != null && !IsContainedInPromptList(removedPrompts, addedDirect)) AddPromptToQueue(addedDirect, true);
            }
        }

        if (opt.removedPrompts != null && opt.removedPrompts.Length > 0)
            RemovePrompts(opt.removedPrompts);
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

        bool gameOverByGauges = RessourceManager.Instance.IsBonheurDead();
        
        if (hasUserResponse)
        {
            setupPrompt.NextUserPrompt(currentPrompt, selectedResponseIndex);
            yield return new WaitForSeconds(_delay + (hasBoth ? 0.4f : 0f) + (currentPrompt.responseOptions[selectedResponseIndex].responseUserText.Length >= 30 ? 1 : 0));
        }
        if (gameOverByGauges || currentPrompt.responseOptions[selectedResponseIndex].isGameOverResponse)
        {
            setupPrompt.SetupGameOverButtons();
            gameOver = true;
            yield return new WaitForSeconds(_delay - (hasBoth ? 0.4f : 0f));
        }
    
        NextPrompt();
    }
}
