using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ThornProjectile : MonoBehaviour
{
    [SerializeField] float speed = 10;
    [SerializeField] float fireRite = .3f;
    [SerializeField] Move move;
    [SerializeField] GameObject placedEffect;
    [SerializeField] float yOffsetEffectPos = 0.8f;
    [SerializeField] float attackDistance = 3f;
    [SerializeField] Animator animator;

    private static readonly int AttackHash = Animator.StringToHash("Attack");

    List<Enemy> enemies;
    Vector3 target;
    Vector3 dir;
    Team team;
    float attackSpeed;
    int damage;

    internal void Init(Vector3 pos, Team team, int damage, float attackSpeed)
    {
        this.attackSpeed = attackSpeed;
        this.team = team;
        this.damage = damage;
        target = pos;
        dir = (pos - transform.position).normalized;

        move.MoveProjectile(transform, target, speed, Placed);
        var scale = transform.localScale;
        transform.localScale = Vector3.one * 0.1f;
        transform.LeanScale(scale, 0.3f).setEaseOutQuad();

        GameManager.Instance.onEnemiesListChange.AddListener(EnemiesList_Changed);

        animator.SetFloat("AttackSpeed", attackSpeed); // Установить нужную скорость

        EnemiesList_Changed();

        DelayExpired();
    }

    void DelayExpired()
    {
        LeanTween.delayedCall(5f, Expired);

        void Expired()
        {
            transform.LeanScale(Vector3.zero, 0.5f).setOnComplete(Annig);
            var renderer = GetComponentInChildren<SpriteRenderer>();
            LeanTween.color(renderer.gameObject, new(), 0.5f);
        }

        void Annig()
        {
            if (gameObject)
            {
                Destroy(gameObject);
            }
        }
    }

    

    private void EnemiesList_Changed()
    {
        if (GameManager.Instance.IsPVP)
        {
            enemies = GameManager.Instance.allEnemies.FindAll(e => e.Team == team);
        }
        else
        {
            enemies = GameManager.Instance.allEnemies;
        }
    }

    private void Update()
    {
        var canDamage = false;
        foreach (var enemy in enemies)
        {
            if (((Vector2)enemy.transform.position - ((Vector2)transform.position + Vector2.up * yOffsetEffectPos)).magnitude < attackDistance)
            {
                if (!canDamage)
                    canDamage = PlayAttackAnim();

                if (canDamage)
                {
                    LeanTween.delayedCall(attackSpeed / 5f, () => Damage(enemy));
                }
            }
        }

        void Damage(Enemy enemy)
        {
            if (enemy != null)
                enemy.Damage(damage);
        }
    }

    void Placed()
    {
        var effect = Instantiate(placedEffect, transform.position, Quaternion.identity);
        effect.transform.localPosition += Vector3.up * yOffsetEffectPos;
    }

    private bool PlayAttackAnim()
    {
        // Получаем информацию о текущем состоянии на базе‑слое 0
        AnimatorStateInfo stateInfo = animator.GetCurrentAnimatorStateInfo(0);

        // Проверяем, что аниматор не в переходе и находится в стейте "Idle"
        if (!animator.IsInTransition(0) && stateInfo.IsName("Idle"))
        {
            animator.SetTrigger(AttackHash);
            return true;
        }
        else
        {
            return false;
        }
    }
}
