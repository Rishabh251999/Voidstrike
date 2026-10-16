using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class NpcNodeData
{
    public Ship ship;
    public Pilot pilot;
    public ShipCombatStats shipCombatStats;
    public int spawnPlace;
    public string shipName; // This is to get the prefab by name.
    public string shipFunName;
}

[CreateAssetMenu(menuName = "Scriptable Objects/ Node's Data")]
public class NodeData : ScriptableObject
{
    public List<string> adjacentNodes = new();
    public bool discovered;
    public List<GameObject> npcShips; // Possible ships that may appear in the node.
    public int totalNpcsPerShipClass;
    public int totalNodeShips;
    public DateTime resetTime; // This is a random date/time between 1 and 5 minutes from now which is used for respawning the node
    public List<NpcNodeData> currentShips;
}

