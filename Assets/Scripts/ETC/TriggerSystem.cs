using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TriggerSystem : MonoBehaviour
{
    [HideInInspector] public GameObject Collided = null;

    [HideInInspector] public bool IsActive = false;

    private void OnTriggerStay2D(Collider2D collision)
    {
        if (IsActive)
            Collided = collision.gameObject;
        else 
            Collided = null;
    }
}
