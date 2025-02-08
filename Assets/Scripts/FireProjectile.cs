using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class FireProjectile : MonoBehaviour
{
    Projectile projectile;
    GameObject effectPrefab;
    float fireArea;
    int fireDamage;

    public void Init(Projectile projectile, GameObject effectPrefab, int fireDamage, float fireArea)
    {
        this.projectile = projectile;
        this.projectile.onDamage += Enemy_Damaged;
        this.effectPrefab = effectPrefab;
        this.fireDamage = fireDamage;
        this.fireArea = fireArea;
    }

    private void Enemy_Damaged(Enemy enemy)
    {
        var effet = Instantiate(effectPrefab, enemy.transform.position, Quaternion.identity);
        effet.transform.position += Vector3.back * 10;

        var allEnemies = GameManager.Instance.allEnemies.Select(e => e).ToList();
        for (int i = 0; i < allEnemies.Count; i++)
        {
            if (!allEnemies[i])
                continue;

            var dist = Vector2.Distance(allEnemies[i].transform.position, transform.position);
            if (dist < fireArea)
            {
                allEnemies[i].Damage(fireDamage);
            }
        }
        
    }

    private void OnDisable()
    {
        projectile.onDamage -= Enemy_Damaged;
    }

}
