using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpriteSortOrder : MonoBehaviour
{
    [SerializeField] private SpriteRenderer Renderer;

    private void Update()
    {
        Renderer.sortingOrder = -(int)(transform.position.y * 100);
    }
}
