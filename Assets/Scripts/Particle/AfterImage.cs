using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AfterImage : MonoBehaviour
{
    private ParticleSystem mParticle;

    private void Awake()
    {
        mParticle = GetComponent<ParticleSystem>();
    }

    private void Update()
    {
        ParticleSystem.MainModule main = mParticle.main;

        if (main.startRotation.mode == ParticleSystemCurveMode.Constant)
        {
            main.startRotation = -transform.eulerAngles.z * Mathf.Deg2Rad;
        }
    }
}
