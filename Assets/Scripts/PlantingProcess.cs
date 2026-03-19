using UnityEngine;
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

}
}
