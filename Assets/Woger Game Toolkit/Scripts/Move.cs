using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class Move : MonoBehaviour
{
    public void MoveProjectile(Transform projectile, Vector3 targetPos, float speed, Action callback = null)
    {
        StartCoroutine(MoveRoutine(projectile, targetPos, speed, callback));
    }

    public IEnumerator MoveRoutine(Transform projectile, Vector3 targetPos, float speed, Action callback = null)
    {
        while (Vector3.Distance(projectile.position, targetPos) > 0.01f)
        {
            projectile.position = Vector3.MoveTowards(projectile.position, targetPos, speed * Time.deltaTime);
            yield return null;
        }

        // Достиг цели
        callback?.Invoke();
    }
}
