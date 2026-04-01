using UnityEngine;
namespace PlantsEvolution
{
public class PointElement
{
    private const int initialPoints = 16;
    public float Points { get; private set; }
    public int Score { get; private set; }
    public void AddPoints(float amount)
    {
        if(amount < 0)
        {
            Debug.LogError($"[PointElement] Cannot add negative points: {amount}. Use Remove() method instead.");
            return;
        }
        Points += amount;
        Score += Mathf.RoundToInt(amount);
    }
    public void Remove(float amount)
    {
        if (amount > Points)
        {
            Points = 0;
        }
        else
        {
           Points -= amount;
        }
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
