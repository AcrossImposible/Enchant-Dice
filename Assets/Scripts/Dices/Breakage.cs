using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Breakage : Dice
{
    protected override void InitProjectile(Projectile projectile)
    {
        base.InitProjectile(projectile);

        target = null;
    }

    protected override Enemy GetTarget()
    {
        var enemies = GameManager.Instance.allEnemies;

        if (GameManager.Instance.IsPVP)
        {
            enemies = GameManager.Instance.allEnemies.FindAll(e => e.Team == team);
        }

        if (enemies.Count == 0)
            return null;

        return enemies[Random.Range(0, enemies.Count)];
    }

    protected override int CalculateDamage()
    {
        return base.CalculateDamage() + Stage;
    }

    protected override float CurrentRateThreshold()
    {
        if (IncreaseStage >= 5)
            return 0;

        return base.CurrentRateThreshold();
    }
}
