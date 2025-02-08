using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;

public class Dice : MonoBehaviour
{
    [SerializeField] Color color;
    [SerializeField] public int baseDamage = 5;
    [SerializeField] public float fireRate = 0.5f;

    [Space]

    [SerializeField] public int stepDamage = 1;
    [SerializeField] public float stepFireRate = 0.03f;

    [Space]

    [SerializeField] GameObject[] stages;
    [SerializeField] TMP_Text labelIncrese;
    [SerializeField] SpriteRenderer border;
    [SerializeField] public SpriteRenderer icon;
    [SerializeField] Projectile projectilePrefab;

    [Space]
    [SerializeField] public string title;
    [TextArea] public string info;

    public int idxInInventory = -1;

    public Color Color => color;
    public int Stage { get; set; } = 0;
    public int IncreaseStage { get; set; } = 0;
    public Cell Cell { get; set; }
    public Team Team => team;

    protected Team team;
    protected Enemy target;
    protected User user;

    bool stopAtack;
    float currentRate;
    int idxFireableGun;

    public virtual void Init(User user, Team team, int stage)
    {
        this.user = user;
        this.team = team;
        Stage = stage;

        currentRate = fireRate;

        DisableAllStages();

        stages[Stage].SetActive(true);

        foreach (var stageView in stages)
        {
            foreach (Transform item in stageView.transform)
            {
                item.GetComponent<SpriteRenderer>().color = color;
            }
        }

        border.color = this is Chameleon ? Color.white : color;

        labelIncrese.gameObject.SetActive(false);
        //print(user);
        //print(idxInInventory);
        //print(user.inventory.Find(d => d.idx == idxInInventory));
        baseDamage += user.inventory.Find(d => d.idx == idxInInventory).lvl * stepDamage;
    }

    public void AddDots()
    {
        if (Stage == Settings.MAX_STAGE)
            return;

        Stage++;

        DisableAllStages();

        stages[Stage].SetActive(true);
    }


    public void Increase()
    {
        DisableAllStages();

        IncreaseStage++;

        string increaseLabel = string.Empty;
        switch (IncreaseStage)
        {
            case 1:
                increaseLabel = "I";
                break;
            case 2:
                increaseLabel = "II";
                break;
            case 3:
                increaseLabel = "III";
                break;
            case 4:
                increaseLabel = "IV";
                break;
            case 5:
                increaseLabel = "V";
                break;
        }

        labelIncrese.text = increaseLabel;

        labelIncrese.gameObject.SetActive(true);
    }

    void DisableAllStages()
    {
        foreach (var item in stages)
        {
            item.SetActive(false);
        }
    }

    private void Update()
    {
        Attack();

        if (Input.GetKeyDown(KeyCode.I))
        {
            AddDots();
        }
    }

    protected virtual void Attack()
    {
        currentRate += Time.deltaTime;

        if (currentRate < CurrentRateThreshold() || stopAtack)
            return;

        if (target)
        {
            var pos = stages[Stage].transform.GetChild(idxFireableGun).position;
            pos.z = -70;
            var projectile = Instantiate(projectilePrefab, pos, Quaternion.identity);
            InitProjectile(projectile);

            idxFireableGun++;

            if (idxFireableGun > Stage)
                idxFireableGun = 0;

            currentRate = 0;

            target = null;
        }
        else
        {
            target = GetTarget();
        }
    }

    protected virtual float CurrentRateThreshold()
    {
        return fireRate / (Stage + 1) / ((IncreaseStage * IncreaseStage) + 1);
    }

    protected virtual void InitProjectile(Projectile projectile)
    {
        var damage = CalculateDamage();
        damage = CalculateCriticalDamage(damage, out bool isCrit);
        projectile.Init(target, damage, color);
    }

    protected virtual int CalculateDamage()
    {
        return baseDamage * (IncreaseStage * IncreaseStage + 1);
    }

    protected virtual int CalculateCriticalDamage(int damage, out bool isCrit)
    {
        isCrit = false;

        if (Random.Range(0, 100) < 5)
        {
            damage += damage * (int)((float)user.crit * 0.01f);
            isCrit = true;
        }

        return damage;
    }

    protected virtual Enemy GetTarget()
    {
        var enemies = GameManager.Instance.allEnemies;

        if (GameManager.Instance.IsPVP)
        {
            enemies = GameManager.Instance.allEnemies.FindAll(e => e.Team == team);
        }

        if (enemies.Count == 0)
            return null;

        var minDist = enemies.Min(e => e.DistanceToFinish());
        return enemies.Find(e => Mathf.Approximately(minDist, e.DistanceToFinish()));


        //if(GameManager.Instance.IsPVP)
        //    return GameManager.Instance?.allEnemies.Find(e => e && e.Team == team);

        //return GameManager.Instance?.allEnemies.Find(e => e);
    }

    public void Stop()
    {
        stopAtack = true;
    }
}
