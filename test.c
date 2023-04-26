#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

typedef unsigned int uint;
typedef unsigned char byte;

// Définir les paramètres de l'algorithme génétique

#define Wall 0
#define Room 1
#define Entrance 2
#define Exit 3

/*
int POP_SIZE = 100;             // Taille de la population
int CHROM_LENGTH = 100;         // Longueur de chaque chromosome ( string binaire )
int NUM_ROOMS = 10;             // Nombre de pièces dans le donjon
int MAX_GENS = 100;      // Nombre max de générations
float MUTATION_RATE = 0.01;     // Probablité d'inverser un bit lors de la mutation 
int SELECTION_PRESSURE = 2;     // Pression pendant la séléction du meilleur individu
*/

// FITNESS FUNCTION
int fitness( byte* chrom, const uint CHROM_LENGTH, const uint NUM_ROOMS )
{
    static byte* layout = NULL;
    if( !layout ) 
        layout = malloc( sizeof( byte ) * CHROM_LENGTH );

    int num_exits = 0;
    int num_connections = 0;

    for( int x = 0; x < CHROM_LENGTH; x++ )  
        layout[x] = chrom[x];
    
    // Calcule du fitness
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
                            nb_neighbours += (layout[(x+i)*NUM_ROOMS+(y+j)]!=Wall)?(1):(0);  
            }
            else 
                empty_rooms++;

    return nb_neighbours/(empty_rooms*empty_rooms);
}

// OPERATIONS GENETIQUES
void mutate( byte* chrom, const uint CHROM_LENGTH, const float MUTATION_RATE ) // Inversion de tous les bits
{
    for( int i = 0; i < CHROM_LENGTH; i++ ) 
        if( ((float)rand()/(float)RAND_MAX) < MUTATION_RATE ) 
            chrom[i] = (chrom[i])?(0):(1);
}

void crossover( byte* parent1, byte* parent2, byte* child, const uint CHROM_LENGTH )
{
    for( int i = 0; i < CHROM_LENGTH; i++ ) 
        if( ((float)rand()/(float)RAND_MAX) < 0.5 ) 
            child[i] = parent1[i];
        else 
            child[i] = parent2[i];
}

// METHODE DE SELECTION
void tournament_selection( byte* pop, const uint POP_SIZE,const uint CHROM_LENGTH, int* fitness_vals, byte* parent1, byte* parent2, const uint SELECTION_PRESSURE )
{
    for( int i = 0; i < SELECTION_PRESSURE; i++ ) 
    {
        int idx = rand()%POP_SIZE;
        int best_fit = fitness_vals[idx];

        for( int j = 0; j < SELECTION_PRESSURE-1; j++ ) 
        {
            idx = rand()%POP_SIZE;

            if( fitness_vals[idx] > best_fit ) 
            {
                best_fit = fitness_vals[idx];
                parent1 = pop+idx*CHROM_LENGTH;
            }
        }

        if( i ) 
            parent2 = pop+idx*CHROM_LENGTH;
        else 
            parent1 = pop+idx*CHROM_LENGTH;
    }
}

byte* generate_layout( const uint POP_SIZE, const uint CHROM_LENGTH, const uint NUM_ROOMS, 
                      const uint MAX_GENS, const float MUTATION_RATE, const uint SELECTION_PRESSURE )
{
    srand(time(NULL));

   // Allocate and initialize the population
    byte* pop = malloc( sizeof( byte ) * POP_SIZE  * CHROM_LENGTH );

    for( int i = 0; i < POP_SIZE*CHROM_LENGTH; i++ ) 
        pop[i] = (rand() % 2)?(Room):(Wall);
        
    // Allocate and initialize the fitness values
    int* fitness_vals = malloc( sizeof(int) * POP_SIZE );
    for( int i = 0; i < POP_SIZE; i++ ) 
        fitness_vals[i] = fitness( pop+i*CHROM_LENGTH, CHROM_LENGTH, NUM_ROOMS );

    // Allocate the parents and the child
    byte* parent1 = malloc( sizeof( byte ) * CHROM_LENGTH );
    byte* parent2 = malloc( sizeof( byte ) * CHROM_LENGTH );
    byte* child = malloc( sizeof( byte ) * CHROM_LENGTH );

    // Run the genetic algorithm
    for( int gen = 0; gen < MAX_GENS; gen++ )
    {
        // Select parents using tournament selection
        tournament_selection( pop, POP_SIZE, CHROM_LENGTH, fitness_vals, parent1, parent2, SELECTION_PRESSURE );
        
        // Create new offspring through crossover
        crossover( parent1, parent2, child, CHROM_LENGTH );
        
        // Mutate the offspring
        mutate( child, CHROM_LENGTH, MUTATION_RATE );
        
        // Evaluate the fitness of the offspring
        int child_fitness = fitness(child, CHROM_LENGTH, NUM_ROOMS );
        
        // Replace the least fit individual with the offspring
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

    byte* result;
    result = malloc( sizeof(int) * CHROM_LENGTH );
    for( int i = 0; i < CHROM_LENGTH; i++ ) result[i] = pop[max_fit*CHROM_LENGTH+i];

    free( child );
    free( parent2 );
    free( parent1 );
    free( fitness_vals );
    free(pop);

    return result;
}

int main( int agrc, char** argv )
{
    if( 0 )
    {
        printf("\n Usage : \n"
               " - population size\n"
               " - dimension of the level\n"
               " - maximum number of generations\n"
               " - rate of mutation ( belongs to [0, 1] )"
               " - selection pressure\n");
        return 1;
    }

    uint population_size = 100;      // Size of the population
    uint number_of_rooms = 10;      // Number of rooms in the dungeon
    uint chromosomes_length = number_of_rooms*number_of_rooms;
    uint nb_max_generations = 100;   // Maximum number of gens
    float mutation_rate = 0.01;              // Probability of a bit flipping during mutation
    uint selection_pressure = 2;     // Pressure on selecting the fittest individuals

    byte* layout = generate_layout( population_size, chromosomes_length, number_of_rooms, 
                                    nb_max_generations, mutation_rate, selection_pressure );
    
    printf("\n");
    for( int i = 0; i < chromosomes_length; i++ )
    {
        if( !(i%number_of_rooms) )
            printf("\n");

        printf(" %c", (layout[i])?('1'):('0'));
    }
    printf("\n");

    free(layout);
    return 0;
}
