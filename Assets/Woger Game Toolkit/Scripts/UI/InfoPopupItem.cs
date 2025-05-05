using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class InfoPopupItem : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Image image;
    [SerializeField] HorizontalLayoutGroup horizontalGroup;
    [SerializeField] ContentSizeFitter labelSizeFitter;

    public void Init(string txt, Sprite icon = null)
    {
        if (!icon)
        {
            labelSizeFitter.horizontalFit = ContentSizeFitter.FitMode.Unconstrained;
            labelSizeFitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            horizontalGroup.childControlWidth = true;
            image.gameObject.SetActive(false);
        }
        else
        {
            image.sprite = icon;
        }

        label.SetText(txt);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);
    }
}
