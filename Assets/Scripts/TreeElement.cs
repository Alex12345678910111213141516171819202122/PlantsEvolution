using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
namespace PlantsEvolution
{
public class TreeElement
{
    public readonly int TreeID;
    public readonly Color Color;
    public GeneticElement GeneticElement { get; private set;}

    public const int MaxPhotosyntesisKCount = 100; 

    public const float PhotosyntesisK = 0.3f;

    public PointElement PointElement { get;}
    public List<Vector3> GroupPositions { get; private set; }

    public void Connect(Vector3 position)
    {
        GroupPositions.Add(position);
    }

    public TreeElement(int treeID, GeneticElement geneticElement, PointElement pointElement, Color color)
    {
        TreeID = treeID;
        this.GeneticElement = geneticElement;
        PointElement = pointElement;
        this.Color = color;
        GroupPositions = new List<Vector3>();
    }

    public byte[] GetGen(byte CellGenNumber)
    {
        if (CellGenNumber >= GeneticElement.GeneticData.Length)
        {
            Debug.LogError($"[TreeElement] Invalid CellGenNumber: {CellGenNumber}. It must be between 0 and {GeneticElement.GeneticData.Length - 1}.");
            return null;
        }
        return GeneticElement.GeneticData[CellGenNumber];
    }

    public void Destroy(CellGrid cellGrid)
    {
        foreach (var position in GroupPositions)
            {
                cellGrid.RemoveCell(position);
            }
    }
}
}
