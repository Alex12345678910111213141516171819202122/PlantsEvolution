using System;
using System.Collections.Generic;
using UnityEngine;
namespace PlantsEvolution
{
    public class CommonTreeBuilder : ITreeBuilder
    {
        TreeElement _treeElement;
        private int ID;
        private GeneticElement _geneticElement;
        private PointElement _pointElement;
        
        private static readonly Color[] ColorPool = new Color[]
        {
            new Color(0.22f, 0.71f, 0.29f), // приглушённый зелёный
            new Color(0.95f, 0.77f, 0.20f), // тёплый жёлтый
            new Color(0.85f, 0.33f, 0.25f), // терракотовый
            new Color(0.25f, 0.50f, 0.88f), // синий
            new Color(0.65f, 0.35f, 0.75f), // фиолетовый
            new Color(0.20f, 0.75f, 0.72f), // бирюзовый
            new Color(0.95f, 0.55f, 0.20f), // оранжевый
            new Color(0.88f, 0.40f, 0.62f), // розовый
        };

        private int colorIndex = 0;
        private int ColorIndex
        {
            get
            {
                int index = colorIndex;
                colorIndex = (colorIndex + 1) % ColorPool.Length;
                return index;
            }
        }

        public void BuildGeneticElement()
        {
            _geneticElement = new GeneticElement(5, 2, 6);
        }

        public void BuildPointElement()
        {
            _pointElement = new PointElement();
        }

        public void SetTreeID(int id)
        {
            ID = id;
        }

        public TreeElement GetTreeElement()
        {
            if (_treeElement == null)
            {
                Debug.Log($"[Genetics] Assembling tree element with ID: {ID}");
                _treeElement = new TreeElement(ID, _geneticElement, _pointElement, ColorPool[ColorIndex]);
            }
            return _treeElement;
        }

        public void Clear()
        {
            _treeElement = null;
        }
    }
}
