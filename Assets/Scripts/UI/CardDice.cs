using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;
using System;

public class CardDice : MonoBehaviour
{
    [SerializeField] InventoryDice inventoryDice;
    [SerializeField] TMP_Text labelCount;
    [SerializeField] TMP_Text labelLvl;
    [SerializeField] TMP_Text labelPriceUpgrade;
    [SerializeField] GameObject panelLock;
    [SerializeField] GameObject panelPriceUpgrade;
    [SerializeField] Button btnUpgrade;

    public Action onUpgraded;

    User.Dice dice;

    private void OnEnable()
    {
        panelLock.SetActive(true);
        btnUpgrade.gameObject.SetActive(false);
        panelPriceUpgrade.gameObject.SetActive(false);

        btnUpgrade.onClick.RemoveAllListeners();
        btnUpgrade.onClick.AddListener(BtnUpgrade_Clicked);

        
        StartCoroutine(Delay());

        IEnumerator Delay()
        {
            yield return new WaitForSeconds(0.1f);

            if (dice != null)
            {
                UpdateView();
            }

        }
    }

    public void Init(User.Dice dice, Dice dicePrefab)
    {
        this.dice = dice;

        UpdateView();

        inventoryDice.Init(dicePrefab);

        panelLock.SetActive(false);
    }

    private void BtnUpgrade_Clicked()
    {
        if (dice.lvl * dice.lvl * 5 > User.Data.golda)
            return;

        User.Data.golda -= dice.lvl * dice.lvl * 5;

        dice.count -= dice.lvl * 2;
        dice.lvl++;

        UpdateView();

        onUpgraded?.Invoke();

#if UNITY_ANDROID
            Saver.Save();
#endif

#if UNITY_WEBGL
        Saver.ConvertToYG();
        YG.YandexGame.SaveProgress();
#endif
    } 


    void UpdateView()
    {
        string txt = Language.Rus ? "Óð." : "lvl";
        labelLvl.text = $"{txt} {dice.lvl}";
        labelCount.text = $"{dice.count}/{2 * dice.lvl}";
        labelPriceUpgrade.text = $"{dice.lvl * dice.lvl * 5}";

        if (dice.count >= 2 * dice.lvl)
        {
            btnUpgrade.gameObject.SetActive(true);
            panelPriceUpgrade.gameObject.SetActive(true);
            labelCount.gameObject.SetActive(false);
        }
        else
        {
            btnUpgrade.gameObject.SetActive(false);
            panelPriceUpgrade.gameObject.SetActive(false);
            labelCount.gameObject.SetActive(true);
        }
    }
}
