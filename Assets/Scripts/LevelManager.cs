using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelManager : MonoBehaviour
{
    private GameObject _loadedLevel;
    public GameObject player;
    private static readonly string[] SideToString = {"Left", "Back", "Right", "Front"};

    private int player_index = -1;
    private int dim_level = 3;
    public enum Room { Wall, Room, Entrance, Exit };
    public Room[] layout = { Room.Wall,     Room.Room, Room.Wall,
                             Room.Room,     Room.Room, Room.Exit, 
                             Room.Wall, Room.Entrance, Room.Wall };

    private uint population_size;
    private uint chromosomes_lenght;
    private uint nb_max_generations;
    private float mutation_rate;
    private uint selection_pressure;

    private void Start()
    {
        for (int i = 0; i < dim_level * dim_level && player_index == -1; i++)
            if (layout[i] == Room.Entrance)
                player_index = i;
        
        if( player_index == -1 )
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
        Condition = (player_index % dim_level != 0) && (layout[player_index - 1] != Room.Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Left = " + !Condition);

        CondWall = GameObject.FindWithTag("BackCondWall");
        Condition = (player_index < (dim_level * (dim_level - 1))) && (layout[player_index + dim_level] != Room.Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Back = " + !Condition);

        CondWall = GameObject.FindWithTag("RightCondWall");
        Condition = (player_index % dim_level != dim_level - 1) && (layout[player_index + 1] != Room.Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Right = " + !Condition);

        CondWall = GameObject.FindWithTag("FrontCondWall");
        Condition = (player_index > dim_level - 1) && (layout[player_index - dim_level] != Room.Wall);
        CondWall.GetComponent<Renderer>().enabled = !Condition;
        CondWall.GetComponent<Collider>().enabled = !Condition;
        print("Front = " + !Condition);
    }

    public void ChangeLevel(GameObject prefab, ChangeLevelTrigger.Side side)
    {
        switch ((int)side)
        {
            case 0: player_index++; break;
            case 1: player_index += dim_level; break;
            case 2: player_index--; break;
            default: player_index -= dim_level; break;
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
