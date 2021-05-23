using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;

public class CustomCamera : MonoBehaviour
{
    private Transform mTarget = null;

    private Vector3 ShakePos = Vector3.zero;

    private void Update()
    {
        Targetting();
    }

    private void FixedUpdate()
    {
        CameraShake();
    }

    private void Targetting()
    {
        if (mTarget)
            transform.position = new Vector3(mTarget.position.x, mTarget.position.y, -10) + ShakePos;
        else
            foreach (GameObject Iter in GameObject.FindGameObjectsWithTag("Player"))
            {
                if (Iter.GetComponent<PhotonView>().IsMine)
                    mTarget = Iter.transform;
            }
    }
    private void CameraShake()
    {
        if (mTarget)
            if (mTarget.GetComponent<Player>().IsDamaged)
            {
                ShakePos = new Vector3(Random.Range(-0.1f, 0.1f), Random.Range(-0.1f, 0.1f), 0);
                return;
            }

        ShakePos = Vector3.zero;
    }
}
