using System.Collections.Generic;
using UnityEngine;

public class MissionsData
{
    public List<Mission> missions = new();
}

[System.Serializable]
public class Mission
{
    public int id;
    public string description;
    public int reward;
    public string scene;
    public string action;
    public TargetData target = new();
    public bool enabled;
    public bool completed;
}

[System.Serializable]
public class TargetData
{
    public string type;
    public string name;
}