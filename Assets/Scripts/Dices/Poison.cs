using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Poison : Dice
{
    [Space]

    [SerializeField] PoisonEnemy enemyPoisonPrefab;
    [SerializeField] public int poisonDamage = 1;
    [SerializeField] public int poisonDamageStep = 1;

    List<Enemy> attacked = new();

    public override void Init(User user, Team team, int stage)
    {
        base.Init(user, team, stage);

        poisonDamage += poisonDamageStep * user.inventory.Find(d => d.idx == idxInInventory).lvl;

    }

    protected override void InitProjectile(Projectile projectile)
    {
        base.InitProjectile(projectile);

        projectile.onDamage += Enemy_Damaged;

        target = null;
    }

    private void Enemy_Damaged(Enemy enemy)
    {
        if (enemy.IsPoisoned)
            return;

        var poisonEffect = Instantiate(enemyPoisonPrefab, enemy.transform);
        poisonEffect.Init(enemy, poisonDamage * (Stage + 1) * (int)Mathf.Pow(IncreaseStage + 1, 3));

        enemy.IsPoisoned = true;
    }


    protected override Enemy GetTarget()
    {
        Enemy enemy = null;

        if (GameManager.Instance.IsPVP)
            enemy = GameManager.Instance.allEnemies.Find(e => !e.IsPoisoned && e.Team == team && !attacked.Find(a => a == e));
        else
            enemy = GameManager.Instance.allEnemies.Find(e => !e.IsPoisoned && !attacked.Find(a => a == e));

        attacked.Add(enemy);
        attacked.RemoveAll(a => !a);

        if (!enemy)
            if (GameManager.Instance.IsPVP)
                enemy = GameManager.Instance.allEnemies.Find(e => e.Team == team);
            else if (GameManager.Instance.allEnemies.Any())
                enemy = GameManager.Instance.allEnemies.First();

        return enemy;
    }

    
}
