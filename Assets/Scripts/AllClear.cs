using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AllClear : MonoBehaviour
{
    void AnimationFinished()
    {
        this.gameObject.SetActive(false);
    }
}
