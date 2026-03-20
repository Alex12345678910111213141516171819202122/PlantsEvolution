using UnityEngine;
namespace PlantsEvolution
{
public class GeneticElement
{
    const int size = 6;
    const int length = 50;
    const int maxCells = 2;
    const int mutationRate = 1;
    public readonly int[][] GeneticData;


    public GeneticElement(int length = length, int maxCells = maxCells, int size = size)
    {
        GeneticData = new int[length][];

        for (int i = 1; i < length; i++)
        {
            GeneticData[i] = new int[size + 1];
            int k = UnityEngine.Random.Range(0, maxCells + 1);
            GeneticData[i][size] = k;
            for(int j = 0; j < k; j++)
            {
                int cellIndex = UnityEngine.Random.Range(0, size);
                if(GeneticData[i][cellIndex] == 0)
                {
                    GeneticData[i][cellIndex] = UnityEngine.Random.Range(1, length);
                }
                else
                {
                    j--;
                }
            }
        }
    }

    public void Mutate(int mutationRate = mutationRate)
        {
            if(mutationRate < 0 || mutationRate > GeneticData.Length)
            {
                Debug.LogWarning($"[Genetics] Invalid mutation rate: {mutationRate}. Mutation rate should be between 0 and {GeneticData.Length}.");
                return;
            }
            int geneIndex = Random.Range(1, GeneticData.Length);
            for (int i = 0; i < mutationRate; i++)
            {
                if(GeneticData[geneIndex][size] == maxCells)
                {
                    int occupiedCells = GeneticData[geneIndex][size];
                    int targetOccupiedCell = Random.Range(0, occupiedCells);
                    int currentOccupiedCell = 0;

                    for (int j = 0; j < size; j++)
                    {
                        if (GeneticData[geneIndex][j] != 0)
                        {
                            if (currentOccupiedCell == targetOccupiedCell)
                            {
                                GeneticData[geneIndex][j] = 0;
                                GeneticData[geneIndex][size]--;
                                break;
                            }
                            currentOccupiedCell++;
                        }
                    }
                }
                    int cellIndex = Random.Range(0, size);
                    if(GeneticData[geneIndex][cellIndex] == 0)
                    {
                        GeneticData[geneIndex][size]++;
                    }
                    GeneticData[geneIndex][cellIndex] = Random.Range(1, GeneticData.Length);
            }

        }

    public GeneticElement Clone()
    {
        GeneticElement clone = new GeneticElement();

        for (int i = 0; i < GeneticData.Length; i++)
        {
            int[] gene = GeneticData[i];
            clone.GeneticData[i] = gene == null ? null : (int[])gene.Clone();
        }

        return clone;
    }
}
}