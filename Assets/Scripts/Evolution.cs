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
        if (treeElements == null || treeElements.Count == 0 || TreesCount <= 0 || EvolutionRate < 0 || MutationRate < 0)
        {
            Debug.LogError("[Evolution] Invalid input params.");
            return null;
        }

        int targetTreesCount = TreesCount * TreesCount;
        int safeExponent = Mathf.Max(0, EvolutionRate - 1);
        int treesToEvolve = Mathf.Max(1, Mathf.CeilToInt(treeElements.Count / Mathf.Pow(2, safeExponent)));

        List<TreeElement> parents = GetBestTrees(treeElements, treesToEvolve);
        if (parents.Count == 0)
        {
            Debug.LogError("[Evolution] No valid parents.");
            return null;
        }

        List<TreeElement> newGeneration = new List<TreeElement>(targetTreesCount);

        // 1. Сначала добавляем лучших родителей БЕЗ мутаций
        foreach (TreeElement parent in parents)
        {
            if (newGeneration.Count >= targetTreesCount) break;
            if (parent?.GeneticElement == null) continue;

            TreeElement eliteTree = new TreeElement(
                newGeneration.Count,
                parent.GeneticElement.Clone(), // Клон без мутации
                new PointElement(),
                parent.Color
            );
            newGeneration.Add(eliteTree);
        }

        // 2. Заполняем остальное пространство мутированными детьми
        int childrenPerParent = Mathf.Max(1, EvolutionRate);

        while (newGeneration.Count < targetTreesCount)
        {
            bool createdAtLeastOne = false;

            foreach (TreeElement parent in parents)
            {
                if (newGeneration.Count >= targetTreesCount) break;
                if (parent?.GeneticElement == null) continue;

                for (int i = 0; i < childrenPerParent && newGeneration.Count < targetTreesCount; i++)
                {
                    GeneticElement childGenes = parent.GeneticElement.Clone();
                    if (childGenes == null) continue;

                    if (EvolutionRate > 0)
                        childGenes.Mutate(MutationRate);

                    // ID строго по индексу в новой генерации (без дыр)
                    int childId = newGeneration.Count;

                    TreeElement child = new TreeElement(
                        childId,
                        childGenes,
                        new PointElement(),
                        parent.Color
                    );

                    newGeneration.Add(child);
                    createdAtLeastOne = true;
                }
            }

            if (!createdAtLeastOne)
            {
                Debug.LogError("[Evolution] Failed to produce children.");
                return null;
            }
        }

        return newGeneration;
    }

    private List<TreeElement> GetBestTrees(List<TreeElement> trees, int n)
    {
        return trees
            .Where(t => t?.GeneticElement != null)
            .OrderByDescending(t => t.PointElement?.Score ?? int.MinValue)
            .Take(Mathf.Max(1, n))
            .ToList();
    }
}