using System;
using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class ChangeLevelTrigger : MonoBehaviour
{
    public GameObject levelPrefab;
    public enum Side { Left, Back, Right, Front };
    public Side side;
    private bool _nextEntryDisabled = false;

    private void OnTriggerEnter(Collider other)
    {
        if (other.gameObject.CompareTag("Player"))
        {
            if (levelPrefab == null)
            {
                Debug.Log("No level for this trigger");
                //return;
            }

            if (_nextEntryDisabled)
            {
                _nextEntryDisabled = false;
                return;
            }
            
            GameObject manager = GameObject.Find("LevelManager");
            
            Side otherSide = Side.Left;
            if (side == Side.Left)
                otherSide = Side.Right;
            else if (side == Side.Right)
                otherSide = Side.Left;
            else if (side == Side.Back)
                otherSide = Side.Front;
            else if (side == Side.Front)
                otherSide = Side.Back;

            manager.GetComponent<LevelManager>().ChangeLevel(levelPrefab, otherSide);
        }
    }

    public void DisableNextEntry()
    {
        _nextEntryDisabled = true;
    }
}
