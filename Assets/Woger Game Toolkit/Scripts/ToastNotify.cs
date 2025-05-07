using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class ToastNotify : MonoBehaviour
{
    [SerializeField] TMP_Text label;
    [SerializeField] Image icon;
    [SerializeField] RectTransform content;
    [SerializeField] CanvasGroup canvasGroup;
    [SerializeField] Vector3 hideScale;
    [SerializeField] float timeHide = 0.198f;

    static Transform uiRoot;

    public void Init(string txt, Sprite icon)
    {
        label.SetText(txt);
        this.icon.sprite = icon;

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);

        transform.localScale = Vector3.one * 0.3f;

        LeanTween.scale(gameObject, Vector3.one, 0.18f).setEaseOutQuad();

        LeanTween.delayedCall(1.8f, AutoHide);
    }

    void AutoHide()
    {
        LeanTween.scale(gameObject, hideScale, timeHide)
            .setEaseOutQuad().setOnComplete(Annig);
        LeanTweanTool.SetTransparencyImage(canvasGroup, 0, timeHide - 0.01f);

        void Annig()
        {
            Destroy(gameObject);
        }
    }

    public static void Show(ToastNotify prefab, string txt, Sprite icon)
    {
        if (!uiRoot)
        {
            uiRoot = FindFirstObjectByType<CanvasScaler>().transform;
        }

        var view = Instantiate(prefab, uiRoot);
        view.Init(txt, icon);
    }
}
