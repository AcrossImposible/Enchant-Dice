using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerStone : MonoBehaviour
{
    [SerializeField] ParticleSystem[] tapEffectsPrefabs;

    private void OnMouseDown()
    {
        User.Data.countStones++;
        Destroy(gameObject);
        Saver.Save();
        EventsHolder.onPowerStoneClicked?.Invoke();

        foreach (var item in tapEffectsPrefabs)
        {
            Instantiate(item, transform.position, Quaternion.identity);
        }
    }
}
