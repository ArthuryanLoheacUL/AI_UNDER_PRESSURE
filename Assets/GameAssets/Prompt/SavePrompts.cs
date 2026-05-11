using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class SavePrompts : MonoBehaviour
{
    PromptsManager promptsManager;
    public List<Prompt> listAllPrompts;

    Dictionary<string, Prompt> dictPrompts = new Dictionary<string, Prompt>();

    void Awake()
    {
        promptsManager = GetComponent<PromptsManager>();

        foreach (Prompt p in listAllPrompts)
        {
            dictPrompts.Add(p.senderName + p.message.Truncate(20, ""), p);
        }
    }

    #region SAVE
    void OnApplicationQuit()
    {
        if (!promptsManager)
            return;
        if (promptsManager.gameOver)
        {
            // end game
            PlayerPrefs.SetInt("InGame", 0);
            PlayerPrefs.SetInt("QueuePrompts", 0);
            PlayerPrefs.SetInt("PrevPrompts", 0);
            PlayerPrefs.SetInt("RemovedPrompts", 0);
            PlayerPrefs.SetInt("Prompts", 0);
            PlayerPrefs.SetInt("PromptsWithWeights", 0);
            PlayerPrefs.SetInt("TotalPrompts", 0);
        } else
        {
            // in game
            PlayerPrefs.SetInt("InGame", 1);
            SaveQueuePrompts();
            SavePrevPrompts();
            SaveRemovedPrompts();
            SavePrompts_();
            SavePromptsWithWeights();
            SaveCurrentPrompt();
            SaveRessources();
            PlayerPrefs.SetInt("TotalPrompts", promptsManager.GetTotalPrompts());
        }
    }

    void SaveRessources()
    {
        PlayerPrefs.SetInt("Bonheur", RessourceManager.Instance.GetBonheur());
        PlayerPrefs.SetInt("Ressource", RessourceManager.Instance.GetRessource());
    }
    
    void SaveCurrentPrompt()
    {
        Prompt p = promptsManager.GetCurrentPrompt();
        string code = p.senderName + p.message.Truncate(20, "");
        PlayerPrefs.SetString("CurrentPrompt", code);
    }

    void SaveQueuePrompts()
    {
        PlayerPrefs.SetInt("QueuePrompts", promptsManager.GetQueuePrompts().Count);
        for (int i = 0; i < promptsManager.GetQueuePrompts().Count; i++)
        {
            Prompt p = promptsManager.GetQueuePrompts()[i];
            string code = p.senderName + p.message.Truncate(20, "");
            PlayerPrefs.SetString("QueuePrompts_" + i.ToString(), code);
        }
    }

    void SavePrevPrompts()
    {
        PlayerPrefs.SetInt("PrevPrompts", promptsManager.GetPrevPrompts().Count);
        for (int i = 0; i < promptsManager.GetPrevPrompts().Count; i++)
        {
            Prompt p = promptsManager.GetPrevPrompts()[i];
            string code = p.senderName + p.message.Truncate(20, "");
            PlayerPrefs.SetString("PrevPrompts" + i.ToString(), code);
        }
    }

    void SaveRemovedPrompts()
    {
        PlayerPrefs.SetInt("RemovedPrompts", promptsManager.GetRemovedPrompts().Count);
        for (int i = 0; i < promptsManager.GetRemovedPrompts().Count; i++)
        {
            Prompt p = promptsManager.GetRemovedPrompts()[i];
            string code = p.senderName + p.message.Truncate(20, "");
            PlayerPrefs.SetString("RemovedPrompts_" + i.ToString(), code);
        }
    }

    void SavePrompts_()
    {
        PlayerPrefs.SetInt("Prompts", promptsManager.getPrompts().Count);
        for (int i = 0; i < promptsManager.getPrompts().Count; i++)
        {
            Prompt p = promptsManager.getPrompts()[i];
            string code = p.senderName + p.message.Truncate(20, "");
            PlayerPrefs.SetString("Prompts_" + i.ToString(), code);
        }
    }

    void SavePromptsWithWeights()
    {
        PlayerPrefs.SetInt("PromptsWithWeights", promptsManager.GetPromptsWithWeights().Count);
        for (int i = 0; i < promptsManager.GetPromptsWithWeights().Count; i++)
        {
            PromptsManager.WeightedPrompt p = promptsManager.GetPromptsWithWeights()[i];
            string code = p.prompt.senderName + p.prompt.message.Truncate(20, "");
            PlayerPrefs.SetString("PromptsWithWeights_" + i.ToString(), code);
            PlayerPrefs.SetFloat("PromptsWithWeightsW_" + i.ToString(), p.weight);
        }
    }
    #endregion

    #region LOAD

    public bool LoadPrompts()
    {
        bool isInGame = PlayerPrefs.GetInt("InGame", 0) == 1;
        if (isInGame)
        {
            Debug.Log("IN GAME");
            LoadQueuePrompts();
            LoadPrevPrompts();
            LoadRemovedPrompts();
            LoadPrompts_();
            LoadPromptsWithWeights();
            LoadCurrentPrompt();
            LoadRessources();
            promptsManager.SetTotalPrompts(PlayerPrefs.GetInt("TotalPrompts", 0));
            return true;
        } else
        {
            Debug.Log("NOT IN GAME");
            RessourceManager.Instance.SetDefault();
            promptsManager.RefreshScoreBar();
            return false;
        }
    }

    void LoadRessources()
    {
        RessourceManager.Instance.SetBonheur(PlayerPrefs.GetInt("Bonheur", 0));
        RessourceManager.Instance.SetRessource(PlayerPrefs.GetInt("Ressource", 0));
    }

    void LoadCurrentPrompt()
    {
        string codePrompt = PlayerPrefs.GetString("CurrentPrompt", "");
        if (codePrompt == "" || dictPrompts[codePrompt] == null)
            return;
        promptsManager.GetQueuePrompts().Add(new Prompt(dictPrompts[codePrompt]));   
    }

    void LoadQueuePrompts()
    {
        int l = PlayerPrefs.GetInt("QueuePrompts", 0);
        for (int i = 0; i < l; i++)
        {
            string codePrompt = PlayerPrefs.GetString("QueuePrompts_" + i.ToString(), "");
            if (codePrompt == "" || dictPrompts[codePrompt] == null)
                continue;
            promptsManager.GetQueuePrompts().Add(new Prompt(dictPrompts[codePrompt]));
        }
    }

    void LoadPrevPrompts()
    {
        int l = PlayerPrefs.GetInt("PrevPrompts", 0);
        for (int i = 0; i < l; i++)
        {
            string codePrompt = PlayerPrefs.GetString("PrevPrompts_" + i.ToString(), "");
            if (codePrompt == "" || dictPrompts[codePrompt] == null)
                continue;
            promptsManager.GetPrevPrompts().Add(new Prompt(dictPrompts[codePrompt]));
        }
    }

    void LoadRemovedPrompts()
    {
        int l = PlayerPrefs.GetInt("RemovedPrompts", 0);
        for (int i = 0; i < l; i++)
        {
            string codePrompt = PlayerPrefs.GetString("RemovedPrompts_" + i.ToString(), "");
            if (codePrompt == "" || dictPrompts[codePrompt] == null)
                continue;
            promptsManager.GetRemovedPrompts().Add(new Prompt(dictPrompts[codePrompt]));
        }
    }

    void LoadPrompts_()
    {
        int l = PlayerPrefs.GetInt("Prompts", 0);
        for (int i = 0; i < l; i++)
        {
            string codePrompt = PlayerPrefs.GetString("Prompts_" + i.ToString(), "");
            if (codePrompt == "" || dictPrompts[codePrompt] == null)
                continue;
            promptsManager.getPrompts().Add(new Prompt(dictPrompts[codePrompt]));
        }
    }

    void LoadPromptsWithWeights()
    {
        int l = PlayerPrefs.GetInt("PromptsWithWeights", 0);
        for (int i = 0; i < l; i++)
        {
            string codePrompt = PlayerPrefs.GetString("PromptsWithWeights_" + i.ToString(), "");
            float weight = PlayerPrefs.GetFloat("PromptsWithWeightsW_" + i.ToString(), 1);
            if (codePrompt == "" || dictPrompts[codePrompt] == null)
                continue;
            PromptsManager.WeightedPrompt p;
            p.prompt = new Prompt(dictPrompts[codePrompt]);
            p.weight = weight;
            promptsManager.GetPromptsWithWeights().Add(p);
        }
    }


    #endregion
}
