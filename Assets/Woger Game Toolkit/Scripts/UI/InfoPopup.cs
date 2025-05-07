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
    [SerializeField] Button buttonBack;
    [SerializeField] TMP_Text title;
    [SerializeField] RectTransform content;
    [SerializeField] CanvasGroup canvasGroup;

    [Header("Anim Settings")]
    [SerializeField] float timeClose = 0.3f;
    [SerializeField] Vector3 closeSize;
    [SerializeField] float timeShow = 0.3f;
    [SerializeField] Vector3 startSize;
    [SerializeField] float startAlpha;

    static Transform uiRoot;

    public void Init(string name, params InfoItemData[] infoItems)
    {
        Clear();

        title.SetText(name);

        foreach (var item in infoItems)
        {
            var itemView = Instantiate(infoItemPrefab, content);
            itemView.Init(item.text, item.icon);
            itemView.gameObject.SetActive(true);
        }

        button.transform.SetAsLastSibling();

        button.onClick.AddListener(Close);
        buttonBack.onClick.AddListener(Close);

        LayoutRebuilder.ForceRebuildLayoutImmediate(transform as RectTransform);

        canvasGroup.alpha = startAlpha;
        transform.localScale = startSize;

        transform.LeanScale(Vector3.one, timeShow).setEaseOutQuad();
        LeanTweanTool.SetTransparencyImage(canvasGroup, 1, timeShow).setEaseOutQuad();
    }

    public void Close()
    {
        transform.LeanScale(closeSize, timeClose)
            .setEaseOutQuad().setOnComplete(Annig);
        LeanTweanTool.SetTransparencyImage(canvasGroup, 0, timeClose - 0.01f);

        void Annig()
        {
            Destroy(gameObject);
        }
    }

    public static void Show(InfoPopup prefab, string name, params InfoItemData[] infoItems)
    {
        uiRoot ??= FindFirstObjectByType<CanvasScaler>().transform;

        var popup = Instantiate(prefab, uiRoot);
        popup.Init(name, infoItems);
    }

    void Clear()
    {
        foreach (Transform item in content)
        {
            var infoItem = item.GetComponent<InfoPopupItem>();

            if(infoItem && infoItem == infoItemPrefab)
            {
                infoItem.gameObject.SetActive(false);
                continue;
            }

            if (infoItem)
            {
                Destroy(item.gameObject);
            }
        }
    }

    [System.Serializable]
    public class InfoItemData
    {
        public string text;
        public Sprite icon;

        public InfoItemData(string text)
        {
            this.text = text;
        }

        public InfoItemData(string text, Sprite icon)
        {
            this.text = text;
            this.icon = icon;
        }
    }
}
