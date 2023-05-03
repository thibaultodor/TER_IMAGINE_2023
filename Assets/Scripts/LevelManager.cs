using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameObject _loadedLevel;
    public GameObject player;
    private static readonly string[] SideToString = { "Left", "Back", "Right", "Front" };

    private int player_index = -1;
    private int w;
    private int h;
    public byte[] layout;

    private const int Wall = 0;
    private const int Room = 1;
    private const int Entrance = 2;
    private const int Exit = 3;

    private uint population_size;
    private uint num_rooms;
    private uint nb_max_generations;
    private double mutation_rate;
    private uint selection_pressure;

    private void Start()
    {
        population_size = 100;
        num_rooms = 10;
        nb_max_generations = 100;
        mutation_rate = 0.2;
        selection_pressure = 2;

        layout = DungeonLayoutGenerator.get_new_layout(population_size, num_rooms, nb_max_generations, mutation_rate, selection_pressure, out w, out h );

        for (int i = 0; i < w * h && player_index == -1; i++)
            if (layout[i] == Entrance)
                player_index = i;

        if (player_index == -1)
        {
            print("There are no entrances !");
            return;
        }

        _loadedLevel = GameObject.FindWithTag("Level");
        update_cond_walls();
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
    }

    public void ChangeLevel(GameObject prefab, ChangeLevelTrigger.Side side)
    {
        switch ((int)side)
        {
            case 0: player_index++; break;
            case 1: player_index -= w; break;
            case 2: player_index--; break;
            default: player_index += w; break;
        }


        update_cond_walls();
        /*
        Destroy(_loadedLevel);
        _loadedLevel = Instantiate(prefab);
        */
        Transform otherTriggerTF = _loadedLevel.transform
            .GetChild(0)
            .Find("Triggers")
            .Find(SideToString[(int)side]);
        Vector3 position = otherTriggerTF.position;
        player.transform.position = position;
        otherTriggerTF.gameObject.GetComponent<ChangeLevelTrigger>().DisableNextEntry();


    }
}