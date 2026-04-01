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
    public Dictionary<Vector3, CellElement> Cells  { get; private set; }
    private int[,] topY { get; set; }
    public int[,] topCellId { get; set; }

    private void InitializeStorage()
    {
        Cells = new Dictionary<Vector3, CellElement>();
        topY = new int[xSize, zSize];
        topCellId = new int[xSize, zSize];
        for (int x = 0; x < xSize; x++)
        {
            for (int z = 0; z < zSize; z++)
            {
                topY[x, z] = int.MinValue;
                topCellId[x, z] = -1;
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
                positions.Add(new Vector3(startX + x * stepX, y, startZ + z * stepZ));
            }
        }

        return positions;
    }

    
    private void GetGridIndices(Vector3 position, out int x, out int z)
    {
        x = (int)((position.x - Position.x) / cellSize);
        z = (int)((position.z - Position.z) / cellSize);
    }

    private void UpdateTopCell(Vector3 position, int cellID)
    {
        GetGridIndices(position, out int x, out int z);
        int y = (int)position.y;

        if (y > topY[x, z])
        {
            topY[x, z] = y;
            topCellId[x, z] = cellID;
        }
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
        if (!Cells.ContainsKey(position))
        {
            CellElement cellElement = new CellElement(cellID, gen);
            Cells[position] = cellElement;
            UpdateTopCell(position, cellID);
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
}
}

