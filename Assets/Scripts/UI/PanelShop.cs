using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;

public class PanelShop : MonoBehaviour
{
    [Header("������� �� �������")]
    [SerializeField] BtnCoinsRewarded[] btnsCoinsRewarded;
    [SerializeField] ToastNotify coinsRewardNotifyPrefab;
    [SerializeField] Sprite coinSprite;
    [SerializeField] GameObject coinsRewardEffectPrefab;
    [SerializeField] GameObject coinsFlyEffectPrefab;
    [SerializeField] InfoPopup infoPopupPrefab;
    [SerializeField] TMP_Text rewardCoinsTitle; 

    [HideInInspector] public UnityEvent onUserDataUpdate;

    const string rewardedCoinsKey = "rewardedCoins";
    const string coinsRewardIdxKey = "coinsRewardIdxKey";
    const string coinsRewardResetKey = "coinsRewardResetKey";

    bool coinsRewardsUnavailable;
    bool allCoinsRewardUnavailable;
    int rewardID;

    public void Init()
    {
        DailyRewardModule.RegisterReward(rewardedCoinsKey, DailyRewardModule.ResetMode.FixedInterval, TimeSpan.FromMinutes(1));
        DailyRewardModule.RegisterReward(coinsRewardResetKey, DailyRewardModule.ResetMode.FixedInterval, TimeSpan.FromMinutes(3));

        //DailyRewardModule.RegisterReward(coinsRewardResetKey, DailyRewardModule.ResetMode.DailyUtcReset);

        YG.YandexGame.RewardVideoEvent += RewardVideo_Watched;
        YG.YandexGame.CloseVideoEvent += RewarVideo_Closed;

        for (int i = 0; i < btnsCoinsRewarded.Length; i++)
        {
            var btnCoins = btnsCoinsRewarded[i];
            btnCoins.Init();
            btnCoins.onClick.AddListener(CoinsRewarded_Clicked);
        }

        CoinsRewardCheckAvailable();
    }

    private void RewarVideo_Closed()
    {
        
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

            onUserDataUpdate?.Invoke();

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
            : "������ �������������� ������!";

        if (allCoinsRewardUnavailable && !coinsRewardsUnavailable)
        {
            CoinsRewardCheckAvailable();
        }

        if (!allCoinsRewardUnavailable && coinsRewardsUnavailable)
        {
            AllCoinsRewardUnavailable();
        }

        // �������� �����������
        if (DailyRewardModule.CanClaim(coinsRewardResetKey) && PlayerPrefs.HasKey(coinsRewardIdxKey))
        {
            PlayerPrefs.DeleteKey(coinsRewardIdxKey);
            PlayerPrefs.Save();

            //DailyRewardModule.Claim(coinsRewardResetKey, null);

            if (!allCoinsRewardUnavailable)
            {
                CoinsRewardCheckAvailable();
            }
            print("�����");
        }



        if (Input.GetKeyDown(KeyCode.J))
        {
            PlayerPrefs.DeleteKey(rewardedCoinsKey);
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
                    "��������!",
                    new InfoPopup.InfoItemData(
                        $"������� ������ �������. �� ���������, ������������ ������� ����� ����� �������� � ������� ���")
                );
            }
            else
            {
                var r = btn.Idx == 1 ? "+100" : "+500";

                InfoPopup.Show
                (
                    infoPopupPrefab,
                    "��������!",
                    new InfoPopup.InfoItemData($"������� ��� ����� �������� ���������� �������"),
                    new InfoPopup.InfoItemData($"{r}", coinSprite)
                );
            }
            return;
        }

#if UNITY_WEBGL && YG_PLUGIN_YANDEX_GAME
        YG.YandexGame.RewVideoShow(btn.Idx);
        
#endif
    }

    public void Dispose ()
    {
#if UNITY_WEBGL && YG_PLUGIN_YANDEX_GAME
        YG.YandexGame.RewardVideoEvent -= RewardVideo_Watched;
        YG.YandexGame.CloseVideoEvent -= RewarVideo_Closed;
#endif
    }
}
