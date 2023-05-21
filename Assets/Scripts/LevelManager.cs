using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Random = System.Random;

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
    private Dictionary<int, Sprite> RoomSprites;
    private static readonly string[] biomes = { "grass", "rock", "snow", "sea"  };
    private int cur_biomes_idx;

    public GameObject EnemyPrefab;
    private int nb_enemies;
    private int max_enemies = 3;
    private int[] rooms_enemies;

    private Vector3 O, u, v;
    private Color WallColor;
    private int wFloor, hFloor, w_begin, h_begin, w_end, h_end;
    public GameObject WallPrefab;

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

        w = h = 1;
        layout[0] = Entrance;

        visited = new bool[w * h];
        for (int i = 0; i < w * h; i++)
            visited[i] = false;

        Random rand = new Random();

        rooms_enemies = new int[w * h];
        for (int i = 0; i < w * h; i++)
            if (layout[i] != Exit)
                rooms_enemies[i] = rand.Next() % max_enemies + 1;

        for (int i = 0; i < w * h; i++)
            if (layout[i] == Entrance)
            {
                player_index = i;
                visited[i] = true;
            }

        wFloor = 23;
        hFloor = 13;
        SpriteGenerator = new WFC( wFloor, hFloor, 0, 2);
        RoomSprites = new Dictionary<int, Sprite>();
        cur_biomes_idx = rand.Next()%biomes.Length;
        Set_Sprite("WFCSamples/" + biomes[cur_biomes_idx]);
        SpriteGenerator.CreateRoomSprites(ref RoomSprites, ref layout, w, h, Wall, "WFCSamples/" + biomes[cur_biomes_idx]);

        if (player_index == -1)
        {
            print("There are no entrances !");
            return;
        }

        _loadedLevel = GameObject.FindWithTag("Level");
        
        GameObject DungeonEnd = GameObject.FindWithTag("Respawn");
        DungeonEnd.GetComponent<MeshRenderer>().enabled = false;
        DungeonEnd.GetComponent<CapsuleCollider>().enabled = false;
        DungeonEnd.GetComponent<Behaviour>().enabled = false;
        update_cond_walls();

        GameObject map = GameObject.FindWithTag("Map");
        Vector3 xy = map.transform.position;
        xy = new Vector3(xy.x - 50 + 50.0f/w, xy.y + 50 - 50.0f / h, xy.z);

        for(int i = 0; i < h; i++)
        {
            for(int j = 0; j < w; j++)
            {
                GameObject titleMap = new GameObject("titleMap");
                titleMap.AddComponent<RawImage>();
                titleMap.AddComponent<PulseColor>();
                titleMap.transform.position = new Vector3(xy.x + (float)((100/w)*j), xy.y - (float)((100 / h) * i), xy.z);
                titleMap.transform.localScale = new Vector3((float)1 / w, (float)1 / h, 1);

                titleMap.GetComponent<RawImage>().color = new Color32(0, 0, 0, 0);
                titleMap.GetComponent<PulseColor>().setRed(false);
                if ((j + (w * i))==player_index)
                {
                    titleMap.GetComponent<PulseColor>().setRed(true);
                    titleMap.GetComponent<RawImage>().color = new Color32(255, 0, 0, 255);
                }

                titleMap.transform.SetParent(map.transform);
            }
        }
    }

    private void update_cond_walls()
    {
        GameObject[] CondWall;
        bool Condition;

        CondWall = GameObject.FindGameObjectsWithTag("LeftCondWall");
        Condition = (player_index % w != 0) && (layout[player_index - 1] != Wall);
        for(int i = 0; i < CondWall.Length; i++)
        {
            CondWall[i].GetComponent<Renderer>().enabled = !Condition;
            CondWall[i].GetComponent<Collider>().enabled = !Condition;
        }

        CondWall = GameObject.FindGameObjectsWithTag("BackCondWall");
        Condition = (player_index < (w * (h - 1))) && (layout[player_index + w] != Wall);
        for (int i = 0; i < CondWall.Length; i++)
        {
            CondWall[i].GetComponent<Renderer>().enabled = !Condition;
            CondWall[i].GetComponent<Collider>().enabled = !Condition;
        }

        CondWall = GameObject.FindGameObjectsWithTag("RightCondWall");
        Condition = (player_index % w != w - 1) && (layout[player_index + 1] != Wall);
        for (int i = 0; i < CondWall.Length; i++)
        {
            CondWall[i].GetComponent<Renderer>().enabled = !Condition;
            CondWall[i].GetComponent<Collider>().enabled = !Condition;
        }

        CondWall = GameObject.FindGameObjectsWithTag("FrontCondWall");
        Condition = (player_index > w - 1) && (layout[player_index - w] != Wall);
        for (int i = 0; i < CondWall.Length; i++)
        {
            CondWall[i].GetComponent<Renderer>().enabled = !Condition;
            CondWall[i].GetComponent<Collider>().enabled = !Condition;
        }

        
        BuildRoom(RoomSprites[player_index]);
    }

    public void ChangeLevel(GameObject prefab, ChangeLevelTrigger.Side side)
    {
        cleanRoom();

        float offset = 2.3f;

        switch ((int)side)
        {
            case 0: player_index++; break;
            case 1: player_index -= w; offset *= -1.0f; break;
            case 2: player_index--; offset *= -1.0f; break;
            default: player_index += w; break;
        }

        if( layout[player_index] == Exit )
        {
            GameObject DungeonEnd = GameObject.FindWithTag("Respawn");
            DungeonEnd.GetComponent<MeshRenderer>().enabled = true;
            DungeonEnd.GetComponent<CapsuleCollider>().enabled = true;
            DungeonEnd.GetComponent<Behaviour>().enabled = true;
        }
        else
        {
            GameObject DungeonEnd = GameObject.FindWithTag("Respawn");
            DungeonEnd.GetComponent<MeshRenderer>().enabled = false;
            DungeonEnd.GetComponent<CapsuleCollider>().enabled = false;
            DungeonEnd.GetComponent<Behaviour>().enabled = false;
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
        //otherTriggerTF.gameObject.GetComponent<ChangeLevelTrigger>().DisableNextEntry();

        GameObject map = GameObject.FindWithTag("Map");

        for (int i = 0; i < (w * h); i++)
        {
            if (visited[i])
            {
                map.GetComponentsInChildren<RawImage>()[i].GetComponent<RawImage>().color = new Color32(255,255,255, 255);
                map.GetComponentsInChildren<PulseColor>()[i].setRed(false);
            }
            if (i == player_index)
            {
                map.GetComponentsInChildren<RawImage>()[i].GetComponent<RawImage>().color = new Color32(255, 0, 0, 255);
                map.GetComponentsInChildren<PulseColor>()[i].setRed(true);
            }
            map.GetComponentsInChildren<PulseColor>()[i].Pulse();
        }

        //print(rooms_enemies[player_index]);

        if ( rooms_enemies[player_index] != 0 )
        {
            Random rand = new Random();
            int nbWP = 4;
            GameObject[] wandarPoints = new GameObject[nbWP];
            for (int i = 0; i < nbWP; i++)
                wandarPoints[i] = GameObject.Find("WandarPoint" + (i+1));

            List<int> idx = new List<int>();

            for ( int i = 0; i < rooms_enemies[player_index]; i++ )
            {
                int rand_idx = rand.Next()%nbWP;
                while(rand_idx == (int)side || idx.Contains(rand_idx))
                    rand_idx = rand.Next() % nbWP;

                GameObject E = Instantiate(EnemyPrefab, new Vector3(wandarPoints[rand_idx].transform.position[0], (float)0.0, wandarPoints[rand_idx].transform.position[2]), transform.rotation);
                E.SetActive(true);
                idx.Add(rand_idx);
            }
        }
    }

    private void cleanRoom()
    {
        GameObject[] enemies = GameObject.FindGameObjectsWithTag("Enemy");
        int nbEnemiesDeadState = 0;
        for (int i = 0; i < enemies.Length; i++)
            if (enemies[i].GetComponent<EnemyBehaviour>().getbDead()){nbEnemiesDeadState++;}
        rooms_enemies[player_index] = enemies.Length - nbEnemiesDeadState;
        for (int i = 0; i < enemies.Length; i++)
            Destroy(enemies[i], 0.0f);
        GameObject[] allyBullet = GameObject.FindGameObjectsWithTag("AllyBullet");
        for (int i = 0; i < allyBullet.Length; i++)
            Destroy(allyBullet[i], 0.0f);
        GameObject[] EnemyBullet = GameObject.FindGameObjectsWithTag("EnemyBullet");
        for (int i = 0; i < EnemyBullet.Length; i++)
            Destroy(EnemyBullet[i], 0.0f);
        GameObject[] Obstacles = GameObject.FindGameObjectsWithTag("Obstacles");
        for (int i = 0; i < Obstacles.Length; i++)
            Destroy(Obstacles[i], 0.0f);
    }

    void Set_Sprite(Sprite s)
    {
        Color[] pixels = s.texture.GetPixels();
        Dictionary<Color, int> histogram = new Dictionary<Color, int>();

        int out_trash;
        int max_color = -1;

        for (int i = 0; i < pixels.Length; i++)
            if (histogram.TryGetValue(pixels[i], out out_trash))
            {
                histogram[pixels[i]]++;

                if (max_color < histogram[pixels[i]])
                {
                    max_color = histogram[pixels[i]];
                    WallColor = pixels[i];
                }
            }
            else
                histogram.Add(pixels[i], 1);

        GameObject Floor = GameObject.FindWithTag("Floor");
        List<Vector3> FloorVertices = new List<Vector3>(Floor.GetComponent<MeshFilter>().mesh.vertices);
        Vector3 up_left_p = Floor.transform.TransformPoint(FloorVertices[0]);
        Vector3 up_right_p = Floor.transform.TransformPoint(FloorVertices[10]);
        Vector3 bot_left_p = Floor.transform.TransformPoint(FloorVertices[110]);
        O = up_left_p;
        u = up_right_p - up_left_p;
        v = bot_left_p - up_left_p;
    }

    void Set_Sprite(String s)
    {
        Color[] pixels = Resources.Load<Texture2D>(s).GetPixels();
        Dictionary<Color, int> histogram = new Dictionary<Color, int>();

        int out_trash;
        int max_color = -1;

        for (int i = 0; i < pixels.Length; i++)
            if (histogram.TryGetValue(pixels[i], out out_trash))
            {
                histogram[pixels[i]]++;

                if (max_color < histogram[pixels[i]])
                {
                    max_color = histogram[pixels[i]];
                    WallColor = pixels[i];
                }
            }
            else
                histogram.Add(pixels[i], 1); 

        GameObject Floor = GameObject.FindWithTag("Floor");
        List<Vector3> FloorVertices = new List<Vector3>(Floor.GetComponent<MeshFilter>().mesh.vertices);
        Vector3 up_left_p = Floor.transform.localToWorldMatrix * Floor.GetComponent<MeshFilter>().mesh.vertices[120];
        Vector3 up_right_p = Floor.transform.localToWorldMatrix * Floor.GetComponent<MeshFilter>().mesh.vertices[10];
        Vector3 bot_left_p = Floor.transform.localToWorldMatrix * Floor.GetComponent<MeshFilter>().mesh.vertices[110];
        O = bot_left_p;
        u = up_right_p - up_left_p;
        v = up_left_p - bot_left_p;
    }

    public void BuildRoom(Sprite s)
    {
        /*
        GameObject A = Instantiate(WallPrefab, O, GameObject.FindWithTag("Floor").transform.rotation) as GameObject;
        A.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
        A.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);
        GameObject B = Instantiate(WallPrefab, O + u, GameObject.FindWithTag("Floor").transform.rotation) as GameObject;
        B.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue); 
        B.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.blue);
        GameObject C = Instantiate(WallPrefab, O + v, GameObject.FindWithTag("Floor").transform.rotation);
        C.GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
        C.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.green);
        A.transform.localScale = B.transform.localScale = C.transform.localScale = new Vector3(5, 5, 5);
        */

        if (layout[player_index] == Exit)
            return;

        int[] leave_a_path = { 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                               1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                               1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1,
                               1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1,
                               1, 1, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1,
                               1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                               1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1,
                               1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                               1, 1, 0, 0, 1, 1, 1, 1, 1, 0, 0, 0, 0, 0, 1, 1, 1, 1, 0, 0, 0, 1, 1,
                               1, 1, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 1, 1,
                               1, 1, 0, 0, 0, 0, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 0, 0, 0, 0, 1, 1,
                               1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1,
                               1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1, 1 };

        Color[] pixels = s.texture.GetPixels();
        print(pixels.Length);

        SpriteRenderer renderer = GameObject.FindWithTag("SpriteWFC").GetComponent<SpriteRenderer>();
        renderer.sprite = s;

        for ( int i = 0; i < hFloor; i++ )
            for( int j = 0; j < wFloor; j++ )
            {
                Color pixel = pixels[i * wFloor + j];

                if (pixel == WallColor && leave_a_path[i * wFloor + j] == 0 )
                {
                    Vector3 pos = O + ((((float)i)+0.5f) / (float)(hFloor))*v + ((((float)j) + 0.236f) / (float)(wFloor))* u;
                    pos[1] += .745f ;
                    GameObject Wall = Instantiate(WallPrefab, pos, new Quaternion(0, 0, -1f, 1));
                    Wall.transform.localScale = new Vector3(2.58f*2.05f, 2.58f, 2.58f);
                    Wall.GetComponent<MeshRenderer>().material.SetColor("_Color", WallColor);
                    Wall.transform.GetChild(0).GetComponent<MeshRenderer>().material.SetColor("_Color", WallColor);

                }
            }

    }
}