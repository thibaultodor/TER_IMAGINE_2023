// See https://aka.ms/new-console-template for more information

using System;

public class DungeonLayoutGenerator
{

    private uint POP_SIZE;
    private uint NUM_ROOMS;
    private uint CHROM_LENGTH;
    private uint MAX_GENS;
    private double MUTATION_RATE;
    private uint SELECTION_PRESSURE;
    private Random rand;

    private const int Wall = 0;
    private const int Room = 1;
    private const int Entrance = 2;
    private const int Exit = 3;

    public DungeonLayoutGenerator( uint pop_size, uint num_rooms, uint max_gens, 
                                   double mutation_rate, uint selection_pressure )
    {
        POP_SIZE = pop_size;
        NUM_ROOMS = num_rooms;
        CHROM_LENGTH = num_rooms*num_rooms;
        MAX_GENS = max_gens;
        MUTATION_RATE = mutation_rate;
        SELECTION_PRESSURE = selection_pressure;
        rand = new Random();
    }

    /////////////////////////// FITNESS FUNCTION
    int fitness( byte[] pop, long idx )
    {
        byte[] layout = new byte[CHROM_LENGTH];

        for( int x = 0; x < CHROM_LENGTH; x++ )  
            layout[x] = pop[idx+x];
        
        int k = 1;
        int nb_neighbours = 0;
        int empty_rooms = 0;

        for( int x = 0; x < NUM_ROOMS; x++ ) 
            for( int y = 0; y < NUM_ROOMS; y++ ) 
                if( layout[x*NUM_ROOMS+y] != Wall ) 
                {
                    for( int i = -k; i >= k; i++ )
                        for( int j = -k; j <= k; j++ )
                            if( x+i > 0 && x+i < NUM_ROOMS && y+j > 0 && y+j < NUM_ROOMS )
                                nb_neighbours += (layout[(x+i)*NUM_ROOMS+(y+j)]!=Wall)?(1):(-1);  
                }
                else 
                    empty_rooms++;

        return nb_neighbours/(empty_rooms*empty_rooms);
    }

    /////////////////////////// GENETIC OPERATIONS
    void mutate( byte[] chrom )
    {
        for( int i = 0; i < CHROM_LENGTH; i++ ) 
            if( rand.NextDouble() < MUTATION_RATE ) 
            {
                if( chrom[i] == Wall )
                    chrom[i] = Room;
                else 
                    chrom[i] = Wall;
            }
    }

    void crossover( byte[] parent1, byte[] parent2, byte[] child )
    {
        for( int i = 0; i < CHROM_LENGTH; i++ ) 
            if( rand.NextDouble() < 0.5 ) 
                child[i] = parent1[i];
            else 
                child[i] = parent2[i];
    }

    /////////////////////////// SELECTION METHOD
    void tournament_selection( byte[] pop, int[] fitness_vals, byte[] parent1, byte[] parent2 )
    {
        for( int i = 0; i < SELECTION_PRESSURE; i++ ) 
        {
            int idx = 0;
            int best_fit = fitness_vals[idx];

            for( int j = 0; j < SELECTION_PRESSURE-1; j++ ) 
            {
                idx = (int) (rand.Next()%POP_SIZE);

                if( fitness_vals[idx] > best_fit ) 
                {
                    best_fit = fitness_vals[idx];

                    for( int k = 0; k < CHROM_LENGTH; k++ )
                        parent1[k] = pop[idx*CHROM_LENGTH+k];
                }
            }

            if( i != 0 ) 
            {
                for( int k = 0; k < CHROM_LENGTH; k++ )
                    parent2[k] = pop[idx*CHROM_LENGTH+k];
            }
            else 
            {
                for( int k = 0; k < CHROM_LENGTH; k++ )
                    parent1[k] = pop[idx*CHROM_LENGTH+k];
            }
        }
    }

    /////////////////////////// LAYOUT GENERATION
    public byte[] generate_layout()
    {
        byte[] pop = new byte[POP_SIZE  * CHROM_LENGTH];
        for( int i = 0; i < POP_SIZE*CHROM_LENGTH; i++ ) 
            if( rand.Next()%2 == 0 )
                pop[i] = Room;
            else 
                pop[i] = Wall;
            
        int[] fitness_vals = new int[ POP_SIZE ];
        for( int i = 0; i < POP_SIZE; i++ ) 
            fitness_vals[i] = fitness( pop, i*CHROM_LENGTH );

        byte[] parent1 = new byte[CHROM_LENGTH];
        byte[] parent2 = new byte[CHROM_LENGTH];
        byte[] child = new byte[CHROM_LENGTH];

        for( int gen = 0; gen < MAX_GENS; gen++ )
        {
            tournament_selection( pop, fitness_vals, parent1, parent2 );
            
            crossover( parent1, parent2, child );
            
            mutate( child );
            
            int child_fitness = fitness( child, 0 );
            
            int min_fit = fitness_vals[0];
            int min_idx = 0;
            for( int i = 1; i < POP_SIZE; i++ ) 
                if( fitness_vals[i] < min_fit ) 
                {
                    min_fit = fitness_vals[i];
                    min_idx = i;
                }
            
            if( child_fitness > min_fit )
            {
                for( int i = 0; i < CHROM_LENGTH; i++ ) 
                    pop[min_idx*CHROM_LENGTH+i] = child[i];
                
                fitness_vals[min_idx] = child_fitness;
            }
        }

        int max_fit = fitness_vals[0];
        int max_idx = 0;
        for( int i = 1; i < POP_SIZE; i++ ) 
            if (fitness_vals[i] > max_fit) 
            {
                max_fit = fitness_vals[i];
                max_idx = i;
            }

        byte[] layout;
        layout = new byte[CHROM_LENGTH];
        for( int i = 0; i < CHROM_LENGTH; i++ ) 
            layout[i] = pop[max_fit*CHROM_LENGTH+i];

        return layout;
    }

    /////////////////////////// Selects the largest chunk of the dungeon layout
    void Find_Connex_Rooms( byte[] visited, int cur_idx, ref int Entrance_idx, ref int Exit_idx, ref int max_rooms,
                            ref int best_distance, ref int nb_rooms_visited, int[] rooms_visited_idx, int nb_rooms_traversed )
    {
        if( visited[cur_idx] == 1 )
            return;

        visited[cur_idx] = 1;
        rooms_visited_idx[nb_rooms_visited] = cur_idx;
        nb_rooms_visited++;

        for( int i = 0; i < nb_rooms_visited; i++ )
            {
                int tmp_dist = (int) ( Math.Abs((cur_idx%NUM_ROOMS)-(rooms_visited_idx[i]%NUM_ROOMS)) 
                                     + Math.Abs((cur_idx/NUM_ROOMS)-(rooms_visited_idx[i]/NUM_ROOMS)) );

                if( ( (tmp_dist == best_distance) && (rand.Next()%2) != 0 ) || tmp_dist > best_distance 
                 || ( nb_rooms_traversed > NUM_ROOMS  && (rand.Next()%2) != 0 ) )
                {
                    Entrance_idx = cur_idx;
                    Exit_idx = rooms_visited_idx[i];
                    best_distance = tmp_dist;
                    max_rooms = nb_rooms_visited;
                }
            }
        
        for( int neighbour = 0; neighbour < 4; neighbour++ )
            switch( (rand.Next()+neighbour)%4 )
            {
                case 0 : if( cur_idx%NUM_ROOMS != 0 && visited[cur_idx-1] == 0 )
                            Find_Connex_Rooms( visited, cur_idx-1, ref Entrance_idx, ref Exit_idx, ref max_rooms, ref best_distance, ref nb_rooms_visited, rooms_visited_idx, nb_rooms_traversed+1 );
                         break;
                case 1 : if( cur_idx%NUM_ROOMS != NUM_ROOMS-1 && visited[cur_idx+1] == 0 )
                            Find_Connex_Rooms( visited, cur_idx+1, ref Entrance_idx, ref Exit_idx, ref max_rooms, ref best_distance, ref nb_rooms_visited, rooms_visited_idx, nb_rooms_traversed+1 );
                         break;
                case 2 : if( cur_idx >= NUM_ROOMS && visited[cur_idx-NUM_ROOMS] == 0 )
                            Find_Connex_Rooms( visited, (int) (cur_idx-NUM_ROOMS), ref Entrance_idx, ref Exit_idx, ref max_rooms, ref best_distance, ref nb_rooms_visited, rooms_visited_idx, nb_rooms_traversed+1 );
                         break;
                default : if( cur_idx/NUM_ROOMS < NUM_ROOMS-1 && visited[cur_idx+NUM_ROOMS] == 0 )
                            Find_Connex_Rooms( visited, (int) (cur_idx+NUM_ROOMS), ref Entrance_idx, ref Exit_idx, ref max_rooms, ref best_distance, ref nb_rooms_visited, rooms_visited_idx, nb_rooms_traversed+1 );
                          break;
            }
        }

    /////////////////////////// creates a new dungeon layout from the dungeon contained in a layout
    void Extract_Dungeon( byte[] layout, byte[] visited, byte[] new_layout, int cur_idx, ref int min_x, ref int max_x, ref int min_y, ref int max_y )
    {
        visited[cur_idx] = 1;

        if( layout[cur_idx] == Wall )
            return;
        
        new_layout[cur_idx] = layout[cur_idx];

        int cur_x = (int) (cur_idx%NUM_ROOMS);
        if( cur_x < min_x ) min_x = cur_x;
        if( cur_x > max_x ) max_x = cur_x;

        int cur_y = (int) (cur_idx/NUM_ROOMS);
        if( cur_y < min_y ) min_y = cur_y;
        if( cur_y > max_y ) max_y = cur_y;
        
        if( cur_idx%NUM_ROOMS != 0 && visited[cur_idx-1] == 0 )
            Extract_Dungeon( layout, visited, new_layout, cur_idx-1, ref min_x, ref max_x, ref min_y, ref max_y );
        
        if( cur_idx%NUM_ROOMS != NUM_ROOMS-1 && visited[cur_idx+1] == 0 )
            Extract_Dungeon( layout, visited, new_layout, cur_idx+1, ref min_x, ref max_x, ref min_y, ref max_y );

        if( cur_idx >= NUM_ROOMS && visited[cur_idx-NUM_ROOMS] == 0 )
            Extract_Dungeon( layout, visited, new_layout, (int) (cur_idx-NUM_ROOMS), ref min_x, ref max_x, ref min_y, ref max_y );

        if( cur_idx/NUM_ROOMS < NUM_ROOMS-1 && visited[cur_idx+NUM_ROOMS] == 0 )
            Extract_Dungeon( layout, visited, new_layout, (int) (cur_idx+NUM_ROOMS), ref min_x, ref max_x, ref min_y, ref max_y );
    }

    /////////////////////////// Creates a proper dungeon layout from the data returned by generate_dungeon()
    public byte[] Find_Entrance_And_Exit( byte[] layout, out int w, out int h )
    {
        byte[] visited = new byte[CHROM_LENGTH];

        for( int i = 0; i < CHROM_LENGTH; i++ )
            if( layout[i] == Wall )
                visited[i] = 1;
            else 
                visited[i] = 0;

        int[] rooms_visited_idx = new int[CHROM_LENGTH];
        int Entrance_idx, Exit_idx, max_rooms, nb_rooms_visited, best_distance;
        Entrance_idx = Exit_idx = best_distance = max_rooms = 0;
        
        for( int i = 0; i < CHROM_LENGTH; i++ )
        {
            nb_rooms_visited = 0;
            Find_Connex_Rooms( visited, i, ref Entrance_idx, ref Exit_idx, ref max_rooms, ref best_distance, ref nb_rooms_visited, rooms_visited_idx, 1 );
        }

        /*Entrance offset*/

        for( int i = 2; (rand.Next()%i) != 0 || i == 2; i++ )
        {
            int rand_num = (int) rand.Next();
            bool moved = false;

            while( moved == false )
                switch( rand_num%4 )
                {
                    case 0 : if( Entrance_idx%NUM_ROOMS != 0 && layout[Entrance_idx-1] == Room )
                             {
                                 Entrance_idx--;
                                 moved = true;
                             }
                             else 
                                 rand_num++;
                             break;
                    case 1 : if( Entrance_idx%NUM_ROOMS != NUM_ROOMS-1 && layout[Entrance_idx+1] == Room )
                             {
                                 Entrance_idx++;
                                 moved = true;
                             }
                             else 
                                 rand_num++;
                             break;
                    case 2 : if( Entrance_idx >= NUM_ROOMS && layout[Entrance_idx-NUM_ROOMS] == Room )
                             {
                                 Entrance_idx = (int) (Entrance_idx - NUM_ROOMS);
                                 moved = true;
                             }
                             else 
                                 rand_num++;
                             break;
                    default : if( Entrance_idx < NUM_ROOMS*(NUM_ROOMS-1) && layout[Entrance_idx+NUM_ROOMS] == Room )
                              {
                                  Entrance_idx = (int) (Entrance_idx + NUM_ROOMS);
                                  moved = true;
                              }
                              else 
                                  rand_num++;
                              break;
                } 
        }
        
        layout[Entrance_idx] = Entrance;
        layout[Exit_idx] = Exit;

        int min_x, max_x, max_y, min_y;
        max_x = max_y = -1;
        min_x = min_y = (int) NUM_ROOMS;
        byte[] new_layout = new byte[CHROM_LENGTH];

        for( int i = 0; i < CHROM_LENGTH; i++ )
        {
            visited[i] = 0;
            new_layout[i] = Wall;
        }
        
        Extract_Dungeon( layout, visited, new_layout, Entrance_idx, ref min_x, ref max_x, ref min_y, ref max_y );

        w = max_x - min_x + 1;
        h = max_y - min_y + 1;

        byte[] final_layout = new byte[w*h];

        for( int y = min_y; y <= max_y; y++ )
            for( int x = min_x; x <= max_x; x++ )
                final_layout[(y-min_y)*w+(x-min_x)] = new_layout[y*NUM_ROOMS+x];
        
        return final_layout;
    }

    public void print_layout( byte[] layout, int w, int h )
    {
        String s = "\n";

        if( w <= 0 )
            w = (int) NUM_ROOMS;
        
        if( h <= 0 )
            h = (int) NUM_ROOMS;
        
        Console.WriteLine(w + " x " + h );
        for( int y = 0; y < h; y++ )
        {
            for( int x = 0; x < w; x++ )
            {   
                switch( layout[y*w+x] )
                {
                    case Room : s  = s + "R"; break;
                    case Wall : s  = s + "-"; break;
                    case Entrance : s  = s + "I"; break;
                    case Exit : s  = s + "O"; break;
                    default : s  = s + "?"; break;
                }

                 s = s + " ";
            }

            Console.WriteLine(s);
            s = "";
        }

        Console.WriteLine("\n");
    }

    public static byte[] get_new_layout( uint pop_size, uint num_rooms, uint max_gens, double mutation_rate, uint selection_pressure, out int w, out int h )
    {
        DungeonLayoutGenerator DLG = new DungeonLayoutGenerator( pop_size, num_rooms, max_gens, mutation_rate, selection_pressure );

        byte[] layout = DLG.Find_Entrance_And_Exit( DLG.generate_layout(), out w, out h );

        DLG.print_layout( layout, w, h );

        return layout;
    }
}
