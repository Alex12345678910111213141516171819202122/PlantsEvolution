using UnityEngine;
using System.Collections.Generic;
namespace PlantsEvolution
{
public class PlantingProcess
{
    private readonly GrowthProcess growthProcess;

    private readonly CellGrid cellGrid;

    public PlantingProcess(GrowthProcess growthProcess, CellGrid cellGrid)
    {
        this.growthProcess = growthProcess;
        this.cellGrid = cellGrid;
    }

    public List<TreeElement> PlantInitialTrees(ITreeBuilder treeBuilder, int initialTreesCount)
        {
            List<TreeElement> plantedTrees = new List<TreeElement>();
            for (int i = 0; i < initialTreesCount; i++)
            {
                if (i < cellGrid.GrowthPositions.Count)
                {
                    TreeElement tree = PlantTree(treeBuilder, i, cellGrid.GrowthPositions[i]);
                    plantedTrees.Add(tree);
                }
                else
                {
                    Debug.LogWarning($"[PlantingProcess] Not enough growth positions to plant tree {i}. Skipping.");
                }
            }
            return plantedTrees;
        }
     public TreeElement PlantTree(ITreeBuilder treeBuilder, int treeID, Vector3 position)
        {
            treeBuilder.SetTreeID(treeID);
            treeBuilder.BuildGeneticElement();
            treeBuilder.BuildPointElement();
            TreeElement treeElement = treeBuilder.GetTreeElement();
            treeBuilder.Clear();
            PlantTree(treeElement, position);
            return treeElement;
        }
    public void PlantTree(TreeElement tree, Vector3 position)
    {
        growthProcess.AddCell(cellGrid, tree, 1, position);
    }

    public void PlantNewGeneration(List<TreeElement> trees)
    {
        for(int i = 0; i < trees.Count; i++)
        {
            if (i >= cellGrid.GrowthPositions.Count)
            {
                Debug.LogWarning($"[PlantingProcess] Not enough growth positions to plant tree {i}. Skipping.");
                break;
            }
            List<Vector3> RPositions = new List<Vector3>(cellGrid.GrowthPositions);
            RPositions.Shuffle();
            growthProcess.AddCell(cellGrid, trees[i], 1, RPositions[i]);
        }
    }

}
}
