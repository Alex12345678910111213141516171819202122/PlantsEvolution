using System.Collections.Generic;
using UnityEngine;
namespace PlantsEvolution
{
public class PhotosyntesisProcess
{
    public const int MaxPhotosyntesisKCount =50; 

    public const float PhotosyntesisK = 1f;
public void Process(List<TreeElement> treeElements, int[,] topCellIds)
{
    foreach (var id in topCellIds)
    {
        if (id != -1)
        {
            TreeElement tree = treeElements.Find(t => t.TreeID == id);
            if (tree != null)
            {
                // Используем количество клеток дерева для уменьшения эффективности
                int cellCount = tree.GroupPositions.Count; // или другой показатель размера
                float efficiency = 1 - (EaseInPower(cellCount, MaxPhotosyntesisKCount) * PhotosyntesisK);
                float pointsToAdd = Mathf.Max(0.1f, efficiency); // минимум 0.1 очка
                
                tree.PointElement.AddPoints(pointsToAdd);
            }
        }
    }
}

float EaseInPower(float x, float maxX, float power = 3f)
{
    float t = Mathf.Clamp01(x / maxX);
    return Mathf.Pow(t, power);
    // power = 2: мягче
    // power = 3-4: оптимально
    // power = 5+: очень резко
}
}
}
