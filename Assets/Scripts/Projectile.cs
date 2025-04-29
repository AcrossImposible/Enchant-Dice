using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Projectile : MonoBehaviour
{
    [SerializeField] SpriteRenderer spriteRenderer;

    public Action<Enemy> onDamage;

    Enemy target;
    Vector3 dir;
    float speed = 3.99f;
    float lifetime;
    int damage;

    public void Init(Enemy target, int damage, Color color)
    {
        this.target = target;
        this.damage = damage;

        spriteRenderer.color = color;
    }

    private void Update()
    {
        if (target)
        {
            dir = target.transform.position - transform.position;
            
            dir.z = 0;

            var dist = Vector2.Distance(transform.position, target.transform.position);

            if (dist < 0.3f)
            {
                target.Damage(damage);

                onDamage?.Invoke(target);

                Destroy(gameObject);
            }
        }

        dir.Normalize(); 
        dir *= 5.5f;
        transform.position += speed * Time.deltaTime * dir;

        if (dir == Vector3.zero)
            Destroy(gameObject);

        lifetime += Time.deltaTime;
        if (lifetime > 5.7f)
            Destroy(gameObject);
    }
}
