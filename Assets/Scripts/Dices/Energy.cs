using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Energy : Dice
{
    [Space]

    [SerializeField] GameObject effectPrefab;
    [SerializeField] public int energyDamage;
    [SerializeField] public int energyDamageStep = 1;

    public override void Init(User user, Team team, int stage)
    {
        base.Init(user, team, stage);

        energyDamage += energyDamageStep * user.inventory.Find(d => d.idx == idxInInventory).lvl;

    }


    protected override void InitProjectile(Projectile projectile)
    {
        base.InitProjectile(projectile);

        var energyProjectile = projectile.gameObject.AddComponent<EnergyProjectile>();
        var finalDamage = energyDamage + (energyDamage * IncreaseStage);
        energyProjectile.Init(effectPrefab, finalDamage, team);
    }
}

public class EnergyProjectileData
{
    public GameObject effectPrefab;
}
