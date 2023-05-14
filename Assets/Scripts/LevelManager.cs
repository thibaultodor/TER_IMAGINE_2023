using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelManager : MonoBehaviour
{
    private GameObject _loadedLevel;
    public GameObject player;
    private static readonly string[] SideToString = { "Left", "Back", "Right", "Front" };

    private int player_index = -1;
    private int w;
    private int h;
    public byte[] layout;
    private bool[] visited;
    private Dictionary<int, Sprite> RoomSprites;

    private const int Wall = 0;
    private const int Room = 1;
    private const int Entrance = 2;
    private const int Exit = 3;

    private uint population_size;
    private uint num_rooms;
    private uint nb_max_generations;
    private double mutation_rate;
    private uint selection_pressure;

    WFC SpriteGenerator;

    private void Start()
    {
        population_size = 100;
        num_rooms = 10;
        nb_max_generations = 100;
        mutation_rate = 0.2;
        selection_pressure = 2;

        w = h = 0;

        while( w < 5 || h < 5 )
            layout = DungeonLayoutGenerator.get_new_layout(population_size, num_rooms, nb_max_generations, mutation_rate, selection_pressure, out w, out h );

        visited = new bool[w * h];
        for (int i = 0; i < w * h; i++)
            visited[i] = false;

        for (int i = 0; i < w * h; i++)
            if (layout[i] == Entrance)
            {
                player_index = i;
                visited[i] = true;
            }

        SpriteGenerator = new WFC( GameObject.FindWithTag("SpriteWFC"), 23, 13, 0, 2);
        RoomSprites = new Dictionary<int, Sprite>();
        SpriteGenerator.CreateRoomSprites(ref RoomSprites, ref layout, w, h, Wall, "WFCSamples/grass");

        if (player_index == -1)
        {
            print("There are no entrances !");
            return;
        }

        _loadedLevel = GameObject.FindWithTag("Level");
        update_cond_walls();
        
        GameObject DungeonEnd = GameObject.FindWithTag("Respawn");
        DungeonEnd.GetComponent<MeshRenderer>().enabled = false;
        DungeonEnd.GetComponent<CapsuleCollider>().enabled = false;
        DungeonEnd.GetComponent<Behaviour>().enabled = false;

        

        update_cond_walls();

        for(int i = 0; i< (w * h); i++)
        {
            print("Layout "+i+" : "+layout[i]);
        }

        GameObject map = GameObject.FindWithTag("Map");
        Vector3 xy = map.transform.position;
        xy = new Vector3(xy.x - 50 + 50.0f/w, xy.y + 50 - 50.0f / h, xy.z);

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

        SpriteRenderer renderer = GameObject.FindWithTag("SpriteWFC").GetComponent<SpriteRenderer>();
        renderer.sprite = RoomSprites[player_index];
    }

    public void ChangeLevel(GameObject prefab, ChangeLevelTrigger.Side side)
    {
        float offset = 3.0f;

        switch ((int)side)
        {
            case 0: player_index++; break;
            case 1: player_index -= w; offset *= -1.0f; break;
            case 2: player_index--; offset *= -1.0f; break;
            default: player_index += w; break;
        }

        if (layout[player_index] == Exit )
        {
            GameObject DungeonEnd = GameObject.FindWithTag("Respawn");
            DungeonEnd.GetComponent<MeshRenderer>().enabled = true;
            DungeonEnd.GetComponent<CapsuleCollider>().enabled = true;
            DungeonEnd.GetComponent<Behaviour>().enabled = true;
        }

        visited[player_index] = true;

        update_cond_walls();

        Transform otherTriggerTF = _loadedLevel.transform
            .GetChild(0)
            .Find("Triggers")
            .Find(SideToString[(int)side]);
        Vector3 position = otherTriggerTF.position;
        if( (int)side == 0 || (int)side == 2 ) position[2] += offset;
        else position[0] += offset;
        player.transform.position = position;
        otherTriggerTF.gameObject.GetComponent<ChangeLevelTrigger>().DisableNextEntry();

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
    }
}