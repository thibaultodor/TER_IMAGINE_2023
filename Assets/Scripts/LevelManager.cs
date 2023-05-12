using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private GameObject _loadedLevel;
    public GameObject player;
    private static readonly string[] SideToString = {"Left", "Back", "Right", "Front"};

    private void Start()
    {
        _loadedLevel = GameObject.FindWithTag("Level");
<<<<<<< Updated upstream
=======
        update_cond_walls();

        for(int i = 0; i< (w * h); i++)
        {
            print("Layout "+i+" : "+layout[i]);
        }

        GameObject map = GameObject.FindWithTag("Map");
        Vector3 xy = map.transform.position;
        xy = new Vector3(xy.x - 50, xy.y + 50, xy.z);

        for(int i = 0; i < h; i++)
        {
            for(int j = 0; j < w; j++)
            {
                GameObject titleMap = new GameObject("titleMap");
                titleMap.AddComponent<RawImage>();
                titleMap.transform.position = new Vector3(xy.x + (float)((100/w)*j), xy.y - (float)((100 / h) * i), xy.z);
                titleMap.transform.localScale = new Vector3((float)1 / w, (float)1 / h, 1);

                titleMap.GetComponent<RawImage>().color = new Color32(0, 0, 0, 0);
                if ((j + (w * i))==player_index)
                {
                    titleMap.GetComponent<RawImage>().color = new Color32(255, 0, 0, 255);
                }

                titleMap.transform.SetParent(map.transform);
            }
        }
    }

    private void update_cond_walls()
    {
        GameObject CondWall;
        bool Condition;
        print(player_index);

        CondWall = GameObject.FindWithTag("LeftCondWall");
        Condition = (player_index % w != 0) && (layout[player_index - 1] != Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Left = " + !Condition);

        CondWall = GameObject.FindWithTag("BackCondWall");
        Condition = (player_index < (w * (h - 1))) && (layout[player_index + w] != Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Back = " + !Condition);

        CondWall = GameObject.FindWithTag("RightCondWall");
        Condition = (player_index % w != w - 1) && (layout[player_index + 1] != Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Right = " + !Condition);

        CondWall = GameObject.FindWithTag("FrontCondWall");
        Condition = (player_index > w - 1) && (layout[player_index - w] != Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Front = " + !Condition);
>>>>>>> Stashed changes
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
<<<<<<< Updated upstream
=======

        GameObject map = GameObject.FindWithTag("Map");

        for (int i = 0; i < (w * h); i++)
        {
            if (visited[i])
            {
                map.GetComponentsInChildren<RawImage>()[i].GetComponent<RawImage>().color = new Color32(255,255,255, 255);
            }
            if (i == player_index)
            {
                map.GetComponentsInChildren<RawImage>()[i].GetComponent<RawImage>().color = new Color32(255, 0, 0, 255);
            }
        }
>>>>>>> Stashed changes
    }
}
