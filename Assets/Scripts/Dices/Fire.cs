using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class Fire : Dice
{
    [Space]

    [SerializeField] GameObject fireEffectPrefab;
    [SerializeField] public int fireDamage = 3;
    [SerializeField] int fireDamageStep = 2;
    [SerializeField] float fireArea = 1.9f;

    public override void Init(User user, Team team, int stage)
    {
        base.Init(user, team, stage);

        fireDamage += fireDamageStep * user.inventory.Find(d => d.idx == idxInInventory).lvl;

        fireDamage += fireDamage * IncreaseStage * IncreaseStage;
    }


    protected override void InitProjectile(Projectile projectile)
    {
        base.InitProjectile(projectile);

        var fireProjectile = projectile.gameObject.AddComponent<FireProjectile>();
        fireProjectile.Init(projectile, fireEffectPrefab, fireDamage, fireArea);
    }

    
}
