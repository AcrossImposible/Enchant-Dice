using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PowerStone : MonoBehaviour
{
    private void OnMouseDown()
    {
        User.Data.countStones++;
        Destroy(gameObject);
        Saver.Save();
        EventsHolder.onPowerStoneClicked?.Invoke();
    }
}
