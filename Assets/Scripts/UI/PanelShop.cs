using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;

public class PanelShop : MonoBehaviour
{
    [Header("БЕСПЛАТНЫЕ БОНУСЫ")]
    [SerializeField] TMP_Text freeBonusTitle;
    [SerializeField] Button btnFreeCoins;
    [SerializeField] Button btnFreeStones;
    [SerializeField] ParticleSystem flyCoinsEffect;

    [Header("МОНЕТКИ ЗА РЕКЛАМУ")]
    [SerializeField] BtnCoinsRewarded[] btnsCoinsRewarded;
    [SerializeField] ToastNotify coinsRewardNotifyPrefab;
    [SerializeField] Sprite coinSprite;
    [SerializeField] GameObject coinsRewardEffectPrefab;
    [SerializeField] GameObject coinsFlyEffectPrefab;
    [SerializeField] InfoPopup infoPopupPrefab;
    [SerializeField] TMP_Text rewardCoinsTitle; 

    [HideInInspector] public UnityEvent<float> onCoinsUpdate;

    const string rewardedCoinsKey = "rewardedCoins";
    const string coinsRewardIdxKey = "coinsRewardIdxKey";
    const string coinsRewardResetKey = "coinsRewardResetKey";

    const string coinsFreeKey = "coinsFreeKey";
    const string stonesFreeKey = "stonesFreeKey";

    AvailableView btnFreeCoinsAvailable;
    AvailableView btnFreeStonesAvailable;

    string freeUnavailableMsg;

    bool coinsRewardsUnavailable;
    bool allCoinsRewardUnavailable;
    int rewardID;

    public void Init()
    {
        DailyRewardModule.RegisterReward(rewardedCoinsKey, DailyRewardModule.ResetMode.FixedInterval, TimeSpan.FromMinutes(1));
        DailyRewardModule.RegisterReward(coinsRewardResetKey, DailyRewardModule.ResetMode.FixedInterval, TimeSpan.FromMinutes(3));
        DailyRewardModule.RegisterReward(coinsFreeKey, DailyRewardModule.ResetMode.FixedInterval, TimeSpan.FromSeconds(10));
        DailyRewardModule.RegisterReward(stonesFreeKey, DailyRewardModule.ResetMode.FixedInterval, TimeSpan.FromMinutes(10));

        //DailyRewardModule.RegisterReward(coinsRewardResetKey, DailyRewardModule.ResetMode.DailyUtcReset);

        freeUnavailableMsg = "Это ежедневная награда, ты сможешь снова получить её завтра";

        YG.YandexGame.RewardVideoEvent += RewardVideo_Watched;
        YG.YandexGame.CloseVideoEvent += RewarVideo_Closed;

        btnFreeCoins.onClick.AddListener(FreeCoins_Clicked);
        btnFreeStones.onClick.AddListener(FreeStones_Clicked);

        for (int i = 0; i < btnsCoinsRewarded.Length; i++)
        {
            var btnCoins = btnsCoinsRewarded[i];
            btnCoins.Init();
            btnCoins.onClick.AddListener(CoinsRewarded_Clicked);
        }

        btnFreeCoinsAvailable = btnFreeCoins.GetComponent<AvailableView>();
        btnFreeStonesAvailable = btnFreeStones.GetComponent<AvailableView>();

        CoinsRewardCheckAvailable();
    }

    private void FreeStones_Clicked()
    {
        if (btnFreeStonesAvailable.state is AvailableView.State.Unavailable)
        {
            InfoPopup.Show(infoPopupPrefab, "Внимание!", new InfoPopup.InfoItemData(freeUnavailableMsg));
        }
        else
        {
            DailyRewardModule.Claim(stonesFreeKey, null);
            btnFreeStones.GetComponent<AttentionAnim>().Play();
            btnFreeStonesAvailable.Unavailable();
            User.Data.countStones += 100;
            Saver.Save();
        }
    }

    private void FreeCoins_Clicked()
    {
        var availableView = btnFreeCoins.GetComponent<AvailableView>();
        if (availableView.state is AvailableView.State.Unavailable)
        {
            InfoPopup.Show(infoPopupPrefab, "Внимание!", new InfoPopup.InfoItemData(freeUnavailableMsg));
        }
        else
        {
            DailyRewardModule.Claim(coinsFreeKey, null);
            btnFreeCoins.GetComponent<AttentionAnim>().Play();
            btnFreeCoins.GetComponent<AvailableView>().Unavailable();
            User.Data.golda += 100;
            Saver.Save();
            flyCoinsEffect.Play();
            onCoinsUpdate?.Invoke(2.1f);
        }
    }

    private void RewardVideo_Watched(int adID)
    {
        rewardID = adID;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.3f);

            Action doAvailables = () => { };

            var maxWatchedIdx = 0;
            if (PlayerPrefs.HasKey(coinsRewardIdxKey))
            {
                maxWatchedIdx = PlayerPrefs.GetInt(coinsRewardIdxKey);
            }

            int reward = 0;
            if (rewardID == 0)
            {
                doAvailables = () =>
                {
                    btnsCoinsRewarded[0].Watched();
                    btnsCoinsRewarded[1].Available();
                };
                reward = 100;
            }

            if (rewardID == 1)
            {
                doAvailables = () =>
                {
                    btnsCoinsRewarded[1].Watched();
                    btnsCoinsRewarded[2].Available();
                };
                reward = 500;

            }

            if (rewardID == 2)
            {
                doAvailables = () =>
                {
                    btnsCoinsRewarded[2].Watched();
                };
                reward = 1000;
            }

            if (maxWatchedIdx <= rewardID)
            {
                maxWatchedIdx = rewardID;
                PlayerPrefs.SetInt(coinsRewardIdxKey, rewardID);
                PlayerPrefs.Save();
            }

            CheckMaxWatchedIdx();

            User.Data.golda += reward;
            Saver.Save();

            DailyRewardModule.Claim(coinsRewardResetKey, null);

            onCoinsUpdate?.Invoke(3.1f);

            ToastNotify.Show(coinsRewardNotifyPrefab, $"+{reward}", coinSprite);
            Instantiate(coinsRewardEffectPrefab, transform);
            Instantiate(coinsFlyEffectPrefab, transform);

            yield return new WaitForSeconds(1.8f);

            if (!allCoinsRewardUnavailable)
            {
                doAvailables();
            }

            void CheckMaxWatchedIdx()
            {
                if (maxWatchedIdx == 2)
                {
                    DailyRewardModule.Claim(rewardedCoinsKey, AllCoinsRewardUnavailable);
                }
            }
        }
    }

    private void Update()
    {
        TimeSpan rem = DailyRewardModule.GetTimeRemaining(rewardedCoinsKey);
        coinsRewardsUnavailable = rem > TimeSpan.Zero;

        rewardCoinsTitle.text = coinsRewardsUnavailable
            ? $"{rem.Hours:D2}:{rem.Minutes:D2}:{rem.Seconds:D2}"
            : "Получи дополнительные монеты!";

        if (allCoinsRewardUnavailable && !coinsRewardsUnavailable)
        {
            CoinsRewardCheckAvailable();
        }

        if (!allCoinsRewardUnavailable && coinsRewardsUnavailable)
        {
            AllCoinsRewardUnavailable();
        }

        // Проверка доступности
        if (DailyRewardModule.CanClaim(coinsRewardResetKey) && PlayerPrefs.HasKey(coinsRewardIdxKey))
        {
            PlayerPrefs.DeleteKey(coinsRewardIdxKey);
            PlayerPrefs.Save();

            //DailyRewardModule.Claim(coinsRewardResetKey, null);

            if (!allCoinsRewardUnavailable)
            {
                CoinsRewardCheckAvailable();
            }
            print("доооо");
        }



        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayerPrefs.DeleteKey(rewardedCoinsKey);
        }

        FreeBonusUpdate();
    }

    void FreeBonusUpdate()
    {
        if (DailyRewardModule.CanClaim(coinsFreeKey))
        {
            if (btnFreeCoinsAvailable.state is AvailableView.State.Unavailable)
            {
                btnFreeCoinsAvailable.Available();
                btnFreeCoins.GetComponent<AttentionAnim>().Play();
            }
        }
        else if (!(btnFreeCoinsAvailable.state is AvailableView.State.Unavailable))
        {
            btnFreeCoinsAvailable.Unavailable();
        }

        if (DailyRewardModule.CanClaim(stonesFreeKey))
        {
            if (btnFreeStonesAvailable.state is AvailableView.State.Unavailable)
            {
                btnFreeStonesAvailable.Available();
                btnFreeStones.GetComponent<AttentionAnim>().Play();
            }
        }
        else if (!(btnFreeStonesAvailable.state is AvailableView.State.Unavailable))
        {
            btnFreeStonesAvailable.Unavailable();
        }

        var unavailableTitle = "Сегодня уже всё получено";
        if (btnFreeStonesAvailable.state is AvailableView.State.Unavailable && btnFreeCoinsAvailable.state is AvailableView.State.Unavailable)
        {
            freeBonusTitle.SetText(unavailableTitle);
        }
        else if (freeBonusTitle.text == unavailableTitle)
        {
            freeBonusTitle.SetText("Забери просто так!");
        }
    }

    void CoinsRewardCheckAvailable()
    {
        allCoinsRewardUnavailable = false;

        for (int i = 0; i < btnsCoinsRewarded.Length; i++)
        {
            var btnCoins = btnsCoinsRewarded[i];


            if (!PlayerPrefs.HasKey(coinsRewardIdxKey))
            {
                if (i == 0)
                {
                    btnCoins.Available();
                }
                else
                {
                    btnCoins.Unavailable();
                }
            }
            else
            {
                var watchedIdx = PlayerPrefs.GetInt(coinsRewardIdxKey);
                if (i <= watchedIdx)
                {
                    btnCoins.Watched();
                }
                else if (i - 1 <= watchedIdx)
                {
                    btnCoins.Available();
                }
                else
                {
                    btnCoins.Unavailable();
                }
            }
        }
    }

    void AllCoinsRewardUnavailable()
    {
        allCoinsRewardUnavailable = true;
        foreach (var item in btnsCoinsRewarded)
        {
            item.Unavailable();
        }
    }

    private void CoinsRewarded_Clicked(BtnCoinsRewarded btn)
    {
        
        if (btn.state is BtnCoinsRewarded.State.Unavailable)
        {
            if (allCoinsRewardUnavailable)
            {
                InfoPopup.Show
                (
                    infoPopupPrefab,
                    "Внимание!",
                    new InfoPopup.InfoItemData(
                        $"Дождись сброса времени. Не переживай, максимальная награда будет сразу доступна в течении дня")
                );
            }
            else
            {
                var r = btn.Idx == 1 ? "+100" : "+500";

                InfoPopup.Show
                (
                    infoPopupPrefab,
                    "Внимание!",
                    new InfoPopup.InfoItemData($"Сначала вам нужно получить предыдущую награду"),
                    new InfoPopup.InfoItemData($"{r}", coinSprite)
                );
            }
            return;
        }

#if UNITY_WEBGL && YG_PLUGIN_YANDEX_GAME
        YG.YandexGame.RewVideoShow(btn.Idx);
        
#endif
    }

    private void RewarVideo_Closed()
    {

    }

    public void Dispose ()
    {
#if UNITY_WEBGL && YG_PLUGIN_YANDEX_GAME
        YG.YandexGame.RewardVideoEvent -= RewardVideo_Watched;
        YG.YandexGame.CloseVideoEvent -= RewarVideo_Closed;
#endif
    }
}
