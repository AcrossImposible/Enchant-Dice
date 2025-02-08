using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Iron : Dice
{
    protected override Enemy GetTarget()
    {
        var enemies = GameManager.Instance.allEnemies;

        if (GameManager.Instance.IsPVP)
        {
            enemies = GameManager.Instance.allEnemies.FindAll(e => e.Team == team);
        }

        if (enemies.Count == 0)
            return null;

        var maxHP = enemies.Max(e => e.currentHP);
        return enemies.Find(e => Mathf.Approximately(maxHP, e.currentHP));

    }
}
