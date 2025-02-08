using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class CardChestResult : MonoBehaviour
{
    [SerializeField] TMP_Text labelCount;
    [SerializeField] InventoryDice dice;

    public void Init(int count, Dice dice)
    {
        labelCount.text = $"x{count}";

        if (dice)
            this.dice.Init(dice);
        else
            this.dice.gameObject.SetActive(false);
    }
}
