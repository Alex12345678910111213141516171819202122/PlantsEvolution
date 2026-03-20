using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
namespace PlantsEvolution
{
    public class GrowthProcess
    {
        List<Vector3> growthOffsets;
        List<Vector3> nextGrowthPositions;
        private readonly float cellSize;

        public GrowthProcess(float cellSize)
        {
            this.cellSize = cellSize;
            growthOffsets = new List<Vector3>()
        {
            new Vector3(1,0,0) * cellSize,
            new Vector3(-1,0,0) * cellSize,
            new Vector3(0,1,0) * cellSize,
            new Vector3(0,-1,0) * cellSize,
            new Vector3(0,0,1) * cellSize,
            new Vector3(0,0,-1) * cellSize
        };
            nextGrowthPositions = new List<Vector3>();
            Debug.Log("[GrowthProcess] Initialized with empty growth positions list");
        }

        public void AddGrowthPosition(Vector3 position)
        {
            if (!nextGrowthPositions.Contains(position))
            {
                nextGrowthPositions.Add(position);
            }
        }

        public bool AddCell(CellGrid cellGrid, TreeElement tree, int genValue, Vector3 position)
        {
            if(cellGrid.AddCell(genValue, tree.TreeID, ref position))
            {
                tree.Connect(position);
                AddGrowthPosition(position);
                return true;
            }
            return false;
        }

        public void ClearGrowthPositions()
        {
            nextGrowthPositions.Clear();
        }

        public void Process(List<TreeElement> treeElements, CellGrid cellGrid)
        {
            List<Vector3> oldGrowthPositions = new List<Vector3>(nextGrowthPositions);
            nextGrowthPositions.Clear();
            if (oldGrowthPositions.Count == 0)
            {
                Debug.LogWarning("[Process] No growth positions to process");
                return;
            }
            foreach (var position in oldGrowthPositions)
            {
                if(cellGrid.Cells.ContainsKey(position))
                {
                    CellElement cellElement = cellGrid.Cells[position];
                    TreeElement tree = treeElements[cellElement.CellID];

                    int[] Gen = tree.GetGen(cellElement.CellGenNumber);
                    if (Gen == null)
                    {
                        continue;
                    }

                    int directionsCount = Mathf.Min(growthOffsets.Count, Gen.Length - 1);
                    for (int i = 0; i < directionsCount; i++)
                    {
                        int genValue = Gen[i];
                        if (genValue != 0)
                        {
                            Vector3 newPosition = position + growthOffsets[i];
                            AddCell(cellGrid, tree, genValue, newPosition);
                        }
                    }
                }
                else
                {
                    Debug.LogWarning($"[Process] No cell found at position: {position}");
                }
            }
        }
    }
}
