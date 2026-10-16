using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using LootLocker.Requests;
using System.IO;
using UnityEngine.Networking;

[Serializable]
public class Pilot
{
    public List<PilotStats> pilotStat;
}

[Serializable]
public class PilotStats
{
    [HideInInspector] public string statName;
    public float statValue;
}

[Serializable]
public class Gun
{
    [HideInInspector] public string gunName;
    public float accuracy;
    public float firingRate;
    public float damage;
}

[Serializable]
public class Pod
{
    [HideInInspector] public string podName;
    public float increasedValue;
}

[Serializable]
public class Ship
{
    public ShipData shipData;

    public float hull;
    public float shields;
    public float jumpSpeed;
    public float evasion;
    public int maxGuns;
    public int maxPods;

    public List<Pod> pods;
    public List<Gun> guns;
}

[Serializable]
public class ShipCombatStats
{
    public float hull;
    public float maxHull;
    public float shields;
    public float evasion;
    public float jumpSpeed;
}

public class BattleManager : MonoBehaviour
{
    [Header("Classes")]
    public Ship playerShip;
    public Pilot playerPilot;

    [Space(10)]

    public Ship enemyShip;
    public Pilot enemyPilot;
    public int spawnPlace;


    [Header("bool")]
    public bool isEngaged = false;
    public bool alreadyEngaged = false;

    [Header("Float")]
    public float battleDuration = 0f; // Time taken between each consecutive shots.
    // private float playerDamageTaken = 0;
    private float enemyDamageTaken = 0f;
    private float NpcDamageRatio;

    [Space(10)]

    [Header("BattleReady Stats")]
    public ShipCombatStats playerStats;
    public ShipCombatStats enemyStats;

    [Header("CoRoutine")]
    private Coroutine battleCoroutine;


    private void Start()
    {
        if(gameObject.CompareTag("Player"))
        {
            InitializePlayerData();
            playerStats = CalculateCombinedStats(playerPilot, playerShip);
            StoreCombinedStats();
        }

        if(gameObject.CompareTag("Ships"))
        {
            int nodesDataIndex = CockPitManager.Instance.data.nodesData.FindIndex(nodeData => nodeData.name == CockPitManager.Instance.data.actualNode);

            if(nodesDataIndex == -1 || (nodesDataIndex >= 0 && DateTime.Now >= CockPitManager.Instance.data.nodesData[nodesDataIndex].resetTime)) // Add if the lastupdated time is less or greater than now.
            {
                InitializeEnemyPlayerData(); // Completes playerShip
                playerStats = CalculateCombinedStats(playerPilot, playerShip); // Completes playerStats
            }

            InitializePlayerEnemyData();  // Completes enemyShip
            GetComponent<Enemy>().permanentMaxHull = playerStats.maxHull;

            enemyStats = CalculateCombinedStats(CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerPilot, 
                CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().playerShip);

        } 
    }


    /// <summary>
    /// This will fill all the data of ourselves.
    /// </summary>
    public void InitializePlayerData()
    {
        // Initialize Pilot first. Doing this check because these two pilots have the attributes that are not required duribng battle. 
        if(!CockPitManager.Instance.data.pilotData.pilotName.Equals("Female Alien") && !CockPitManager.Instance.data.pilotData.pilotName.Equals("Human Male"))
        {
            for (int i = 0; i < CockPitManager.Instance.data.pilotData.skills.Count; i++)
            {
                playerPilot.pilotStat.Add(new());

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Hull"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Shields"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Speed"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Evasion"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }
            }
        }

        // Initialize Ship data now.
        playerShip.shipData = CockPitManager.Instance.data.shipData;
        playerShip.hull = CockPitManager.Instance.data.shipData.hull;
        playerShip.shields = CockPitManager.Instance.data.shipData.shields;
        playerShip.jumpSpeed = CockPitManager.Instance.data.shipData.jumpSpeed;
        playerShip.evasion = CockPitManager.Instance.data.shipData.evasion;
        playerShip.maxGuns = CockPitManager.Instance.data.shipData.maxGuns;
        playerShip.maxPods = CockPitManager.Instance.data.shipData.maxMiscPods;

        for (int i = 0; i < CockPitManager.Instance.data.weaponDatas.Count; i++)
        {
            playerShip.guns.Add(new Gun());

            playerShip.guns[i].gunName = CockPitManager.Instance.data.weaponDatas[i].weapon_Name;

            for (int j = 0; j < 3; j++)
            {
                if (CockPitManager.Instance.data.weaponDatas[i].data[j].data_Type.ToString().Equals("Accuracy"))
                    playerShip.guns[i].accuracy = CockPitManager.Instance.data.weaponDatas[i].data[j].value;
                else if (CockPitManager.Instance.data.weaponDatas[i].data[j].data_Type.ToString().Equals("FiringRate"))
                    playerShip.guns[i].firingRate = CockPitManager.Instance.data.weaponDatas[i].data[j].value;
                else if (CockPitManager.Instance.data.weaponDatas[i].data[j].data_Type.ToString().Equals("Damage"))
                    playerShip.guns[i].damage = CockPitManager.Instance.data.weaponDatas[i].data[j].value;
            }

        }

        for (int i = 0; i < CockPitManager.Instance.data.podDatas.Count; i++)
        {
            playerShip.pods.Add(new Pod());

            if (CockPitManager.Instance.data.podDatas[i].pod_Name.Equals("Hull Augment"))
            {
                playerShip.pods[i].podName = CockPitManager.Instance.data.podDatas[i].pod_Name;
                playerShip.pods[i].increasedValue = CockPitManager.Instance.data.podDatas[i].data.value;
            }

            else if (CockPitManager.Instance.data.podDatas[i].pod_Name.Equals("Shield Amplifier"))
            {
                playerShip.pods[i].podName = CockPitManager.Instance.data.podDatas[i].pod_Name;
                playerShip.pods[i].increasedValue = CockPitManager.Instance.data.podDatas[i].data.value;
            }

            else if (CockPitManager.Instance.data.podDatas[i].pod_Name.Equals("Jump Drive"))
            {
                playerShip.pods[i].podName = CockPitManager.Instance.data.podDatas[i].pod_Name;
                playerShip.pods[i].increasedValue = CockPitManager.Instance.data.podDatas[i].data.value;
            }
        }
    }


    /// <summary>
    /// This will fill all the data of the enemy.
    /// </summary>
    public void InitializePlayerEnemyData()
    {
        // Initialize Pilot first. Doing this check because these two pilots have the attributes that are not required duribng battle.
        if (!CockPitManager.Instance.data.pilotData.pilotName.Equals("Female Alien") && !CockPitManager.Instance.data.pilotData.pilotName.Equals("Human Male"))
        {
            for (int i = 0; i < CockPitManager.Instance.data.pilotData.skills.Count; i++)
            {
                enemyPilot.pilotStat.Add(new());

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Hull"))
                {
                    enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Shields"))
                {
                    enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Speed"))
                {
                    enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Evasion"))
                {
                    enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }
            }
        }

        // Initialize Ship data now.
        enemyShip.shipData = CockPitManager.Instance.data.shipData;
        enemyShip.hull = CockPitManager.Instance.data.shipData.hull;
        enemyShip.shields = CockPitManager.Instance.data.shipData.shields;
        enemyShip.jumpSpeed = CockPitManager.Instance.data.shipData.jumpSpeed;
        enemyShip.evasion = CockPitManager.Instance.data.shipData.evasion;
        enemyShip.maxGuns = CockPitManager.Instance.data.shipData.maxGuns;
        enemyShip.maxPods = CockPitManager.Instance.data.shipData.maxMiscPods;

        for (int i = 0; i < CockPitManager.Instance.data.weaponDatas.Count; i++)
        {
            enemyShip.guns.Add(new Gun());

            enemyShip.guns[i].gunName = CockPitManager.Instance.data.weaponDatas[i].weapon_Name;

            for (int j = 0; j < 3; j++)
            {
                if (CockPitManager.Instance.data.weaponDatas[i].data[j].data_Type.ToString().Equals("Accuracy"))
                    enemyShip.guns[i].accuracy = CockPitManager.Instance.data.weaponDatas[i].data[j].value;
                else if (CockPitManager.Instance.data.weaponDatas[i].data[j].data_Type.ToString().Equals("FiringRate"))
                    enemyShip.guns[i].firingRate = CockPitManager.Instance.data.weaponDatas[i].data[j].value;
                else if (CockPitManager.Instance.data.weaponDatas[i].data[j].data_Type.ToString().Equals("Damage"))
                    enemyShip.guns[i].damage = CockPitManager.Instance.data.weaponDatas[i].data[j].value;
            }

        }

        for (int i = 0; i < CockPitManager.Instance.data.podDatas.Count; i++)
        {
            enemyShip.pods.Add(new Pod());

            if (CockPitManager.Instance.data.podDatas[i].pod_Name.Equals("Hull Augment"))
            {
                enemyShip.pods[i].podName = CockPitManager.Instance.data.podDatas[i].pod_Name;
                enemyShip.pods[i].increasedValue = CockPitManager.Instance.data.podDatas[i].data.value;
            }

            else if (CockPitManager.Instance.data.podDatas[i].pod_Name.Equals("Shield Amplifier"))
            {
                enemyShip.pods[i].podName = CockPitManager.Instance.data.podDatas[i].pod_Name;
                enemyShip.pods[i].increasedValue = CockPitManager.Instance.data.podDatas[i].data.value;
            }

            else if (CockPitManager.Instance.data.podDatas[i].pod_Name.Equals("Jump Drive"))
            {
                enemyShip.pods[i].podName = CockPitManager.Instance.data.podDatas[i].pod_Name;
                enemyShip.pods[i].increasedValue = CockPitManager.Instance.data.podDatas[i].data.value;
            }
        }
    }


    /// <summary>
    /// This will fill all the data of ourselves.
    /// </summary>
    public void InitializeEnemyPlayerData()
    {
        playerPilot.pilotStat.Clear();

        // Initialize Pilot first. Doing this check because these two pilots have the attributes that are not required duribng battle.
        if (!CockPitManager.Instance.data.pilotData.pilotName.Equals("Female Alien") && !CockPitManager.Instance.data.pilotData.pilotName.Equals("Human Male"))
        {
            for (int i = 0; i < CockPitManager.Instance.data.pilotData.skills.Count; i++)
            {
                playerPilot.pilotStat.Add(new());

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Hull"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Shields"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Speed"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }

                if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Evasion"))
                {
                    playerPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                    playerPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;
                }
            }
        }

        // Initialize enemy Ship data now.
        playerShip.pods.Clear();
        playerShip.guns.Clear();

        int randomNumberForPod = UnityEngine.Random.Range(0, GetComponent<Enemy>().shipdata.maxMiscPods);
        int randomNumberForWeapons = UnityEngine.Random.Range(1, GetComponent<Enemy>().shipdata.maxGuns);

        playerShip.shipData = GetComponent<Enemy>().shipdata;
        playerShip.hull = GetComponent<Enemy>().shipdata.hull;
        playerShip.shields = GetComponent<Enemy>().shipdata.shields;
        playerShip.jumpSpeed = GetComponent<Enemy>().shipdata.jumpSpeed;
        playerShip.evasion = GetComponent<Enemy>().shipdata.evasion;
        playerShip.maxGuns = randomNumberForWeapons;
        playerShip.maxPods = randomNumberForPod;

        for (int i = 0; i < playerShip.maxGuns; i++)
        {
            playerShip.guns.Add(new Gun());

            int randomWeaponNumber = UnityEngine.Random.Range(0, GetComponent<Enemy>().shipdata.maxGuns);

            playerShip.guns[i].gunName = CockPitManager.Instance.weaponDatas[randomWeaponNumber].weapon_Name;

            for (int j = 0; j < 3; j++)
            {
                if(CockPitManager.Instance.weaponDatas[randomWeaponNumber].data[j].data_Type.ToString().Equals("Accuracy"))
                {
                    playerShip.guns[i].accuracy =
                        CockPitManager.Instance.weaponDatas[randomWeaponNumber].data[j].value;
                }

                else if (CockPitManager.Instance.weaponDatas[randomWeaponNumber].data[j].data_Type.ToString().Equals("FiringRate"))
                {
                    playerShip.guns[i].firingRate =
                        CockPitManager.Instance.weaponDatas[randomWeaponNumber].data[j].value;
                }

                else if (CockPitManager.Instance.weaponDatas[randomWeaponNumber].data[j].data_Type.ToString().Equals("Damage"))
                {
                    playerShip.guns[i].damage =
                        CockPitManager.Instance.weaponDatas[randomWeaponNumber].data[j].value;
                }
            }
        }

        for (int i = 0; i < playerShip.maxPods; i++)
        {
            playerShip.pods.Add(new Pod());

            int randomPodNumber = UnityEngine.Random.Range(0, GetComponent<Enemy>().shipdata.maxMiscPods - 1);

            playerShip.pods[i].podName = CockPitManager.Instance.podDatas[randomPodNumber].pod_Name;

            playerShip.pods[i].increasedValue = CockPitManager.Instance.podDatas[randomPodNumber].data.value;
        }
    }


    /// <summary>
    /// This will fill all the data of the enemy in the player battle manager.
    /// </summary>
    public void FillEnemyDataForPlayerManager(bool value)
    {
        var playerBattleManager = CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>();
        var enemyShipObject = CockPitManager.Instance.enemyShipObject.GetComponent<BattleManager>();
        var enemyShipData = CockPitManager.Instance.enemyShipData;

        if (value)
        {
            enemyShipData.shipData = CockPitManager.Instance.enemyShipObject.GetComponent<Enemy>().shipdata;

            playerBattleManager.enemyShip.shipData = CockPitManager.Instance.enemyShipData.shipData;

            enemyShipData.hull = enemyShipObject.playerShip.hull;
            enemyShipData.shields = enemyShipObject.playerShip.shields;
            enemyShipData.jumpSpeed = enemyShipObject.playerShip.jumpSpeed;
            enemyShipData.evasion = enemyShipObject.playerShip.evasion;
            enemyShipData.maxGuns = enemyShipObject.playerShip.maxGuns;
            enemyShipData.maxPods = enemyShipObject.playerShip.maxPods;

            playerBattleManager.enemyShip.hull = enemyShipData.hull;
            playerBattleManager.enemyShip.shields = enemyShipData.shields;
            playerBattleManager.enemyShip.jumpSpeed = enemyShipData.jumpSpeed;
            playerBattleManager.enemyShip.evasion = enemyShipData.evasion;
            playerBattleManager.enemyShip.maxGuns = enemyShipData.maxGuns;
            playerBattleManager.enemyShip.maxPods = enemyShipData.maxPods;

            playerBattleManager.enemyShip.pods.Clear();
            playerBattleManager.enemyShip.guns.Clear();
            playerBattleManager.enemyPilot.pilotStat.Clear();

            CockPitManager.Instance.enemyShipData.pod.Clear();
            CockPitManager.Instance.enemyShipData.weapon.Clear();

            for (int i = 0; i < playerBattleManager.enemyShip.maxPods; i++)
            {
                playerBattleManager.enemyShip.pods.Add(new());
                CockPitManager.Instance.enemyShipData.pod.Add(new());

                playerBattleManager.enemyShip.pods[i] = enemyShipObject.playerShip.pods[i];

                CockPitManager.Instance.enemyShipData.pod[i].podName = playerBattleManager.enemyShip.pods[i].podName;
                CockPitManager.Instance.enemyShipData.pod[i].increasedValue = playerBattleManager.enemyShip.pods[i].increasedValue;
            }

            for (int i = 0; i < playerBattleManager.enemyShip.maxGuns; i++)
            {
                playerBattleManager.enemyShip.guns.Add(new());
                CockPitManager.Instance.enemyShipData.weapon.Add(new());

                playerBattleManager.enemyShip.guns[i] = enemyShipObject.playerShip.guns[i];

                CockPitManager.Instance.enemyShipData.weapon[i].gunName = playerBattleManager.enemyShip.guns[i].gunName;

                CockPitManager.Instance.enemyShipData.weapon[i].accuracy = playerBattleManager.enemyShip.guns[i].accuracy;
                CockPitManager.Instance.enemyShipData.weapon[i].firingRate = playerBattleManager.enemyShip.guns[i].firingRate;
                CockPitManager.Instance.enemyShipData.weapon[i].damage = playerBattleManager.enemyShip.guns[i].damage;
            }

            // Initialize Pilot first. Doing this check because these two pilots have the attributes that are not required duribng battle.
            if (!CockPitManager.Instance.data.pilotData.pilotName.Equals("Female Alien") && !CockPitManager.Instance.data.pilotData.pilotName.Equals("Human Male"))
            {
                for (int i = 0; i < CockPitManager.Instance.data.pilotData.skills.Count; i++)
                {
                    playerBattleManager.enemyPilot.pilotStat.Add(new());
                    enemyShipData.pilot.Add(new());

                    if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Hull"))
                    {
                        playerBattleManager.enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                        playerBattleManager.enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;

                        enemyShipData.pilot[i].statName = playerBattleManager.enemyPilot.pilotStat[i].statName;
                        enemyShipData.pilot[i].statValue = playerBattleManager.enemyPilot.pilotStat[i].statValue;
                    }

                    if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Shields"))
                    {
                        playerBattleManager.enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                        playerBattleManager.enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;

                        enemyShipData.pilot[i].statName = playerBattleManager.enemyPilot.pilotStat[i].statName;
                        enemyShipData.pilot[i].statValue = playerBattleManager.enemyPilot.pilotStat[i].statValue;
                    }

                    if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Speed"))
                    {
                        playerBattleManager.enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                        playerBattleManager.enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;

                        enemyShipData.pilot[i].statName = playerBattleManager.enemyPilot.pilotStat[i].statName;
                        enemyShipData.pilot[i].statValue = playerBattleManager.enemyPilot.pilotStat[i].statValue;
                    }

                    if (CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString().Equals("Evasion"))
                    {
                        playerBattleManager.enemyPilot.pilotStat[i].statName = CockPitManager.Instance.data.pilotData.skills[i].skill_Type.ToString();
                        playerBattleManager.enemyPilot.pilotStat[i].statValue = CockPitManager.Instance.data.pilotData.skills[i].value;

                        enemyShipData.pilot[i].statName = playerBattleManager.enemyPilot.pilotStat[i].statName;
                        enemyShipData.pilot[i].statValue = playerBattleManager.enemyPilot.pilotStat[i].statValue;
                    }
                }
            }
        }

        else
        {
            enemyShipData.shipData = null;
            enemyShipData.hull = 0;
            enemyShipData.shields = 0;
            enemyShipData.jumpSpeed = 0;
            enemyShipData.evasion = 0;
            enemyShipData.maxGuns = 0;

            enemyShipData.pod.Clear();
            enemyShipData.weapon.Clear();

            enemyShipData.pilot.Clear();
        }
    }


    /// <summary>
    /// This function and add everthing and make a conbined stat.
    /// </summary>
    public ShipCombatStats CalculateCombinedStats(Pilot pilot, Ship ship)
    {
        ShipCombatStats stats = new()
        {
            hull = ship.hull,
            jumpSpeed = ship.jumpSpeed,
            evasion = ship.evasion,
            shields = ship.shields,
        };

        float hullMultiplier = 1.0f;
        float ShieldMultiplier = 1.0f;
        float SpeedMultiplier = 1.0f;
        float EvasionMultiplier = 1.0f;

        pilot.pilotStat.ForEach(a =>
        {
            if (a.statName.Equals("Hull"))
                hullMultiplier += a.statValue / 100;
            if (a.statName.Equals("Shields"))
                ShieldMultiplier += a.statValue / 100;
            if (a.statName.Equals("Speed"))
                SpeedMultiplier += a.statValue / 100;
            if (a.statName.Equals("Evasion"))
                EvasionMultiplier += a.statValue / 100;
        });

        ship.pods.ForEach(a =>
        {
            if (a.podName.Equals("Hull Augment"))
                stats.hull += a.increasedValue;
            if (a.podName.Equals("Shield Amplifier"))
                stats.shields += a.increasedValue;
            if (a.podName.Equals("Jump Drive"))
                stats.jumpSpeed += a.increasedValue;
        });

        stats.maxHull = stats.hull * hullMultiplier;

        if (gameObject.CompareTag("Ships"))
        {
            stats.hull = stats.maxHull;
        }
        else if(gameObject.CompareTag("Player"))
        {
            CockPitManager.Instance.data.shipMaxHealth = stats.maxHull;
            stats.hull = stats.maxHull * CockPitManager.Instance.data.shipHealth;
        }
            
        stats.shields = stats.shields * ShieldMultiplier;
        stats.jumpSpeed = stats.jumpSpeed * SpeedMultiplier;
        stats.evasion = stats.evasion * EvasionMultiplier;

        return stats;
    }


    /// <summary>
    /// To store the ship data for AllData SO.
    /// </summary>
    public void StoreCombinedStats()
    {
        CockPitManager.Instance.data.shipCombatStatsAllData.hull = playerStats.hull;
        CockPitManager.Instance.data.shipCombatStatsAllData.maxHull = playerStats.maxHull;
        CockPitManager.Instance.data.shipCombatStatsAllData.shields = playerStats.shields;
        CockPitManager.Instance.data.shipCombatStatsAllData.evasion = playerStats.evasion;
        CockPitManager.Instance.data.shipCombatStatsAllData.jumpSpeed = playerStats.jumpSpeed;
    }


    /// <summary>
    /// 
    /// </summary>
    public ShipCombatStats CalculateUpdatedCombinedStats(Pilot pilot, Ship ship)
    {
        ShipCombatStats stats = new()
        {
            jumpSpeed = ship.jumpSpeed,
            evasion = ship.evasion,
            shields = ship.shields,
        };

        float hullMultiplier = 1.0f;
        float ShieldMultiplier = 1.0f;
        float SpeedMultiplier = 1.0f;
        float EvasionMultiplier = 1.0f;

        pilot.pilotStat.ForEach(a =>
        {
            if (a.statName.Equals("Hull"))
                hullMultiplier += a.statValue / 100;
            if (a.statName.Equals("Shields"))
                ShieldMultiplier += a.statValue / 100;
            if (a.statName.Equals("Speed"))
                SpeedMultiplier += a.statValue / 100;
            if (a.statName.Equals("Evasion"))
                EvasionMultiplier += a.statValue / 100;
        });

        ship.pods.ForEach(a =>
        {
            if (a.podName.Equals("Hull Augment"))
                stats.hull += a.increasedValue;
            if (a.podName.Equals("Shield Amplifier"))
                stats.shields += a.increasedValue;
            if (a.podName.Equals("Jump Drive"))
                stats.jumpSpeed += a.increasedValue;
        });

        if (gameObject.CompareTag("Player"))
            stats.maxHull = CockPitManager.Instance.selectedEnemyShip.GetComponent<Enemy>().permanentMaxHull;

        stats.hull = ship.hull * hullMultiplier;
        stats.shields = stats.shields * ShieldMultiplier;
        stats.jumpSpeed = stats.jumpSpeed * SpeedMultiplier;
        stats.evasion = stats.evasion * EvasionMultiplier;

        return stats;
    }


    /// <summary>
    /// This will start the battle visually between the player and the enemy.
    /// </summary>
    public void Engage()
    {
        if(CockPitManager.Instance.enemyShipObject)
        {
            CockPitManager.Instance.selectedEnemyShip = CockPitManager.Instance.enemyShipObject;

            // CockPitManager.Instance.selectedEnemyShip.GetComponent<Enemy>().DisengageFromPlayer();

            DisEngage();

            for (var i = 0; i < CockPitManager.Instance.enemyShipsParent.transform.childCount; i++)
            {
                if (CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().isEngaged)
                    CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().isEngaged = false;
            }

            CockPitManager.Instance.LaserRotator();

            if (CockPitManager.Instance.selectedEnemyShip != null)
            {
                CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().isEngaged = true;
                CockPitManager.Instance.selectedEnemyShip.transform.GetChild(0).gameObject.SetActive(true);
                CockPitManager.Instance.selectedEnemyShip.transform.GetChild(1).gameObject.SetActive(true);
                CockPitManager.Instance.selectedEnemyShip.transform.GetChild(1).GetChild(0).GetComponent<Image>().color = new(1, 0, 0, 1);
                CockPitManager.Instance.selectedEnemyShip.transform.GetChild(2).gameObject.SetActive(true);
                CockPitManager.Instance.selectedEnemyShip.transform.GetChild(3).gameObject.SetActive(true);
                CockPitManager.Instance.selectedEnemyShip.transform.GetChild(4).gameObject.SetActive(false);
                CockPitManager.Instance.selectedEnemyShip.GetComponent<Enemy>().NpcLaserRotator();
            }

            CockPitManager.Instance.enemyShipImage.color = new(1, 1, 1, 1);
            CockPitManager.Instance.enemyShipImage.sprite = enemyShip.shipData.shipAvatar;
            CockPitManager.Instance.enemyShipImage.preserveAspect = true;
            CockPitManager.Instance.enemyShipName.text = CockPitManager.Instance.selectedEnemyShip.GetComponent<Enemy>().enemyFunName;
            CockPitManager.Instance.enemyShipClassName.text = "Class " + enemyShip.shipData.shipClass + " " + enemyShip.shipData.shipType;
            CockPitManager.Instance.enemyHealthBarFill.transform.parent.gameObject.SetActive(true);

            var shipName = string.Empty;

            if (enemyShip.shipData.shipAvatar.name.Substring(0, 5).Equals("Front"))
                shipName = enemyShip.shipData.shipAvatar.name.Substring(6);

            if (enemyShip.shipData.shipAvatar.name.Substring(0, 4).Equals("Left"))
                shipName = enemyShip.shipData.shipAvatar.name.Substring(5);

            if (enemyShip.shipData.shipAvatar.name.Substring(0, 5).Equals("Right"))
                shipName = enemyShip.shipData.shipAvatar.name.Substring(6);

            CockPitManager.Instance.enemyShipAttackSprite.ForEach(sprite =>
            {
                if (sprite.name.Equals(shipName))
                    CockPitManager.Instance.enemyShipObject.GetComponent<Image>().sprite = sprite;
            });

            CockPitManager.Instance.fireLaser.SetActive(true);

            StartBattle();
        }
    }


    /// <summary>
    /// This will start the battle in backened between the player and the enemy. Call this function when we press the engage key.
    /// </summary>
    public void StartBattle()
    {
        if (isEngaged) return; // Prevent multiple engagements simultaneously

        if (gameObject.CompareTag("Player"))
        {
            enemyStats = CalculateCombinedStats(CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().playerPilot,
                        CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().playerShip);

            battleCoroutine =  StartCoroutine(EngageBattle(playerShip, enemyShip));
        }
    }


    /// <summary>
    /// This courotine basically calculates everything and starts giving damages/battle duration.
    /// </summary>
    private IEnumerator EngageBattle(Ship playerShip, Ship enemyShip)
    {
        float playerTotalFiringRate = CalculateTotalFiringRate(playerShip);
        float enemyTotalFiringRate = CalculateTotalFiringRate(enemyShip);

        float playerBattleDuration = CalculateBattleDuration(playerTotalFiringRate);
        float enemyBattleDuration = CalculateBattleDuration(enemyTotalFiringRate);

        yield return new WaitForSeconds(Mathf.Max(playerBattleDuration, enemyBattleDuration));

        isEngaged = true;
        CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().alreadyEngaged = true;

        if (enemyShip != null && playerShip != null)
        {
            while ((playerShip.hull >= 0 || enemyShip.hull >= 0) &&
            CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().isEngaged &&
            isEngaged)
            {
                // Reset the damage values at the start of each iteration.
                enemyDamageTaken = 0f;

                //Calculate damage taken for enemy from all player guns.
                for (int i = 0; i < playerShip.guns.Count; i++)
                {
                    enemyDamageTaken +=
                        CalculateDamageTaken(playerShip.guns[i].accuracy, enemyStats.evasion,
                            playerShip.guns[i].damage + CockPitManager.Instance.damageIncreaseForGuns,
                            enemyStats.shields, 0.0f);
                }

                if (enemyDamageTaken > 0)
                {
                    enemyStats.hull -= enemyDamageTaken;

                    CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().playerStats.hull -= enemyDamageTaken;
                    CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().playerShip.hull -= enemyDamageTaken;

                    NpcDamageRatio = 
                        CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().playerStats.hull / 
                        CockPitManager.Instance.selectedEnemyShip.GetComponent<Enemy>().permanentMaxHull;

                    NPCShipHealthDamage();
                    MusicPlayerManager.Instance.PlayMusic("Attack Hit");
                }
                else
                {
                    MusicPlayerManager.Instance.PlayMusic("Attack Evade");
                }

                yield return new WaitForSeconds(playerBattleDuration);
            }
        }
    }


    /// <summary>
    /// Calcues the average firing rate depending on how much guns we have.
    /// </summary>
    float CalculateTotalFiringRate(Ship ship)
    {
        if (ship.guns.Count == 0)
            return 0;

        float totalFiringRate = 0;
        ship.guns.ForEach(gun => totalFiringRate += gun.firingRate - 50); // 50 is to make the seconds between each shot range wider

        // Calculate the average by dividing the total by the number of guns.
        float averageFiringRate = totalFiringRate / ship.guns.Count;

        return averageFiringRate;
    }


    /// <summary>
    /// This calculates how much fast/slow the ships will fire.
    /// </summary>
    float CalculateBattleDuration(float totalFiringRate) => battleDuration = 35f / totalFiringRate;


    /// <summary>
    /// This is to reduce the health of NPC in UI.
    /// </summary>
    public void NPCShipHealthDamage()
    {
        CockPitManager.Instance.selectedEnemyShip.transform.GetChild(1).GetChild(0).GetComponent<Scrollbar>().size = NpcDamageRatio;
        if(CockPitManager.Instance.enemyShipObject == CockPitManager.Instance.selectedEnemyShip)
            CockPitManager.Instance.enemyHealthBarFill.GetComponent<Scrollbar>().size = NpcDamageRatio;
    
    }


    /// <summary>
    /// This will stop the battle visually between the player and the enemy.
    /// </summary>
    public void DisEngage()
    {
        for (var i = 0; i < CockPitManager.Instance.enemyShipsParent.transform.childCount; i++)
        {
            if (CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().isEngaged)
            {
                CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetChild(0).gameObject.SetActive(false);
                CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetChild(1).gameObject.SetActive(false);
                CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetChild(2).gameObject.SetActive(false);
                CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetChild(1).GetChild(0).GetComponent<Image>().color = new(0.4f,0.8f,0.4f,1);
                CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<Image>().sprite =
                    CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<Enemy>().frontSprite;
            }
        }

        if (CockPitManager.Instance.selectedEnemyShip!=null && CockPitManager.Instance.selectedEnemyShip.GetComponent<Enemy>().attackingPlayer == false)
        {
            CockPitManager.Instance.selectedEnemyShip.GetComponent<BattleManager>().isEngaged = false;
            CockPitManager.Instance.selectedEnemyShip.GetComponent<Enemy>().EngageToPlayer();
        }

        CockPitManager.Instance.playerBattleManager.GetComponent<BattleManager>().isEngaged = false;
        CockPitManager.Instance.fireLaser.SetActive(false);

        StopEngageCoroutine();

        CockPitManager.Instance.engageButton.SetActive(true);
        CockPitManager.Instance.disengageButton.SetActive(false);

        CockPitManager.Instance.enemyShipImage.sprite = null;
        CockPitManager.Instance.enemyShipImage.color = new(1, 1, 1, 0);
        CockPitManager.Instance.enemyHealthBarFill.transform.parent.gameObject.SetActive(false);
        CockPitManager.Instance.enemyShipName.text = null;
        CockPitManager.Instance.enemyShipClassName.text = null;
    }


    /// <summary>
    /// This is a couroutine that will stop the one where we are attacking an enemy ship.
    /// </summary>
    public void StopEngageCoroutine()
    {
        if (battleCoroutine != null)
        {
            StopCoroutine(battleCoroutine);
            battleCoroutine = null;
        } 
    }


    /// <summary>
    /// To calculate the damage taken.
    /// </summary>
    public float CalculateDamageTaken(float accuracy, float evasion, float gunDamage, float shieldValue, float shieldSlider)
    {
        float damageTaken = 0f;
        // float damageAfterShield = 0f;
        float randAccuracy = UnityEngine.Random.Range(1, 101);
        float randEvasion = UnityEngine.Random.Range(1, 101);

        if (randAccuracy <= accuracy)
        {
            if (!(randEvasion <= evasion))
                {
                    damageTaken = (gunDamage - (gunDamage * (0.7f * shieldValue * shieldSlider / 100))) * 6;
                }
            }
        return damageTaken;
    }


    /// <summary>
    /// 
    /// </summary>
    public void SetLootlockerShipHP(int slot, float health)
    {
        // Ship Slot Health Leaderboards. Use for storing ships health, each one corresponds to a slot.
        List<string> lootlockerStoreNames = new List<string> {"17059", "17061", "17062", "17063"};

        if(health >= 0.01f)
        {
            LootLockerSDKManager.SubmitScore("20", (int)MathF.Floor(health * 10000), lootlockerStoreNames[slot], (response) => {
                CockPitManager.Instance.data.shipHealth = Mathf.Floor(health * 10000.0f) / 10000.0f; // This is neccessary for the healthbar not to jump to another value when going back to home planet
            });
        }

        if(health == 99999){
            CockPitManager.Instance.processesToFinish++;
        }
    }

    /// <summary>
    /// This function will update owned ships file in LootLocker
    /// </summary>
    public void UpdatePlayerMissions(int mission_id)
    {
        int fileId = 0;
        string fileUrl = "";

        // Get owned ships file
        LootLockerSDKManager.GetAllPlayerFiles((response) =>
        {
            if (response.success)
            {
                for (int i = 0; i < response.items.Length; i++)
                {
                    if (response.items[i].purpose == "MISSIONS")
                    {
                        fileId = response.items[i].id;
                        fileUrl = response.items[i].url;
                        break;
                    }
                }
                
                // Get session file
                StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
                {
                    MissionsData storedData = JsonUtility.FromJson<MissionsData>(jsonString);

                    foreach(Mission mission in storedData.missions)
                    {
                        if(mission.id == mission_id)
                            storedData.missions[mission_id - 1].completed = true;
                    }

                    string updatedData = JsonUtility.ToJson(storedData);

                    // Create a temporary file path
                    string filePath = Path.Combine(Application.persistentDataPath, "missions.json");

                    // Write the JSON string to the temporary file
                    File.WriteAllText(filePath, updatedData);
                    var file = File.Open(filePath, FileMode.Open);

                    LootLockerSDKManager.UpdatePlayerFile(fileId, file, response =>
                    {
                        if (response.success)
                        {
                            Debug.Log("Successfully updated player file, url: " + response.url);
                            // Delete the temporary file
                            file.Close();
                            File.Delete(filePath);
                        } 
                        else
                        {
                            Debug.Log("Error updating player file");
                        }
                    });

                }));
            }
            else
            {
                Debug.Log("Error retrieving player storage");
            }
        });
    }
    

    public void RemoveFromOwnedShipsFile(int slot)
    {
        int fileId = 0;
        string fileUrl = "";

        // Get owned ships file
        LootLockerSDKManager.GetAllPlayerFiles((response) =>
        {
            if (response.success)
            {
                for (int i = 0; i < response.items.Length; i++)
                {
                    if (response.items[i].purpose == "OWNED_SHIPS")
                    {
                        fileId = response.items[i].id;
                        fileUrl = response.items[i].url;
                        break;
                    }
                }
                
                // Get session file
                StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
                {
                    ShipObjectWrapper storedData = JsonUtility.FromJson<ShipObjectWrapper>(jsonString);

                    List<string> pods = new();
                    List<string> guns = new();
                    // Updates the specific slot where the ship was bought with the new ship.
                    ShipObject ship = new ShipObject
                    {
                        shipId = "",
                        pods = pods,
                        guns = guns,
                        selected = false
                    };

                    storedData.ships[slot] = ship;
                    storedData.isDead = true;
                    
                    string updatedData = JsonUtility.ToJson(storedData);

                    // Create a temporary file path
                    string filePath = Path.Combine(Application.persistentDataPath, "owned_ships.json");

                    // Write the JSON string to the temporary file
                    File.WriteAllText(filePath, updatedData);
                    var file = File.Open(filePath, FileMode.Open);

                    LootLockerSDKManager.UpdatePlayerFile(fileId, file, response =>
                    {
                        if (response.success)
                        {
                            Debug.Log("Successfully updated player file, url: " + response.url);
                            // Delete the temporary file
                            file.Close();
                            File.Delete(filePath);
                            CockPitManager.Instance.processesToFinish++;
                        } 
                        else
                        {
                            Debug.Log("Error updating player file");
                        }
                    });
                }));
            }
            else
            {
                Debug.Log("Error retrieving player storage");
            }
        });
    }

    /// <summary>
    /// This function updates the selected pilot in LootLocker.
    /// </summary>
    public void UpdatePlayerSoundVolume()
    {
        string fileUrl = "";
        int fileId = 0;

        // Get player session file
        LootLockerSDKManager.GetAllPlayerFiles((response) =>
        {
            if (response.success)
            {
                for (int i = 0; i < response.items.Length; i++)
                {
                    if (response.items[i].purpose == "PLAYER_SESSION")
                    {
                        fileUrl = response.items[i].url;
                        fileId = response.items[i].id;
                        break;
                    }
                }
                
                // Get session file
                StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
                {
                    PlayerSessionData storedData = JsonUtility.FromJson<PlayerSessionData>(jsonString);

                    // Replaces the stored background and sfx volumes
                    storedData.settings.backgroundVolume = MusicPlayerManager.Instance.volumeController.value;

                    string updatedData = JsonUtility.ToJson(storedData);

                    // Create a temporary file path
                    string filePath = Path.Combine(Application.persistentDataPath, "player_session.json");

                    // Write the JSON string to the temporary file
                    File.WriteAllText(filePath, updatedData);

                    var file = File.Open(filePath, FileMode.Open);

                    LootLockerSDKManager.UpdatePlayerFile(fileId, file, response =>
                    {
                        if (response.success)
                        {
                            Debug.Log("Successfully updated player file, url: " + response.url);
                            CockPitManager.Instance.data.volumeValue = MusicPlayerManager.Instance.volumeController.value;
                            // Delete the temporary file
                            file.Close();
                            File.Delete(filePath);
                        } 
                        else
                        {
                            Debug.Log("Error updating player file");
                        }
                    });

                }));
            }
            else
            {
                Debug.Log("Error retrieving player storage");
            }
        });
    }

    /// <summary>
    /// This function returns the json text from a specific url that LootLocker provides
    /// </summary>
    private IEnumerator GetJSONFromURL(string url, Action<string> callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to retrieve JSON from URL: " + webRequest.error);
                callback(null);
            }

            string json = webRequest.downloadHandler.text;
            callback(json);
        }
    }
}