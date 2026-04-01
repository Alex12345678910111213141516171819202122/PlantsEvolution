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

        public bool AddCell(CellGrid cellGrid, TreeElement tree, byte genValue, Vector3 position)
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
                if (cellGrid.Cells.TryGetValue(position, out CellElement cellElement))
                {
                    if (cellElement.CellID < 0 || cellElement.CellID >= treeElements.Count)
                    {
                        Debug.LogWarning($"[Process] Invalid CellID {cellElement.CellID} for position {position}. Skipping.");
                        continue;
                    }

                    TreeElement tree = treeElements[cellElement.CellID];
                    if (tree == null)
                    {
                        Debug.LogWarning($"[Process] Tree with id {cellElement.CellID} is null. Skipping.");
                        continue;
                    }

                    byte[] Gen = tree.GetGen(cellElement.CellGenNumber);
                    if (Gen == null)
                    {
                        AddGrowthPosition(position);
                        continue;
                    }

                    bool grewUp = false;
                    int availableGrowths = Mathf.FloorToInt(tree.PointElement.Points / 4.0f);
                    if (availableGrowths <= 0)
                    {
                        AddGrowthPosition(position);
                        continue;
                    }

                    int directionsCount = Mathf.Min(growthOffsets.Count, Gen.Length - 1);
                    for (int i = 0; i < directionsCount; i++)
                    {
                        byte genValue = (byte)Gen[i];
                        if (genValue != 0 && availableGrowths > 0)
                        {
                            Vector3 newPosition = position + growthOffsets[i];
                            if (AddCell(cellGrid, tree, genValue, newPosition))
                            {
                                tree.PointElement.Remove(4);
                                availableGrowths -= 1;
                                grewUp = true;
                            }
                        }
                    }

                    if (!grewUp)
                    {
                        AddGrowthPosition(position);
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
