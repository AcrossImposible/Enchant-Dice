using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using System;
using TMPro;
using YG;

public class YG_LB_ItemView : MonoBehaviour
{
    public ImageLoadYG imageLoad;
    public MonoBehaviour[] topPlayerActivityComponents = new MonoBehaviour[0];
    public MonoBehaviour[] thisPlayerActivityComponents = new MonoBehaviour[0];

    [Serializable]
    public struct TextLegasy
    {
        public Text rank, name, score;
    }
    public TextLegasy textLegasy;


    [Serializable]
    public struct TextMP
    {
        public TextMeshProUGUI rank, name, score;
    }
    public TextMP textMP;


    public class Data
    {
        public string rank;
        public string name;
        public string score;
        public string photoUrl;
        public bool inTop;
        public bool thisPlayer;
        public bool borderline;
        public Sprite photoSprite;
    }

    [HideInInspector]
    public Data data = new Data();


    [ContextMenu(nameof(UpdateEntries))]
    public void UpdateEntries()
    {
        if (textLegasy.rank && data.rank != null) textLegasy.rank.text = data.rank.ToString();
        if (textLegasy.name && data.name != null) textLegasy.name.text = data.name;
        if (textLegasy.score && data.score != null) textLegasy.score.text = data.score.ToString();


        if (textMP.rank && data.rank != null) textMP.rank.text = data.rank.ToString();
        if (textMP.name && data.name != null) textMP.name.text = data.name;
        if (textMP.score && data.score != null) textMP.score.text = data.score.ToString();

        if (imageLoad)
        {
            if (data.photoSprite)
            {
                imageLoad.PutSprite(data.photoSprite);
            }
            else if (data.photoUrl == null)
            {
                imageLoad.ClearImage();
            }
            else
            {
                imageLoad.Load(data.photoUrl);
            }
        }

        if (topPlayerActivityComponents.Length > 0)
        {
            if (data.inTop && transform.GetSiblingIndex() < 3)
            {
                ActivityMomoObjects(topPlayerActivityComponents, true);
            }
            else
            {
                ActivityMomoObjects(topPlayerActivityComponents, false);
            }
        }

        if (thisPlayerActivityComponents.Length > 0)
        {
            if (data.thisPlayer)
            {
                ActivityMomoObjects(thisPlayerActivityComponents, true);
            }
            else
            {
                ActivityMomoObjects(thisPlayerActivityComponents, false);
            }
        }

        void ActivityMomoObjects(MonoBehaviour[] objects, bool activity)
        {
            for (int i = 0; i < objects.Length; i++)
            {
                objects[i].enabled = activity;
            }
        }
    }
}
