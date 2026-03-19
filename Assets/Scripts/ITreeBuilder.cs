using UnityEngine;
namespace PlantsEvolution
{

public interface ITreeBuilder
    {
        public void BuildGeneticElement();
        public void BuildPointElement();
        TreeElement GetTreeElement();
        public void SetTreeID(int id);
        public void Clear();
    }
}