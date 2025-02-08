using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MiniBoss : Enemy
{
    public override void Init(Transform[] trajectory, int number, Team team)
    {
        base.Init(trajectory, number, team);

        reward *= (GameManager.Instance.Wave + 1) / 5;
    }

    protected override int CalculateHP(int number)
    {
        var bonusByWave = (GameManager.Instance.Wave + 1) / 5;
        return baseHP * (int)Mathf.Pow(bonusByWave, 2);
    }
}
