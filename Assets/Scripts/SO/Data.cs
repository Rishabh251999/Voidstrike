using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class ShipCombatStatsAllData
{
    public float hull;
    public float maxHull;
    public float shields;
    public float evasion;
    public float jumpSpeed;
}

[CreateAssetMenu(menuName = "Scriptable Objects/ Data")]
public class Data : ScriptableObject
{
    public string whichScene;
    public string previousScene;

    public float volumeValue;
    public float sfxVolumeValue;
    public float shipHealth;
    public float shipMaxHealth;

    public PilotData pilotData;
    public ShipData shipData;

    public List<PodData> podDatas;
    public List<WeaponData> weaponDatas;
    public List<GameObject> nodeNpcShips;
    public List<string> shipPlans;
    public List<string> discoveredNodes;
    public List<Mission> missionsData;
    public List<Sprite> leftShipImages;
    public List<Sprite> frontShipImages;
    public List<Sprite> redShipImages;

    public string actualNode;
    public string lastVisitedPlanet;
    public string playerName;

    public int currency;
    public int kills;
    public int pvpKills;
    public int slot;
    public string playerId;
    // public bool isDead;
    public ShipCombatStatsAllData shipCombatStatsAllData;
    public List<NodeData> nodesData;
    public List<string> shipFunNames;
}