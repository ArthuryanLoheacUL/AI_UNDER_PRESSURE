using System;
using System.Collections.Generic;
using UnityEngine;

public class ProfilePictureManager : MonoBehaviour
{
    [Serializable]
    public struct Picture
    {
        public string name;
        public Sprite picture;
    }
    public List<Picture> lstPictures = new List<Picture>();
    public Sprite defaultPicture;

    public Sprite GetSprite(string name)
    {
        foreach (Picture p in lstPictures)
        {
            if (p.name == name)
                return p.picture;
        }
        return defaultPicture;
    }
}
