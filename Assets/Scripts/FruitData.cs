// ============================================================
//  FruitData.cs  —  fruit type definitions & static database
// ============================================================
using UnityEngine;

public enum FruitType
{
    Cherry = 0, Strawberry, Grape, Orange, Apple,
    Pear, Peach, Pineapple, Watermelon, Melon, Durian
}

[System.Serializable]
public class FruitData
{
    public FruitType type;
    public string    fruitName;
    public float     radius;        // world-space physics radius
    public Color     primaryColor;
    public Color     edgeColor;
    public int       scoreValue;    // awarded when two of these merge

    public FruitData(FruitType t, string n, float r,
                     Color primary, Color edge, int score)
    { type=t; fruitName=n; radius=r; primaryColor=primary; edgeColor=edge; scoreValue=score; }
}

public static class FruitDatabase
{
    public static readonly FruitData[] All =
    {
        new(FruitType.Cherry,    "Cherry",    0.54f, new Color(0.92f,0.12f,0.18f), new Color(0.55f,0.03f,0.08f), 1 ),
        new(FruitType.Strawberry,"Strawberry",0.74f, new Color(1.00f,0.22f,0.28f), new Color(0.70f,0.08f,0.12f), 3 ),
        new(FruitType.Grape,     "Grape",     1.00f, new Color(0.55f,0.18f,0.82f), new Color(0.30f,0.05f,0.50f), 6 ),
        new(FruitType.Orange,    "Orange",    1.28f, new Color(1.00f,0.55f,0.05f), new Color(0.80f,0.33f,0.00f), 10),
        new(FruitType.Apple,     "Apple",     1.60f, new Color(0.18f,0.80f,0.18f), new Color(0.08f,0.50f,0.08f), 15),
        new(FruitType.Pear,      "Pear",      1.92f, new Color(0.80f,0.90f,0.15f), new Color(0.50f,0.62f,0.05f), 21),
        new(FruitType.Peach,     "Peach",     2.24f, new Color(1.00f,0.68f,0.52f), new Color(0.82f,0.42f,0.25f), 28),
        new(FruitType.Pineapple, "Pineapple", 2.64f, new Color(1.00f,0.88f,0.08f), new Color(0.78f,0.62f,0.00f), 36),
        new(FruitType.Watermelon,"Watermelon",3.16f, new Color(0.12f,0.72f,0.20f), new Color(0.06f,0.45f,0.08f), 45),
        new(FruitType.Melon,     "Melon",     3.60f, new Color(0.95f,0.95f,0.60f), new Color(0.75f,0.75f,0.40f), 55),
        new(FruitType.Durian,    "Durian",    4.20f, new Color(0.70f,0.65f,0.15f), new Color(0.40f,0.35f,0.05f), 66),
    };

    public static int       Count               => All.Length;
    public static FruitData Get(int i)          => All[i];
    public static FruitData Get(FruitType t)    => All[(int)t];
    public static bool      HasNext(FruitType t)=> (int)t < Count - 1;
    public static FruitData Next(FruitType t)   => All[(int)t + 1];
}