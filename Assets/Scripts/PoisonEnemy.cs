using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PoisonEnemy : MonoBehaviour
{
    Enemy enemy;
    int damage;
    float damageRate;

    public void Init(Enemy enemy, int damage)
    {
        this.damage = damage;
        this.enemy = enemy;
    }

    private void Update()
    {
        damageRate += Time.deltaTime;

        if (damageRate > 1)
        {
            enemy.Damage(damage);

            damageRate = 0;
        }
    }
}
