using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ Pod's Data")]
public class PodData : ScriptableObject
{
    public string pod_Name;
    public int price;

    public Sprite podAvatar;

    public PodsData data = new();
}

[System.Serializable]
public class PodsData
{
    public enum Data_Type
    {
        Hull,
        Shields,
        Speed,
        Evasion
    }

    public Data_Type data_Type;
    public int value;
}