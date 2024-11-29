using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class popUpText : MonoBehaviour
{
    [SerializeField] private float moveSpeed; // Speed at which the UI element moves
    private RectTransform rect;

    private void Start()
    {
        rect = GetComponent<RectTransform>();
    }

    private void Update()
    {
        // Move the RectTransform upwards over time
        rect.anchoredPosition += Vector2.up * moveSpeed * Time.deltaTime;
    }

}
