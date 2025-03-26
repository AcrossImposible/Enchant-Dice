using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static Settings;

public class AIBehaviour : MonoBehaviour
{
    Player player;
    float actionTimer;

    private void Start()
    {
        player = GetComponent<Player>();
    }

    private void Update()
    {
        actionTimer += Time.deltaTime;

        SpawnDice();
        MergeDice();
        IncreaseDice();
        CheckLockedDeck();

        if (Input.GetKeyDown(KeyCode.K))
        {
            print(player.Coins);
            print(player.PriceDice);
        }
    }

    void SpawnDice()
    {
        if (actionTimer < 3)
            return;

        if (player.SpawnDice())
            actionTimer = 0;
    }

    void MergeDice()
    {
        if (actionTimer < 3)
            return;

        if (player.GetCell())
            return;

        foreach (var dice in player.allDices)
        {
            if (!dice || dice.Stage >= MAX_STAGE)
                continue;

            var otherDice = player.allDices.Find(d => d && d != dice && d.Stage == dice.Stage && (dice.Color == d.Color || dice is Chameleon || d is Chameleon));
            if (otherDice)
            {
                dice.Cell.IsEmpty = true;
                var stage = otherDice.Stage;
                var cell = otherDice.Cell;
                Destroy(dice.gameObject);
                Destroy(otherDice.gameObject);

                actionTimer = 0;
                var newDice = player.SpawnDice(cell);
                
                newDice.Init(player.user, player.team, stage + 1);
                
                return;
            }
        }
    }

    void IncreaseDice()
    {
        if (actionTimer < 3)
            return;

       
        foreach (var dice in player.allDices)
        {
            if (!dice || dice.Stage < MAX_STAGE || dice.IncreaseStage > 0)
                continue;

            var otherDice = player.allDices.Find(d => d && d != dice && d.Stage == dice.Stage && dice.Color == d.Color && d.IncreaseStage == 0);
            if (otherDice)
            {
                otherDice.Increase();
                dice.Cell.IsEmpty = true;
                Destroy(dice.gameObject);
                actionTimer = 0;

                return;
            }

        }

        foreach (var dice in player.allDices)
        {
            if (!dice || dice.IncreaseStage == 0 || dice.IncreaseStage >= MAX_INCREASE_STAGE)
                continue;

            var countRequiredItems = 10 * (int)Mathf.Pow(dice.IncreaseStage, 1.57f);

            if (player.user.countStones < countRequiredItems)
                return;

            var chanceIncrese = 15 / dice.IncreaseStage;
            player.user.countStones -= countRequiredItems;

            if (Random.Range(0, 100) < chanceIncrese)
            {
                
                dice.Increase();
                actionTimer = 0;
                return;
            }
        }
    }

    void CheckLockedDeck()
    {
        if (actionTimer < 3)
            return;

        // HOT FIX
        if (!player.GetCell())
        {
            player.user.countStones += 1;
            var toDestroy = player.allDices.Find(d => d.Stage == 0);
            if(!toDestroy || toDestroy is RandomGrow)
            {
                return;
            }
            toDestroy.Cell.IsEmpty = true;
            Destroy(toDestroy.gameObject);
            actionTimer = -1;
        }
    }
}
