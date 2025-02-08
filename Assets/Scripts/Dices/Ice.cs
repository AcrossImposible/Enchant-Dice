using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Ice : Dice
{
    [Space]

    [SerializeField] GameObject[] icedPrefabs;


    protected override void InitProjectile(Projectile projectile)
    {
        base.InitProjectile(projectile);

        projectile.onDamage += Enemy_Damaged;

        target = null;
    }

    private void Enemy_Damaged(Enemy enemy)
    {
        if (enemy.FrozenStage > 1)
            return;

        var ice = Instantiate(icedPrefabs[enemy.FrozenStage], enemy.transform);
        enemy.FrozenStage++;
    }

    protected override int CalculateDamage()
    {
        return base.CalculateDamage() * Stage;
    }
}
