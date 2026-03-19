using System;
using Unity.VisualScripting;
using UnityEngine;
#nullable enable

namespace PlantsEvolution
{
public class CellElement
    {
        public CellElement(int cellID, int cellGenNumber)
        {
            CellID = cellID;
            CellGenNumber = cellGenNumber;
        }
        public int CellID { get; private set; }
        public int CellGenNumber { get; private set; }

    }
}
