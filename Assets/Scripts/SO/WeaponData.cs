using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ Weapon's Data")]
public class WeaponData : ScriptableObject
{
    public string weapon_Name;
    public int price;
    public string description;
    public string weapon_type;

    public Sprite weaponAvatar;

    public List<WeaponsData> data = new();
}


[System.Serializable]
public class WeaponsData
{
    public enum Data_Type
    {
        Accuracy,
        FiringRate,
        Damage,
    }

    public Data_Type data_Type;
    public int value;
}