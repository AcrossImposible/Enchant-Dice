using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnergyProjectile : MonoBehaviour
{
    GameObject effectPrefab;
    Team team;
    int energyDamage;

    public void Init(GameObject effectPrefab, int energyDamage, Team team)
    {
        this.effectPrefab = effectPrefab;
        this.energyDamage = energyDamage;
        this.team = team;

        GetComponent<Projectile>().onDamage += Enemy_Damaged;
    }

    private void Enemy_Damaged(Enemy enemy)
    {
        SpawnEffect(enemy);
        List<Enemy> forDamage = new();
        int countEnemiesDamaged = 0;
        var allEneimes = GameManager.Instance.allEnemies;
        for (int i = 0; i < allEneimes.Count; i++)
        {
            if (!allEneimes[i] || allEneimes[i].Team != team)
                continue;

            // TO DO
            var dist = Vector2.Distance(allEneimes[i].transform.position, enemy.transform.position);
            if (dist < 5)
            {
                forDamage.Add(allEneimes[i]);
                countEnemiesDamaged++;
            }

            if (countEnemiesDamaged > 2)
                break;
        }

        foreach (var item in forDamage)
        {
            item.Damage(energyDamage);
            SpawnEffect(item);
        }

    }

    void SpawnEffect(Enemy enemy)
    {
        var pos = enemy.transform.position + Vector3.back;
        var effect = Instantiate(effectPrefab, pos, Quaternion.identity);
        //effect.transform.position += Vector3.back;
    }
}
