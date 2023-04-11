using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class CollectionController : MonoBehaviour
{
    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Player") { Destroy(gameObject); ScoreController.score++; }
    }
}