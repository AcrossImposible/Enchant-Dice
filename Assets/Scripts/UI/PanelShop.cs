using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;
using UnityEngine.Events;

public class PanelShop : MonoBehaviour
{
    [Header("ÃŒÕ≈“ » «¿ –≈ À¿Ã”")]
    [SerializeField] BtnCoinsRewarded[] btnsCoinsRewarded;
    [SerializeField] ToastNotify coinsRewardNotifyPrefab;
    [SerializeField] Sprite coinSprite;
    [SerializeField] GameObject coinsRewardEffectPrefab;
    [SerializeField] GameObject coinsFlyEffectPrefab;

    [HideInInspector] public UnityEvent onUserDataUpdate;

    bool rewardReceived;
    int rewardID;

    public void Init()
    {
        YG.YandexGame.RewardVideoEvent += RewardVideo_Watched;
        YG.YandexGame.CloseVideoEvent += RewarVideo_Closed;

        for (int i = 0; i < btnsCoinsRewarded.Length; i++)
        {
            var btnCoins = btnsCoinsRewarded[i];
            btnCoins.Init();
            btnCoins.onClick.AddListener(CoinsRewarded_Clicked);
            
            if (i == 0)
            {
                btnCoins.Available();
            }
            else
            {
                btnCoins.Unavailable();
                //print(btnCoins);
            }
        }
    }

    private void RewarVideo_Closed()
    {
        
    }

    private void RewardVideo_Watched(int adID)
    {
        rewardReceived = true;
        rewardID = adID;

        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.3f);


            int reward = 0;
            if (rewardID == 0)
            {
                btnsCoinsRewarded[0].Watched();
                btnsCoinsRewarded[1].Available();
                reward = 100;
            }

            if (rewardID == 1)
            {
                btnsCoinsRewarded[1].Watched();
                btnsCoinsRewarded[2].Available();
                reward = 500;
            }

            if (rewardID == 2)
            {
                btnsCoinsRewarded[2].Watched();
                reward = 1000;
            }

            User.Data.golda += reward;
            Saver.Save();

            onUserDataUpdate?.Invoke();

            ToastNotify.Show(coinsRewardNotifyPrefab, $"+{reward}", coinSprite);
            Instantiate(coinsRewardEffectPrefab, transform);
            Instantiate(coinsFlyEffectPrefab, transform);
        }
    }

    private void CoinsRewarded_Clicked(BtnCoinsRewarded btn)
    {
        rewardReceived = false;

        if (btn.state is BtnCoinsRewarded.State.Unavailable)
        {

            return;
        }

#if UNITY_WEBGL && YG_PLUGIN_YANDEX_GAME
        YG.YandexGame.RewVideoShow(btn.Idx);
        
#endif
    }
}
