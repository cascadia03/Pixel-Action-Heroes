using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class UISortOrder : MonoBehaviour
{
    [SerializeField] private Transform Parent;
    [SerializeField] private Canvas    UI;

    private void Update()
    {
        UI.sortingOrder = -(int)(Parent.position.y * 100);
    }
}
