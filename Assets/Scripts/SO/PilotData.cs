using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ Pilot's Data")]
public class PilotData : ScriptableObject
{
    public string pilotName;
    public string nickName;
    public string origin;
    public string description;
    public Sprite pilotAvatar;
    public List<Skill> skills = new();
}


[System.Serializable]
public class Skill
{   
    public enum Skill_Type
    {
        Speed,
        Evasion,
        Shields,
        Hull,
        Cargo,
        RepairCost,
        XPRequired,
        Price
    }

    public Skill_Type skill_Type;
    public int value;
}