using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AttentionAnim : MonoBehaviour
{
    [SerializeField] Vector3 minScale = Vector3.one * 0.5f;
    [SerializeField] Vector3 maxScale = Vector3.one * 1.1f;
    [SerializeField] float duration = 1f;

    WaitForSeconds halfDuration;

    public void Play()
    {
        if (!gameObject.activeInHierarchy)
            return;

        halfDuration = new WaitForSeconds((duration / 2f) + 0.08f);

        StartCoroutine(Anim());

        IEnumerator Anim()
        {
            transform.LeanScale(minScale, duration / 2f).setEaseInOutQuad();

            yield return halfDuration;

            transform.LeanScale(maxScale, duration / 2f).setEaseInOutQuad();

            yield return halfDuration;

            transform.LeanScale(Vector3.one, duration / 4f).setEaseInOutQuad();
        }

    }
}
