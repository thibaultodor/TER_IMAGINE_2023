using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameObject _loadedLevel;
    public GameObject player;
    private static readonly string[] SideToString = {"Left", "Back", "Right", "Front"};

    private void Start()
    {
        _loadedLevel = GameObject.FindWithTag("Level");
    }

    public void ChangeLevel(GameObject prefab, ChangeLevelTrigger.Side side)
    {
        Destroy(_loadedLevel);
        _loadedLevel = Instantiate(prefab);
        Transform otherTriggerTF = _loadedLevel.transform
            .GetChild(0)
            .Find("Triggers")
            .Find(SideToString[(int)side]);
        Vector3 position = otherTriggerTF.position;
        player.transform.position = position;
        otherTriggerTF.gameObject.GetComponent<ChangeLevelTrigger>().DisableNextEntry();
    }
}
