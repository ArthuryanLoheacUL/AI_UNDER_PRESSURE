using System;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class PromptsPaths : MonoBehaviour
{
    [Serializable]
    public struct PromptsPath
    {
        public string name;
        public List<Prompt> list;
    }

    public List<PromptsPath> listPrompt = new List<PromptsPath>();

    public GameObject LinePrefab;
    public GameObject LinePrefabUnkown;

    void Start()
    {
        foreach(PromptsPath p in listPrompt)
        {
            int get = 0;
            foreach(Prompt c in p.list)
            {
                get += PlayerPrefs.GetInt(c.senderName + c.message.Truncate(5) + c.message.Count().ToString(), 0);
            }
            GameObject g = Instantiate(get <= 0 ? LinePrefabUnkown : LinePrefab, transform);
            g.GetComponent<LineSetup>().SetupLine(get, p.list.Count, p.name);
        }
    }
}
