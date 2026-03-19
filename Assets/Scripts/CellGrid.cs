using System.Collections.Generic;
using UnityEngine;
using PlantsEvolution;
using Unity.VisualScripting;
namespace PlantsEvolution
{
public class CellGrid
{
    public Vector3Int Position  {get; private set; } = new Vector3Int(0, 0, 0);
    private double cellSize = 1.0;
    int xSize = 100;
    int zSize = 100;
    int ySize = 100;
    public Dictionary<Vector3, CellElement> Cells  { get; private set; }
    private int[,] topY { get; set; }
    public int[,] topCellId { get; set; }
    public CellGrid()
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

    public bool AddCell( int gen, int cellID, ref Vector3 position)
    {
        position = OutOfBounds(position);

        if (position.y < 0 || position.y >= ySize)
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
}
}
