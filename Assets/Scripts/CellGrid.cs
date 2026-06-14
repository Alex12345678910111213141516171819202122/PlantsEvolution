using System.Collections.Generic;
using UnityEngine;
using PlantsEvolution;
using Unity.VisualScripting;
using System;
namespace PlantsEvolution
{
public class CellGrid
{
    public Vector3Int Position  {get; private set; } = new Vector3Int(0, 0, 0);
    private float _cellSize = 2.0f;
    public float cellSize
    {
        get => _cellSize;
        private set
        {
            if (value <= 0)
            {
                Debug.LogWarning($"[CellGrid] Invalid cellSize: {value}. Using 1.0f instead.");
                _cellSize = 1.0f;
            }
            else
            {
                _cellSize = value;
            }
        }
    }
    int xSize = 100;
    int zSize = 100;
    int ySize = 100;
    public List<Vector3> GrowthPositions { get; private set; }
    private HashSet<Vector3> prohibitesPositions;
    public Dictionary<Vector3, CellElement> Cells  { get; private set; }
    private int[,] topY { get; set; }
    public int[,] topCellId { get; set; }
    private int[,] bottomY { get; set; }
    public int[,] bottomCellId { get; set; }
    
    private int[,] minX { get; set; }
    public int[,] minXCellId { get; set; }
    private int[,] maxX { get; set; }
    public int[,] maxXCellId { get; set; }
    
    private int[,] minZ { get; set; }
    public int[,] minZCellId { get; set; }
    private int[,] maxZ { get; set; }
    public int[,] maxZCellId { get; set; }

    private void InitializeStorage()
    {
        Cells = new Dictionary<Vector3, CellElement>();
        
        // Top/Bottom boundaries (indexed by x, z)
        topY = new int[xSize, zSize];
        topCellId = new int[xSize, zSize];
        bottomY = new int[xSize, zSize];
        bottomCellId = new int[xSize, zSize];
        
        // X boundaries (indexed by y, z)
        minX = new int[ySize, zSize];
        minXCellId = new int[ySize, zSize];
        maxX = new int[ySize, zSize];
        maxXCellId = new int[ySize, zSize];
        
        // Z boundaries (indexed by x, y)
        minZ = new int[xSize, ySize];
        minZCellId = new int[xSize, ySize];
        maxZ = new int[xSize, ySize];
        maxZCellId = new int[xSize, ySize];
        
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                topY[x, z] = int.MinValue;
                topCellId[x, z] = -1;
                bottomY[x, z] = int.MaxValue;
                bottomCellId[x, z] = -1;
            }
        }
        
        for (int y = 0; y < ySize; y++)
        {
            for (int z = 0; z < zSize; z++)
            {
                minX[y, z] = int.MaxValue;
                minXCellId[y, z] = -1;
                maxX[y, z] = int.MinValue;
                maxXCellId[y, z] = -1;
            }
        }
        
        for (int x = 0; x < xSize; x++)
        {
            for (int y = 0; y < ySize; y++)
            {
                minZ[x, y] = int.MaxValue;
                minZCellId[x, y] = -1;
                maxZ[x, y] = int.MinValue;
                maxZCellId[x, y] = -1;
            }
        }
    }

    public CellGrid()
    {
        InitializeStorage();
    }

    public CellGrid(Vector3Int position, float cellSize, int xmargin, int zmargin, float height, int TreesCount)
    {
        Position = position;

        if (cellSize <= 0f)
        {
            Debug.LogWarning($"[CellGrid] Invalid cellSize: {cellSize}. Using 1.0f instead.");
            cellSize = 1.0f;
        }

        if (xmargin < 1)
        {
            Debug.LogWarning($"[CellGrid] Invalid xmargin: {xmargin}. Minimum is 1. Using 1 instead.");
            xmargin = 1;
        }

        if (zmargin < 1)
        {
            Debug.LogWarning($"[CellGrid] Invalid zmargin: {zmargin}. Minimum is 1. Using 1 instead.");
            zmargin = 1;
        }

        if (height <= 0f)
        {
            Debug.LogWarning($"[CellGrid] Invalid height: {height}. Using 1.0f instead.");
            height = 1.0f;
        }

        if (TreesCount < 1)
        {
            Debug.LogWarning($"[CellGrid] Invalid TreesCount: {TreesCount}. Minimum is 1. Using 1 instead.");
            TreesCount = 1;
        }

        this.cellSize = cellSize;

        xSize = Mathf.CeilToInt((TreesCount + (TreesCount + 1) * xmargin) * cellSize);
        zSize = Mathf.CeilToInt((TreesCount + (TreesCount + 1) * zmargin) * cellSize);
        ySize = Mathf.Max(1, Mathf.CeilToInt(height * cellSize));

        InitializeStorage();

        //prohibitesPositions = new HashSet<Vector3>(GetTwoCirclesPositions(30, 20));
        this.GrowthPositions = GetUniformCellPositions(xmargin, zmargin, TreesCount);
    }

    public List<Vector3> GetUniformCellPositions(int xmargin, int zmargin, int cellsCount, int y = 0)
    {
        if (xmargin < 1)
        {
            Debug.LogWarning($"[CellGrid] Invalid xmargin: {xmargin}. Minimum is 1. Using 1 instead.");
            xmargin = 1;
        }

        if (zmargin < 1)
        {
            Debug.LogWarning($"[CellGrid] Invalid zmargin: {zmargin}. Minimum is 1. Using 1 instead.");
            zmargin = 1;
        }

        if (cellsCount < 1)
        {
            Debug.LogWarning($"[CellGrid] Invalid cellsCount: {cellsCount}. Minimum is 1. Using 1 instead.");
            cellsCount = 1;
        }

        List<Vector3> positions = new List<Vector3>(cellsCount * cellsCount);

        float size = (float)cellSize;
        float stepX = (1f + xmargin) * size;
        float stepZ = (1f + zmargin) * size;

        float startX = Position.x + (xmargin + 0.5f) * size;
        float startZ = Position.z + (zmargin + 0.5f) * size;

        for (int x = 0; x < cellsCount; x++)
        {
            for (int z = 0; z < cellsCount; z++)
            {
                Vector3 pos = new Vector3(startX + x * stepX, y, startZ + z * stepZ);
                if (prohibitesPositions == null || !prohibitesPositions.Contains(pos))
                {
                    positions.Add(pos);
                }
            }
        }

        return positions;
    }

    
    private void GetGridIndices(Vector3 position, out int x, out int z)
    {
        x = (int)((position.x - Position.x) / cellSize);
        z = (int)((position.z - Position.z) / cellSize);
    }

    private void UpdateYBoundaries(int x, int z, int y, int cellID)
    {
        if (y > topY[x, z])
        {
            topY[x, z] = y;
            topCellId[x, z] = cellID;
        }
        
        if (y < bottomY[x, z])
        {
            bottomY[x, z] = y;
            bottomCellId[x, z] = cellID;
        }
    }


    
    private void UpdateXBoundaries(int x, int y, int z, int cellID)
    {
        if (y < 0 || y >= ySize || z < 0 || z >= zSize)
            return;
            
        if (x < minX[y, z])
        {
            minX[y, z] = x;
            minXCellId[y, z] = cellID;
        }
        
        if (x > maxX[y, z])
        {
            maxX[y, z] = x;
            maxXCellId[y, z] = cellID;
        }
    }
    
    private void UpdateZBoundaries(int x, int y, int z, int cellID)
    {
        if (x < 0 || x >= xSize || y < 0 || y >= ySize)
            return;
            
        if (z < minZ[x, y])
        {
            minZ[x, y] = z;
            minZCellId[x, y] = cellID;
        }
        
        if (z > maxZ[x, y])
        {
            maxZ[x, y] = z;
            maxZCellId[x, y] = cellID;
        }
    }
    
    private void UpdateBoundaryCells(Vector3 position, int cellID)
    {
        GetGridIndices(position, out int x, out int z);
        int y = (int)position.y;
        
        UpdateYBoundaries(x, z, y, cellID);
        UpdateXBoundaries(x, y, z, cellID);
        UpdateZBoundaries(x, y, z, cellID);
    }

    public Vector3 OutOfBounds(Vector3 position)
    {
        float wrappedX = Position.x + Mathf.Repeat(position.x - Position.x, xSize);
        float wrappedZ = Position.z + Mathf.Repeat(position.z - Position.z, zSize);

        return new Vector3(wrappedX, position.y, wrappedZ);
    }

    public bool AddCell( byte gen, int cellID, ref Vector3 position)
    {
        // position = OutOfBounds(position);

        if (position.y < 0 || position.y >= ySize || position.x < Position.x || position.x >= Position.x + xSize * cellSize || position.z < Position.z || position.z >= Position.z + zSize * cellSize)
        {
            return false;
        }
        if(prohibitesPositions != null && prohibitesPositions.Contains(position))
        {
            return false;
        }
        if (!Cells.ContainsKey(position))
        {
            CellElement cellElement = new CellElement(cellID, gen);
            Cells[position] = cellElement;
            UpdateBoundaryCells(position, cellID);
            return true;
        }
        return false;
    }

    public void RemoveCell(Vector3 position)
    {
        if (!Cells.Remove(position))
        {
            Debug.LogWarning($"[CellGrid] Attempted to remove non-existent cell at position: {position}");
            return;
        }

        GetGridIndices(position, out int x, out int z);
        if ((int)position.y == topY[x, z])
        {
            topY[x, z] = int.MinValue;
            topCellId[x, z] = -1;
        }
    }

    public HashSet<Vector3> GetTopCellPositions()
    {
        var result = new HashSet<Vector3>();
        foreach (var kvp in Cells)
        {
            GetGridIndices(kvp.Key, out int x, out int z);
            if (x >= 0 && x < xSize && z >= 0 && z < zSize && (int)kvp.Key.y == topY[x, z])
                result.Add(kvp.Key);
        }
        return result;
    }

    public List<Vector3> GetCrossPositions(int crossSize = 85, int crossWidth = 10)
    {
        HashSet<Vector3> uniquePositions = new HashSet<Vector3>();
        
        // Центр поля в мировых координатах
        float centerWorldX = Position.x + (xSize * cellSize) / 2.0f;
        float centerWorldZ = Position.z + (zSize * cellSize) / 2.0f;
        
        float halfSize = crossSize / 2.0f;
        float halfWidth = crossWidth / 2.0f;
        
        // Вертикальная полоса креста (вдоль оси Z)
        int stepsWidth = Mathf.CeilToInt(crossWidth / cellSize);
        int stepsSize = Mathf.CeilToInt(crossSize / cellSize);
        
        for (int ix = -stepsWidth / 2; ix <= stepsWidth / 2; ix++)
        {
            for (int iz = -stepsSize / 2; iz <= stepsSize / 2; iz++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float x = centerWorldX + ix * cellSize;
                    float z = centerWorldZ + iz * cellSize;
                    uniquePositions.Add(new Vector3(x, y, z));
                }
            }
        }
        
        // Горизонтальная полоса креста (вдоль оси X)
        for (int iz = -stepsWidth / 2; iz <= stepsWidth / 2; iz++)
        {
            for (int ix = -stepsSize / 2; ix <= stepsSize / 2; ix++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float x = centerWorldX + ix * cellSize;
                    float z = centerWorldZ + iz * cellSize;
                    uniquePositions.Add(new Vector3(x, y, z));
                }
            }
        }
        
        // Дополнительная полоска от края +Z в сторону +X
        int halfSteps = stepsSize / 2;
        int maxZEdge = stepsSize / 2;
        
        for (int iz = maxZEdge - stepsWidth / 2; iz <= maxZEdge + stepsWidth / 2; iz++)
        {
            for (int ix = 0; ix <= halfSteps; ix++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float x = centerWorldX + ix * cellSize;
                    float z = centerWorldZ + iz * cellSize;
                    uniquePositions.Add(new Vector3(x, y, z));
                }
            }
        }
        
        // Дополнительная полоска от края -Z в сторону -X
        int minZEdge = -stepsSize / 2;
        
        for (int iz = minZEdge - stepsWidth / 2; iz <= minZEdge + stepsWidth / 2; iz++)
        {
            for (int ix = -halfSteps; ix <= 0; ix++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float x = centerWorldX + ix * cellSize;
                    float z = centerWorldZ + iz * cellSize;
                    uniquePositions.Add(new Vector3(x, y, z));
                }
            }
        }
        
        // Дополнительная полоска от края +X в сторону -Z
        int maxXEdge = stepsSize / 2;
        
        for (int ix = maxXEdge - stepsWidth / 2; ix <= maxXEdge + stepsWidth / 2; ix++)
        {
            for (int iz = -halfSteps; iz <= 0; iz++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float x = centerWorldX + ix * cellSize;
                    float z = centerWorldZ + iz * cellSize;
                    uniquePositions.Add(new Vector3(x, y, z));
                }
            }
        }
        
        // Дополнительная полоска от края -X в сторону +Z
        int minXEdge = -stepsSize / 2;
        
        for (int ix = minXEdge - stepsWidth / 2; ix <= minXEdge + stepsWidth / 2; ix++)
        {
            for (int iz = 0; iz <= halfSteps; iz++)
            {
                for (int y = 0; y < ySize; y++)
                {
                    float x = centerWorldX + ix * cellSize;
                    float z = centerWorldZ + iz * cellSize;
                    uniquePositions.Add(new Vector3(x, y, z));
                }
            }
        }
        
        return new List<Vector3>(uniquePositions);
    }

    public List<Vector3> GetTwoCirclesPositions(float offsetX = 30, float radius = 15)
    {
        HashSet<Vector3> uniquePositions = new HashSet<Vector3>();
        
        // Центр поля в мировых координатах
        float centerWorldX = Position.x + (xSize * cellSize) / 2.0f;
        float centerWorldZ = Position.z + (zSize * cellSize) / 2.0f;
        
        // Расстояние между центрами = 1 радиус (круги пересекаются)
        float circleSpacing = radius * 1.4f;
        
        // Центры двух кругов (смещены на -offsetX по оси X, симметричны по оси Z)
        float circle1X = centerWorldX - offsetX;
        float circle1Z = centerWorldZ + circleSpacing / 2.0f;
        
        float circle2X = centerWorldX - offsetX;
        float circle2Z = centerWorldZ - circleSpacing / 2.0f;
        
        // Количество шагов для покрытия области круга
        int radiusSteps = Mathf.CeilToInt(radius / cellSize);
        
        // Первый круг
        for (int ix = -radiusSteps; ix <= radiusSteps; ix++)
        {
            for (int iz = -radiusSteps; iz <= radiusSteps; iz++)
            {
                float x = circle1X + ix * cellSize;
                float z = circle1Z + iz * cellSize;
                
                // Проверка, находится ли точка внутри круга
                float dx = x - circle1X;
                float dz = z - circle1Z;
                if (dx * dx + dz * dz <= radius * radius)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        uniquePositions.Add(new Vector3(x, y, z));
                    }
                }
            }
        }
        
        // Второй круг
        for (int ix = -radiusSteps; ix <= radiusSteps; ix++)
        {
            for (int iz = -radiusSteps; iz <= radiusSteps; iz++)
            {
                float x = circle2X + ix * cellSize;
                float z = circle2Z + iz * cellSize;
                
                // Проверка, находится ли точка внутри круга
                float dx = x - circle2X;
                float dz = z - circle2Z;
                if (dx * dx + dz * dz <= radius * radius)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        uniquePositions.Add(new Vector3(x, y, z));
                    }
                }
            }
        }
        
        // Овал от центра пересечения кругов в сторону +X
        // Центр пересечения находится на (centerWorldX - offsetX, centerWorldZ)
        float ovalLength = 6.0f * radius; // 3 диаметра
        float ovalWidth = 2.0f * radius;  // 1 диаметр
        
        // Центр овала смещен на половину длины от точки пересечения
        float ovalCenterX = (centerWorldX - offsetX) + ovalLength / 2.0f;
        float ovalCenterZ = centerWorldZ;
        
        float ovalRadiusX = ovalLength / 2.0f;  // полуось по X
        float ovalRadiusZ = ovalWidth / 2.0f;   // полуось по Z
        
        int ovalStepsX = Mathf.CeilToInt(ovalRadiusX / cellSize);
        int ovalStepsZ = Mathf.CeilToInt(ovalRadiusZ / cellSize);
        
        for (int ix = -ovalStepsX; ix <= ovalStepsX; ix++)
        {
            for (int iz = -ovalStepsZ; iz <= ovalStepsZ; iz++)
            {
                float x = ovalCenterX + ix * cellSize;
                float z = ovalCenterZ + iz * cellSize;
                
                // Проверка эллипса: (dx/a)^2 + (dz/b)^2 <= 1
                float dx = x - ovalCenterX;
                float dz = z - ovalCenterZ;
                float normalizedDist = (dx * dx) / (ovalRadiusX * ovalRadiusX) + 
                                       (dz * dz) / (ovalRadiusZ * ovalRadiusZ);
                
                if (normalizedDist <= 1.0f)
                {
                    for (int y = 0; y < ySize; y++)
                    {
                        uniquePositions.Add(new Vector3(x, y, z));
                    }
                }
            }
        }
        
        return new List<Vector3>(uniquePositions);
    }
}
}

