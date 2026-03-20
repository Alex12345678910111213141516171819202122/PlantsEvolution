using System.Collections.Generic;
using System.Linq;
using PlantsEvolution;
using UnityEngine;

public class TreesEvolutionProcess
{
    int MutationRate;
    int EvolutionRate;

    public TreesEvolutionProcess(int mutationRate, int evolutionRate)
    {
        MutationRate = mutationRate;
        EvolutionRate = evolutionRate;
    }

    public List<TreeElement> Process(List<TreeElement> treeElements, int TreesCount)
    {
        if (treeElements == null)
        {
            Debug.LogError("[Evolution] treeElements is null. Evolution cannot proceed.");
            return null;
        }

        if (treeElements.Count <= 0)
        {
            Debug.LogError("[Evolution] treeElements is empty. Evolution cannot proceed.");
            return null;
        }

        if (TreesCount <= 0)
        {
            Debug.LogError($"[Evolution] Invalid TreesCount: {TreesCount}. Expected a positive cube edge size.");
            return null;
        }

        if (EvolutionRate < 0)
        {
            Debug.LogError($"[Evolution] Invalid EvolutionRate: {EvolutionRate}. Evolution cannot proceed.");
            return null;
        }

        int targetTreesCount = TreesCount * TreesCount;
        int treesToEvolve = Mathf.Max(1, Mathf.CeilToInt(treeElements.Count / Mathf.Pow(2, EvolutionRate - 1)));
        List<TreeElement> bestTrees = GetBestTrees(treeElements, treesToEvolve);
        List<TreeElement> evolvedTrees = new List<TreeElement>(bestTrees);

        foreach (TreeElement tree in bestTrees.ToList())
        {
            if (tree == null)
            {
                Debug.LogWarning("[Evolution] Null tree element found in best trees list. Skipping.");
                continue;
            }

            for (int i = 0; i < EvolutionRate; i++)
            {
                if (evolvedTrees.Count >= targetTreesCount)
                {
                    return evolvedTrees;
                }

                GeneticElement newGeneticElement = tree.GeneticElement?.Clone();
                if (newGeneticElement == null)
                {
                    Debug.LogWarning("[Evolution] Tree has null genetic element. Skipping mutation.");
                    continue;
                }

                newGeneticElement.Mutate(MutationRate);
                TreeElement mutatedTree = new TreeElement(evolvedTrees.Count, newGeneticElement, new PointElement(), tree.Color);
                evolvedTrees.Add(mutatedTree);
            }
        }

        return evolvedTrees;
    }

    private List<TreeElement> GetBestTrees(List<TreeElement> trees, int n)
    {
        return trees
            .OrderByDescending(tree => tree.PointElement.Points)
            .Take(n)
            .ToList();
    }
}