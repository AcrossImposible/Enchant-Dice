using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using YG;
using YG.Utils.LB;
using TMPro;

public class YGLeaderBoard : MonoBehaviour
{
    [SerializeField] TMP_Text title;
    [SerializeField] string boardName;
    [SerializeField] ScrollRect scrollRect; // Перетяните в инспекторе
    [SerializeField] RectTransform content; // Тот же Content, что в ScrollRect
    [SerializeField] CanvasGroup canvasGroup;

    [Tooltip("Максимальное кол-во получаемых игроков")]
    public int maxQuantityPlayers = 20;

    [Tooltip("Кол-во получения верхних топ игроков")]
    [Range(1, 20)]
    public int quantityTop = 3;

    [Tooltip("Кол-во получаемых записей возле пользователя")]
    [Range(1, 10)]
    public int quantityAround = 6;

    public enum PlayerPhoto
    { NonePhoto, Small, Medium, Large };
    [Tooltip("Размер подгружаемых изображений игроков. NonePhoto = не подгружать изображение.")]
    public PlayerPhoto playerPhoto = PlayerPhoto.Small;

    [Tooltip("Использовать кастомный спрайт для отображения пользователей без аватаров.")]
    public Sprite isHiddenPlayerPhoto;

    [SerializeField, Tooltip("Конвертация полученных рекордов в Time тип")]
    private bool timeTypeConvert;

    [SerializeField,
        Range(0, 3), Tooltip("Размер десятичной части счёта (при использовании Time type).\n  Например:\n  0 = 00:00\n  1 = 00:00.0\n  2 = 00:00.00\n  3 = 00:00.000\nВы можете проверить это в Unity не прибегая к тестированию в сборке.")]
    private int decimalSize = 1;

    [SerializeField] Transform rootSpawnPlayersData;

    [SerializeField] public GameObject playerDataPrefab;
    [SerializeField] public GameObject borderlinePrefab;

    YG_LB_ItemView[] players = new YG_LB_ItemView[0];

    private string photoSize;
    private bool spawnedBorderline;

    private void Start()
    {
        canvasGroup.alpha = 0;
    }

    public void Init()
    {
        if (string.IsNullOrEmpty(boardName))
        {
            Debug.LogError("Название не указано! Дэбил");
            return;
        }

        if (playerPhoto == PlayerPhoto.NonePhoto)
            photoSize = "nonePhoto";
        if (playerPhoto == PlayerPhoto.Small)
            photoSize = "small";
        else if (playerPhoto == PlayerPhoto.Medium)
            photoSize = "medium";
        else if (playerPhoto == PlayerPhoto.Large)
            photoSize = "large";

        DestroyLBList();

        YandexGame.onGetLeaderboard += OnUpdateLB;
    }

    public void InvokeGetLeadearBoard()
    {
        YandexGame.GetLeaderboard(boardName, maxQuantityPlayers, quantityTop, quantityAround, photoSize);
        //print("=-=-=-=-=-");
    }


    private void OnUpdateLB(LBData lbData)
    {
        if (lbData.technoName == boardName)
        {
            var maxWaveStr = Language.Rus 
                ? "Максимально прожито волн в кооперативе" 
                : "Maximum waves spent in the cooperative";
            title.SetText(maxWaveStr);

            if (!YandexGame.auth)
            {
                var auth = Language.Rus 
                    ? "Войди в свой аккаунт, чтобы соревноваться с другими игроками!"
                    : "Log in to your account to compete with other players!";
                title.SetText($"<color=#FFF300>{auth}</color>");
            }
            else if (lbData.thisPlayer == null)
            {
                var playCoop = Language.Rus 
                    ? "Играй в кооператив, покажи другим как долго ты продержишься!" 
                    : "Play co-op, show others how long you can last!";
                title.SetText($"<color=#56D15D>{playCoop}</color>");
            }

            if (lbData.entries != "no data")
            {
#if UNITY_EDITOR
                lbData = LBMethods.SortLB(lbData, quantityTop, quantityAround, maxQuantityPlayers);
#endif
                SpawnPlayersList(lbData);

            }
            else
            {
                Debug.Log("  нет");
            }

            LeanTweanTool.SetTransparencyImage(canvasGroup, 1f, 0.58f);
        }
        
    }

    private void SpawnPlayersList(LBData lb)
    {
        players = new YG_LB_ItemView[lb.players.Length];
        for (int i = 0; i < players.Length; i++)
        {
            GameObject playerObj = Instantiate(playerDataPrefab, rootSpawnPlayersData);

            players[i] = playerObj.GetComponent<YG_LB_ItemView>();

            int rank = lb.players[i].rank;

            players[i].data.name = LBMethods.AnonimName(lb.players[i].name);
            players[i].data.rank = rank.ToString();

            if (rank <= quantityTop)
            {
                players[i].data.inTop = true;
            }
            else
            {
                players[i].data.inTop = false;
                if (rank - 1 != quantityTop && !spawnedBorderline)
                {
                    spawnedBorderline = true;
                    players[i].data.borderline = true;
                    var line = Instantiate(borderlinePrefab, rootSpawnPlayersData).transform;
                    var idx = line.GetSiblingIndex() - 1;
                    line.SetSiblingIndex(idx);
                }
            }


            if (lb.players[i].uniqueID == YandexGame.playerId)
            {
                players[i].data.thisPlayer = true;
                var rectThisPlayer = players[i].transform as RectTransform;
                StartCoroutine(ScrollToItemCoroutine(rectThisPlayer));
            }
            else
            {
                players[i].data.thisPlayer = false;
            }

            if (timeTypeConvert)
            {
                string timeScore = TimeTypeConvert(lb.players[i].score);
                players[i].data.score = timeScore;
            }
            else
            {
                players[i].data.score = lb.players[i].score.ToString();
            }

            if (playerPhoto != PlayerPhoto.NonePhoto)
            {
                if (isHiddenPlayerPhoto && lb.players[i].photo.Contains("/avatar/0/"))
                {
                    players[i].data.photoSprite = isHiddenPlayerPhoto;
                }
                else
                {
                    players[i].data.photoUrl = lb.players[i].photo;
                }
            }

            players[i].UpdateEntries();
        }

        LayoutRebuilder.ForceRebuildLayoutImmediate(rootSpawnPlayersData as RectTransform);
    }

    public string TimeTypeConvert(int score)
    {
        return LBMethods.TimeTypeConvertStatic(score, decimalSize);
    }

    private void DestroyLBList()
    {
        int childCount = rootSpawnPlayersData.childCount;
        for (int i = childCount - 1; i >= 0; i--)
        {
            Destroy(rootSpawnPlayersData.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// Прокручивает scrollRect так, чтобы target оказался в центре видимой области по вертикали.
    /// </summary>
    public void ScrollToItem(RectTransform target)
    {
        // Позиция цели внутри content (0…1): 0 — низ контента, 1 — верх.
        float contentHeight = content.rect.height;
        float viewportHeight = scrollRect.viewport.rect.height;

        // Позиция центра target относительно нижней границы content
        float targetLocalY = Mathf.Abs(target.localPosition.y) + (target.rect.height * 0.5f);

        // Нормализованная позиция: (targetY – половина viewport) / (content – viewport)
        float normalizedPosition = (targetLocalY - viewportHeight * 0.5f) / (contentHeight - viewportHeight);
        normalizedPosition = Mathf.Clamp01(normalizedPosition);

        var targetValue = 1f - normalizedPosition;
        LeanTween.value(gameObject, p =>
        {
            scrollRect.verticalNormalizedPosition = p;
        }, scrollRect.verticalNormalizedPosition, targetValue, 1f).setEaseOutQuad();
        //scrollRect.verticalNormalizedPosition = 

    }


    public IEnumerator ScrollToItemCoroutine(RectTransform target)
    {
        // Дождаться конца текущего фрейма, чтобы LayoutGroup успел пересчитать размеры
        yield return new WaitForEndOfFrame();
        Canvas.ForceUpdateCanvases();

        ScrollToItem(target);
    }

    private void OnDisable() => YandexGame.onGetLeaderboard -= OnUpdateLB;
}
