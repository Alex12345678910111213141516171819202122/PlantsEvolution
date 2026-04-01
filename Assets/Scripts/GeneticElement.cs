using UnityEngine;
namespace PlantsEvolution
{
public class GeneticElement
{
    const byte size = 6;
    const byte length = 50;
    const byte maxCells = 2;
    const byte mutationRate = 1;
    public readonly byte[][] GeneticData;


    public GeneticElement(byte length = length, byte maxCells = maxCells, byte size = size)
    {
        GeneticData = new byte[length][];

        for (int i = 1; i < length; i++)
        {
            GeneticData[i] = new byte[size + 1];
            byte k = (byte)UnityEngine.Random.Range(0, maxCells + 1);
            GeneticData[i][size] = k;
            for(int j = 0; j < k; j++)
            {
                int cellIndex = UnityEngine.Random.Range(0, size);
                if(GeneticData[i][cellIndex] == 0)
                {
                    GeneticData[i][cellIndex] = (byte)UnityEngine.Random.Range(1, length);
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
                    GeneticData[geneIndex][cellIndex] = (byte)Random.Range(1, GeneticData.Length);
            }

        }

    public GeneticElement Clone()
    {
        GeneticElement clone = new GeneticElement();

        for (int i = 0; i < GeneticData.Length; i++)
        {
            byte[] gene = GeneticData[i];
            clone.GeneticData[i] = gene == null ? null : (byte[])gene.Clone();
        }

        return clone;
    }
}
}