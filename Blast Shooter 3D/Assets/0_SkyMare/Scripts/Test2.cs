using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
public class Test2 : MonoBehaviour
{
    public List<Collider> colliders;

    private void Start()
    {
         for (int i = 0; i < colliders.Count; i++)
        {
            int index = i;
            DOVirtual.DelayedCall(index * 0.1f, () =>
            {
                colliders[i].enabled = false;
            });
        }
    }
}
