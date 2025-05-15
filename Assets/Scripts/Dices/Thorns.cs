using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Thorns : Dice
{
    [Space]

    [SerializeField] ThornProjectile thornProjectilePrefab;

    public override void Init(User user, Team team, int stage)
    {
        base.Init(user, team, stage);

        currentRate = 0;
    }

    protected override void Attack()
    {
        currentRate += Time.deltaTime;

        if (currentRate < CurrentRateThreshold() || stopAtack)
            return;

        currentRate = 0;

        Transform[] points;
        if (GameManager.Instance.IsPVP)
        {
            if (team is Team.Mine)
            {
                points = GameManager.Instance.trajectoryMinePoints;
            }
            else
            {
                points = GameManager.Instance.trajectoryOtherPoints;
            }
        }
        else
        {
            if (team is Team.Mine)
            {
                points = GameManager.Instance.trajectoryMineCoop;
            }
            else
            {
                points = GameManager.Instance.trajectoryOtherCoop;
            }

        }

        var pos = GetRandomPositionOnTrajectory(points) + (Vector3.down * 0.8f);

        var projectile = Instantiate(thornProjectilePrefab, transform.position, Quaternion.identity);
        var damage = CalculateDamage();
        damage = CalculateCriticalDamage(damage, out bool isCrit);
        var attackSpeed = 1.8f + IncreaseStage;
        if (IncreaseStage == 5)
        {
            attackSpeed = 10;
        }
        projectile.Init(pos, team, damage, attackSpeed);
    }

    protected override float CurrentRateThreshold()
    {
        return fireRate / (IncreaseStage + 1);
    }

    protected override int CalculateDamage()
    {
        return baseDamage * Stage * (IncreaseStage * IncreaseStage + 1);
    }

    /// <summary>
    /// Возвращает случайную позицию вдоль траектории, заданной массивом точек.
    /// </summary>
    /// <param name="trajectoryPoints">Массив Transform, описывающих точки траектории.</param>
    /// <returns>Vector3 случайной позиции вдоль траектории.</returns>
    public static Vector3 GetRandomPositionOnTrajectory(Transform[] trajectoryPoints)
    {
        if (trajectoryPoints == null || trajectoryPoints.Length < 2)
        {
            Debug.LogError("Для расчета траектории необходимо минимум две точки.");
            return Vector3.zero;
        }

        // Вычисляем длины сегментов и общую длину
        var segmentLengths = new List<float>(trajectoryPoints.Length - 1);
        float totalLength = 0f;
        for (int i = 0; i < trajectoryPoints.Length - 1; i++)
        {
            float dist = Vector3.Distance(trajectoryPoints[i].position, trajectoryPoints[i + 1].position);
            segmentLengths.Add(dist);
            totalLength += dist;
        }

        // Выбираем случайную точку по длине
        float randomDistance = Random.Range(0f, totalLength);

        // Находим сегмент, в котором лежит randomDistance
        float accumulated = 0f;
        for (int i = 0; i < segmentLengths.Count; i++)
        {
            if (accumulated + segmentLengths[i] >= randomDistance)
            {
                // Определяем относительный параметр t в сегменте
                float segmentT = (randomDistance - accumulated) / segmentLengths[i];
                // Лерп между началом и концом сегмента
                return Vector3.Lerp(
                    trajectoryPoints[i].position,
                    trajectoryPoints[i + 1].position,
                    segmentT
                );
            }

            accumulated += segmentLengths[i];
        }

        // На случай погрешностей, возвращаем последнюю точку
        return trajectoryPoints[trajectoryPoints.Length - 1].position;
    }

}
