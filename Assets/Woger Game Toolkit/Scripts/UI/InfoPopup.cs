using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class InfoPopup : MonoBehaviour
{
    [SerializeField] InfoPopupItem infoItemPrefab;
    [SerializeField] Button button;
    [SerializeField] TMP_Text title;
    [SerializeField] RectTransform content;

    public void Init(params InfoItemData[] infoItems)
    {
        foreach (var item in collection)
        {

        }
    }

    [System.Serializable]
    public class InfoItemData
    {
        public string text;
        public Sprite icon;
    }
}
