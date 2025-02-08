using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using TMPro;

public class PanelIncrease : MonoBehaviour
{
    [SerializeField] Button btnIncrease;
    [SerializeField] TMP_Text labelCountRequiredItem;
    [SerializeField] TMP_Text labelChance;
    [SerializeField] TMP_Text labelAdditionalChance;
    [SerializeField] InventoryDice resultDiceView;

    [Space]

    [SerializeField] AnimationCurve chanceCurve;

    Dice dice;
    int countRequiredItems;
    int additionalChanceIncrese;
    int chanceIncrese;

    public void Init()
    {
        gameObject.SetActive(false);

        EventsHolder.onDiceClicked.AddListener(Dice_Clicked);
        btnIncrease.onClick.AddListener(BtnIncrease_Clicked);

        
    }

    private void Dice_Clicked(Dice dice)
    {
        if (dice.IncreaseStage == 0)
            return;

        this.dice = dice;

        gameObject.SetActive(true);

        UpdateView();

        resultDiceView.Init(dice);
        resultDiceView.UpdateIncreaseView(dice.IncreaseStage);

        if (dice.IncreaseStage == 5)
            btnIncrease.gameObject.SetActive(false);

    }

    private void BtnIncrease_Clicked()
    {
        if (User.Data.countStones < countRequiredItems)
            return;

        if (Random.Range(0, 100) < chanceIncrese)
        {
            dice.Increase();
            resultDiceView.UpdateIncreaseView(dice.IncreaseStage);

            additionalChanceIncrese = 0;

            EventsHolder.onDiceIncreased?.Invoke(dice);
        }
        else
        {
            additionalChanceIncrese++;
        }

        User.Data.countStones -= countRequiredItems;

#if UNITY_ANDROID
        Saver.Save();
#endif

#if UNITY_WEBGL
        Saver.ConvertToYG();
        YG.YandexGame.SaveProgress();
#endif

        UpdateView();
    }

    void UpdateView()
    {
        chanceIncrese = (int)chanceCurve.Evaluate(dice.IncreaseStage);
        chanceIncrese += (additionalChanceIncrese * 10) / dice.IncreaseStage;
        countRequiredItems = 10 * (int)Mathf.Pow(dice.IncreaseStage, 1.57f);

        labelChance.text = $"Increse chance {chanceIncrese}%";

        labelCountRequiredItem.text = $"x{countRequiredItems}";

        labelAdditionalChance.text = $"Increase the chance of gain +{additionalChanceIncrese}";
    
        btnIncrease.gameObject.SetActive(!(dice.IncreaseStage == 5));

    }

}
