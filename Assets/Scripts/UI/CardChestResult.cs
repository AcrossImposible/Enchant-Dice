using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardChestResult : MonoBehaviour
{
    [SerializeField] TMP_Text labelCount;
    [SerializeField] InventoryDice dice;
    [SerializeField] GameObject goldLabel;

    public void Init(int count, Dice dice)
    {
        labelCount.text = $"x{count}";

        if (dice)
        {
            this.dice.Init(dice);
            goldLabel.SetActive(false);
        }
        else
        {
            this.dice.gameObject.SetActive(false);
        }
    }
}
