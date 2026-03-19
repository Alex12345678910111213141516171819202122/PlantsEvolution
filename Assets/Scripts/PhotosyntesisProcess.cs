using System.Collections.Generic;
using UnityEngine;
namespace PlantsEvolution
{
public class PhotosyntesisProcess
{
    public void Process(List<TreeElement> treeElements, int[,] topCellIds)
        {
            foreach (var id in topCellIds)
            {
                if (id != -1)
                {
                    TreeElement tree = treeElements.Find(t => t.TreeID == id);
                    if (tree != null)
                    {
                        tree.PointElement.Add();
                    }
                    else
                    {
                        Debug.LogWarning($"[Photosynthesis] No tree found with ID {id} in the list of tree elements.");
                    }
                }
            }
        }
}
}
