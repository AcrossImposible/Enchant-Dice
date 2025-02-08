using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Adventurer : Dice
{
    protected override int CalculateDamage()
    {
        return Random.Range(baseDamage, User.Data.crit);
    }

    protected override int CalculateCriticalDamage(int damage, out bool isCrit)
    {
        isCrit = false;
        return damage;
    }
}
