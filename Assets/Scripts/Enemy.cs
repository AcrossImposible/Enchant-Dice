using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class Enemy : MonoBehaviour
{
    [SerializeField] TMP_Text labelHP;

    [field: SerializeField]
    public float Speed { get; set; }

    [Space]

    [SerializeField] PowerStone powerStonePrefab;
    public Team Team { get; private set; }
    public bool IsPoisoned { get; set; }
    public int FrozenStage { get; set; }

    public int reward = 20;

    public int baseHP = 10;
    public int currentHP;

    Transform[] trajectory;
    Vector3 moveDir;
    int idxMovePoint;

    public virtual void Init(Transform[] trajectory, int number, Team team)
    {
        this.trajectory = trajectory;
        Team = team;

        idxMovePoint = 0;

        UpdateMoveDirection();

        reward += 10 * (GameManager.Instance.Wave / 8);

        currentHP = CalculateHP(number);
    }

    protected virtual int CalculateHP(int number)
    {
        int koef = GameManager.Instance.IsPVP ? 50 : 10;
        var bonusByNumber = koef * (number / 10);

        float powerValue = GameManager.Instance.IsPVP ? 5.0f : 2.18f;
        var bonusByWave = 10 * (int)Mathf.Pow(GameManager.Instance.Wave, powerValue);

        return baseHP + bonusByNumber + bonusByWave;
    }

    public void Damage(int value)
    {
        currentHP -= value;

        if(currentHP <= 0)
        {
            EventsHolder.onEnemyAnniged?.Invoke(this);
            Destroy(gameObject);
            if (Team == Team.Mine && GameManager.Instance.IsPVP && Random.Range(0, 130) < 8)
            {
                SpawnPowerStone();
            }
        }
    }

    void SpawnPowerStone()
    {
        Instantiate(powerStonePrefab, transform.position, Quaternion.identity);
    }

    private void Update()
    {
        Move();
        UpdateHP();

#if UNITY_EDITOR
        if (Input.GetKeyDown(KeyCode.S))
        {
            Speed += 0.1f;
        }
#endif
    }

    void Move()
    {
        transform.position += (Speed / (FrozenStage + 1)) * Time.deltaTime * moveDir;

        var dist = Vector2.Distance(transform.position, trajectory[idxMovePoint].position);
        if (dist < 0.1f)
        {
            if (idxMovePoint < trajectory.Length - 1)
            {
                idxMovePoint++;
                UpdateMoveDirection();
            }
            else
            {
                EventsHolder.onEnemySkiped?.Invoke(this);
            }
        }
    }

    void UpdateMoveDirection()
    {
        moveDir = trajectory[idxMovePoint].position - transform.position;
    }

    void UpdateHP()
    {
        labelHP.text = currentHP.ToString();
    }

    public float DistanceToFinish()
    {
        float dist = 0;

        for (int i = idxMovePoint; i < trajectory.Length - 1; i++)
        {
            dist += Vector2.Distance(trajectory[i].position, trajectory[i + 1].position);
        }

        dist += Vector2.Distance(trajectory[idxMovePoint].position, transform.position);

        return dist;
    }
}

public enum Team
{
    Mine = 0,
    Other = 1,
}
