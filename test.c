#include <stdio.h>
#include <stdlib.h>
#include <string.h>
#include <time.h>

typedef unsigned int uint;

// Définir les paramètres de l'algorithme génétique

#define Wall 0
#define Room 1
#define Entrance 2
#define Exit 3

/*
int POP_SIZE = 100;             // Taille de la population
int CHROM_LENGTH = 100;         // Longueur de chaque chromosome ( string binaire )
int NUM_ROOMS = 10;             // Nombre de pièces dans le donjon
int MAX_genS = 100;      // Nombre max de générations
float MUTATION_RATE = 0.01;     // Probablité d'inverser un bit lors de la mutation 
int SELECTION_PRESSURE = 2;     // Pression pendant la séléction du meilleur individu
*/

// FITNESS FUNCTION
int fitness( int* chrom, const uint CHROM_LENGTH, const uint NUM_ROOMS )
{
    int** layout = malloc( sizeof( int* ) * NUM_ROOMS );
    for( int i = 0; i < NUM_ROOMS; i++ )
        layout[i] = malloc( sizeof(int) * NUM_ROOMS );
    int num_exits = 0;
    int num_connections = 0;

    for( int x = 0; x < NUM_ROOMS; x++ ) 
        for( int y = 0; y < NUM_ROOMS; y++ ) 
             layout[x][y] = chrom[x*NUM_ROOMS+y];
    
    // Calcule du fitness
    for( int x = 0; x < NUM_ROOMS; x++ ) 
        for( int y = 0; y < NUM_ROOMS; y++ ) 
            if( layout[x][y] ) 
            {
                // Si une pièce est au bord de la "map" alors c'est une sortie
                if( x == 0 || x == NUM_ROOMS-1 || y == 0 || y == NUM_ROOMS-1 ) 
                    num_exits++;
                
                // Si deux pièces sont voisines, alors il y a une connection entre elles
                // On ne teste pas les pièces à droite et en haut pour ne prendre en compte qu'une seule fois chaque connection
                if( x > 0 && layout[x-1][y] == 1 && y > 0 && layout[x][y-1] == 1) 
                    num_connections++;
            }
    
    for( int i = 0; i < NUM_ROOMS; i++ )
        free(layout[i]);
    free(layout);

    return num_exits + num_connections;
}

// OPERATIONS GENETIQUES
void mutate( int* chrom, const uint CHROM_LENGTH, const float MUTATION_RATE ) // Inversion de tous les bits
{
    for( int i = 0; i < CHROM_LENGTH; i++ ) 
        if( ((float)rand()/(float)RAND_MAX) < MUTATION_RATE ) 
            chrom[i] = (chrom[i])?(0):(1);
}

void crossover( int* parent1, int* parent2, int* child, const uint CHROM_LENGTH )
{
    for( int i = 0; i < CHROM_LENGTH; i++ ) 
        if( ((float)rand()/(float)RAND_MAX) < 0.5 ) 
            child[i] = parent1[i];
        else 
            child[i] = parent2[i];
}

// METHODE DE SELECTION
void tournament_selection( int** pop, const uint POP_SIZE, int* fitness_vals, int* parent1, int* parent2, const uint SELECTION_PRESSURE )
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
                parent1 = pop[idx];
            }
        }

        if( i ) 
            parent2 = pop[idx];
        else 
            parent1 = pop[idx];
    }
}

int* generate_layout( const uint POP_SIZE, const uint CHROM_LENGTH, const uint NUM_ROOMS, 
                      const uint MAX_genS, const float MUTATION_RATE, const uint SELECTION_PRESSURE )
{
    srand(time(NULL));

   // Allocate and initialize the population
    int** pop = malloc( sizeof(int*) * POP_SIZE );
    for( int i = 0; i < POP_SIZE; i++ )
    {
        pop[i] = malloc( sizeof(int) * CHROM_LENGTH );

        for( int j = 0; j < CHROM_LENGTH; j++ ) 
            pop[i][j] = (rand() % 2)?(Room):(Wall);
    }
        
    // Allocate and initialize the fitness values
    int* fitness_vals = malloc( sizeof(int) * POP_SIZE );
    for( int i = 0; i < POP_SIZE; i++ ) 
        fitness_vals[i] = fitness( pop[i], CHROM_LENGTH, NUM_ROOMS );

    // Allocate the parents and the child
    int* parent1 = malloc( sizeof(int) * CHROM_LENGTH );
    int* parent2 = malloc( sizeof(int) * CHROM_LENGTH );
    int* child = malloc( sizeof(int) * CHROM_LENGTH );

    // Run the genetic algorithm
    for( int gen = 0; gen < MAX_genS; gen++ )
    {
        // Select parents using tournament selection
        tournament_selection( pop, POP_SIZE, fitness_vals, parent1, parent2, SELECTION_PRESSURE );
        
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
                pop[min_idx][i] = child[i];
            
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

    int* result;
    result = malloc( sizeof(int) * NUM_ROOMS * NUM_ROOMS );
    for( int i = 0; i < CHROM_LENGTH; i++ ) result[i] = pop[max_fit][i];

    free( child );
    free( parent2 );
    free( parent1 );
    free( fitness_vals );
    for( int i = 0; i < POP_SIZE; i++ )
        free(pop[i]);
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
    uint chromosomes_length = 100;   // Length of each chromosome 
    uint number_of_rooms = 10;      // Number of rooms in the dungeon
    uint nb_max_generations = 100;   // Maximum number of gens
    float mutation_rate = 0.01;              // Probability of a bit flipping during mutation
    uint selection_pressure = 2;     // Pressure on selecting the fittest individuals

    int* layout = generate_layout( population_size, number_of_rooms*number_of_rooms, number_of_rooms, 
                                    nb_max_generations, mutation_rate, selection_pressure );
    
    printf("\n");
    for( int i = 0; i < chromosomes_length; i++ )
    {
        if( !(i%number_of_rooms) )
            printf("\n");

        printf("%c", (layout[i])?('1'):('0'));
    }
    printf("\n");

    free(layout);
    return 0;
}
