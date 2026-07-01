using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace PlantsEvolution
{
public class PhotosyntesisProcess
{
    public const int MaxPhotosyntesisKCount = 150; 

    public const float PhotosyntesisK = 1f;
public void Process(List<TreeElement> treeElements, int[,] CellIds, float k = PhotosyntesisK)
{
    foreach (var id in CellIds)
    {
        if (id != -1)
        {
            TreeElement tree = treeElements.Find(t => t.TreeID == id);

            int CellsCount = tree.GroupPositions.Count;
            float countK =  Mathf.Clamp01(CellsCount/MaxPhotosyntesisKCount)/3f;

            if (tree != null)
            {
                float efficiency = 1 * k * (1.05f - countK);
                
                tree.PointElement.AddPoints(efficiency);
            }
        }
    }
}
}
}
