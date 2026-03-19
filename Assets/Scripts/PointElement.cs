using UnityEngine;
namespace PlantsEvolution
{
public class PointElement
{
    private const int initialPoints = 0;
    public int Points { get; private set; }
    public void AddPoints(int amount)
    {
        Points += amount;
    }
    public void Add()
    {
        AddPoints(1);
    }
    public PointElement()
    {
        Points = initialPoints;
    }
}
}
