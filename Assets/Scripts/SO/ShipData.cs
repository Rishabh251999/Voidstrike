using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Scriptable Objects/ Ship's Data")]
public class ShipData : ScriptableObject
{
    public enum ShipClass
    {
        I,
        II,
        III,
    }

    public enum ShipType
    {
        Scout,
        Spectre,
        Predator,
        Guardian,
        Transport
    }

    public ShipClass shipClass;
    public ShipType shipType;

    public int price;
    public int hull;
    public int shields;
    public int jumpSpeed;
    public int evasion;
    public int aggressionFactor;
    public int maxGuns;
    public int maxMiscPods;
    public string id;
    public int rewardMyth;
    public List<string> classShipNames;
    public Sprite shipAvatar;
}
