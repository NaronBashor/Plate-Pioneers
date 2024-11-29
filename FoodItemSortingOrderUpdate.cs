using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FoodItemSortingOrderUpdate : MonoBehaviour
{
    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision != null) {
            if (collision.CompareTag("Food")) {
                collision.GetComponent<SpriteRenderer>().sortingLayerName = "Walk In Front";
                collision.GetComponent<SpriteRenderer>().sortingOrder = 3;
            }
        }
    }
}
