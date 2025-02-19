using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RandomGrow : Dice
{
    [Space]

    [SerializeField] float growDuration = 10f;

    float timer;

    protected override void Update()
    {
        base.Update();

        if (team == Team.Mine || user.isBot)
        {
            timer += Time.deltaTime;

            if (timer > growDuration)
            {
                EventsHolder.onDiceDestroyed?.Invoke(this);

                var player = GameManager.Instance.allPlayers.Find(p => p.team == team);
                player.SpawnDice(Cell, Random.Range(0, 6));
           
                Destroy(gameObject);
            }
        }
    }
}
