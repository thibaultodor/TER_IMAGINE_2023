#include <stdio.h>
#include <stdlib.h>
#include <time.h>

// Définir les paramètres de l'algorithme génétique

#define WALL 0
#define ROOM 1
#define ENTRANCE 2
#define EXIT 3

#define POP_SIZE 100  // Size of the population
#define CHROM_LENGTH 100  // Length of each chromosome (binary string)
#define NUM_ROOMS 10  // Number of rooms in the dungeon
#define MAX_genS 100  // Maximum number of gens
#define MUTATION_RATE 0.01  // Probability of a bit flipping during mutation
#define SELECTION_PRESSURE 2  // Pressure on selecting the fittest individuals

/*
int POP_SIZE = 100;             // Taille de la population
int CHROM_LENGTH = 100;         // Longueur de chaque chromosome ( string binaire )
int NUM_ROOMS = 10;             // Nombre de pièces dans le donjon
int MAX_genS = 100;      // Nombre max de générations
float MUTATION_RATE = 0.01;     // Probablité d'inverser un bit lors de la mutation 
int SELECTION_PRESSURE = 2;     // Pression pendant la séléction du meilleur individu
*/

// FITNESS FUNCTION
int fitness( int* chrom )
{
    int layout[NUM_ROOMS][NUM_ROOMS];
    int num_exits = 0;
    int num_connections = 0;

    for( int i = 0; i < CHROM_LENGTH; i++ ) 
        layout[i % NUM_ROOMS][i / NUM_ROOMS] = chrom[i];
    
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
    
    return num_exits + num_connections;
}

// OPERATIONS GENETIQUES
void mutate( int* chrom ) // Inversion de tous les bits
{
    for( int i = 0; i < CHROM_LENGTH; i++ ) 
        if( ((float)rand()/(float)RAND_MAX) < MUTATION_RATE ) 
            chrom[i] = (chrom[i])?(0):(1);
}

void crossover( int* parent1, int* parent2, int* child )
{
    for( int i = 0; i < CHROM_LENGTH; i++ ) 
        if( ((float)rand()/(float)RAND_MAX) < 0.5 ) 
            child[i] = parent1[i];
        else 
            child[i] = parent2[i];
}

// METHODE DE SELECTION
void tournament_selection( int** pop, int* fitness_vals, int* parent1, int* parent2 )
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

int main( int argc, char** argv )
{
    srand(time(NULL));

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
    
   // Initialize the population
    int pop[POP_SIZE][CHROM_LENGTH];
    for( int i = 0; i < POP_SIZE; i++ ) 
        for( int j = 0; j < CHROM_LENGTH; j++ ) 
            pop[i][j] = rand() % 2;
        

    // Initialize the fitness values
    int fitness_vals[POP_SIZE];
    for( int i = 0; i < POP_SIZE; i++ ) 
        fitness_vals[i] = fitness(pop[i]);

    int max_fit = fitness_vals[0];

    // Run the genetic algorithm
    for( int gen = 0; gen < MAX_genS; gen++ )
    {
        // Select parents using tournament selection
        int parent1[CHROM_LENGTH];
        int parent2[CHROM_LENGTH];
        tournament_selection( (int**) pop, fitness_vals, parent1, parent2);
        
        // Create new offspring through crossover
        int child[CHROM_LENGTH];
        crossover(parent1, parent2, child);
        
        // Mutate the offspring
        mutate(child);
        
        // Evaluate the fitness of the offspring
        int child_fitness = fitness(child);
        
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

    int max_idx = 0;
    for( int i = 1; i < POP_SIZE; i++ ) 
        if (fitness_vals[i] > max_fit) 
        {
            max_fit = fitness_vals[i];
            max_idx = i;
        }

    printf("\n");
    for( int i = 0; i < CHROM_LENGTH; i++ )
    {
        if( !(i%NUM_ROOMS) )
            printf("\n");

        printf("%c", (pop[max_fit][i])?('1'):('0'));
    }
    printf("\n");

    return 0;
}
