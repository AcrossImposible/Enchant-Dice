using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialPlayer : Player
{
    public override Dice SpawnDice(Cell cell = null)
    {
        if (!cell)
        {
            if (PriceDice > Coins)
                return null;

            cell = GetCell();

            if (cell == null)
                return null;

            Coins -= PriceDice;
            PriceDice += 10;

            onCoinsUpdated?.Invoke();
        }
        user = new();
        var idx = user.deck[Random.Range(0, user.deck.Count)].idx;

        var randomDice = dicesPrefab[idx];

        if (allDices.Count < 3)
        {
            randomDice = dicesPrefab[1];
        }
        

        var dice = Instantiate(randomDice, cell.transform.position + Vector3.back, Quaternion.identity);
        dice.Init(user, team, 0);
        dice.Cell = cell;

        cell.IsEmpty = false;

        allDices.Add(dice);
        allDices.RemoveAll(d => !d);

        return dice;
    }
}
