using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using TMPro;
using System;

public class MapViewManager : MonoBehaviour
{
    public static MapViewManager Instance;

    [Header("SO")]
    public Data data;
    public NodeData[] allNodes;

    [Header("Sprites")]
    public Sprite undiscoveredHoverNode;
    public Sprite undiscoveredDefaultNode;
    public Sprite discoveredHoverNode;
    public Sprite discoveredDefaultNode;
    public Sprite hoverPlanet1;
    public Sprite defaultPlanet1;
    public Sprite hoverPlanet2;
    public Sprite defaultPlanet2;
    public Sprite hoverPlanet3;
    public Sprite defaultPlanet3;
    public Sprite actualNode;
    public Sprite bluePath;
    public Sprite blueThinPath;
    public Sprite magentaPath;
    public Sprite magentaThinPath;

    [Header("Game Objects")]
    public GameObject paths;
    public GameObject nodes;
    public GameObject equipedShip;
    public GameObject maximizeButton;
    public GameObject minimizeButton;
    public GameObject notifications;

    [SerializeField] private GameObject mapView;

    [Header("Others")]
    public VisitedPlanetsManager visitedPlanetsManager;
    public ShipManager shipManager;

    private List<string> resizedPaths = new();
    private NpcNodeData npcNodeData;

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        foreach(NodeData node in allNodes)
            node.discovered = false;
    }

    void Update()
    {
        if(data.whichScene == "Multiplayer" && CockPitMultiplayerManager.Instance != null)
            if(CockPitMultiplayerManager.Instance.playerHP.fillAmount != 0)
            {
                nodes.transform.Find(data.actualNode).GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = CockPitMultiplayerManager.Instance.playerHP.fillAmount;
            } else
                nodes.transform.Find(data.actualNode).GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = data.shipHealth;
        else if (data.whichScene != "Multiplayer" && CockPitManager.Instance != null)
            nodes.transform.Find(data.actualNode).GetChild(0).GetChild(0).GetChild(1).GetComponent<Image>().fillAmount = CockPitManager.Instance.playerHP.fillAmount;
    }

    public void RenderMapView()
    {
        data.discoveredNodes.ForEach(discoveredNode => {
            int visitedPlanetIndex = System.Array.FindIndex(allNodes, nodeData => nodeData.name == discoveredNode);
            allNodes[visitedPlanetIndex].discovered = true;
        });

        // Set selected ship
        nodes.transform.Find(data.actualNode).GetChild(0).GetComponent<Image>().sprite = data.shipData.shipAvatar;
        nodes.transform.Find(data.actualNode).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 1);
        nodes.transform.Find(data.actualNode).GetChild(0).GetChild(0).gameObject.SetActive(true);

        // Index of actual node
        int nodeIndex = Array.FindIndex(allNodes, nodeData => nodeData.name == data.actualNode);

        // Activate plan notification if player still havent purchase it.
        if(data.shipPlans.Contains("plan1"))
            notifications.transform.GetChild(0).gameObject.SetActive(false);
        if(data.shipPlans.Contains("plan2") || !data.shipPlans.Contains("plan1"))
            notifications.transform.GetChild(1).gameObject.SetActive(false);

        // For each node, set its state variables
        foreach(NodeData node in allNodes)
        {
            Image nodeImage = nodes.transform.Find(node.name)?.GetComponent<Image>();

            if (node.name == "A1")
                nodeImage.sprite = defaultPlanet1;
            else if (node.name == "A5" && node.discovered)
            {
                nodeImage.sprite = defaultPlanet2;
                nodes.transform.Find(node.name).GetChild(1).GetComponent<TextMeshProUGUI>().text = "A5 - Ebisu";
                if(!data.shipPlans.Contains("plan1"))
                    notifications.transform.GetChild(2).gameObject.SetActive(true);
            }
            else if (node.name == "A9" && node.discovered)
            {
                nodeImage.sprite = defaultPlanet3;
                nodes.transform.Find(node.name).GetChild(1).GetComponent<TextMeshProUGUI>().text = "A9 - Enki";
                if(!data.shipPlans.Contains("plan2"))
                    notifications.transform.GetChild(3).gameObject.SetActive(true);
            }
            else if (node.name == data.actualNode)
            {
                nodeImage.sprite = actualNode;
                nodes.transform.Find(node.name).GetChild(1).GetComponent<TextMeshProUGUI>().text = "";
            }
            else if (node.discovered && node.name != "A1" && node.name != "A5" && node.name != "A9")
            {
                nodeImage.sprite = discoveredDefaultNode;
                nodes.transform.Find(node.name).GetChild(1).GetComponent<TextMeshProUGUI>().text = node.name;
            }
            else if (node.name != "A1" && node.name != "A5" && node.name != "A9")
                nodeImage.sprite = undiscoveredDefaultNode;
            
            node.adjacentNodes.ForEach(adjacentNode =>
                {
                    Transform child1 = paths.transform.Find(adjacentNode + node.name);
                    Transform child2 = paths.transform.Find(node.name + adjacentNode);
                    if(data.discoveredNodes.Contains(adjacentNode) && node.discovered)
                    {
                        if(child1)
                        {
                            child1.GetComponent<Image>().color = new(1, 1, 1, 0.2f);
                            child1.GetComponent<Image>().sprite = blueThinPath;
                         }
                        else if(child2)
                        {
                            child2.GetComponent<Image>().color = new(1, 1, 1, 0.2f);
                            child2.GetComponent<Image>().sprite = blueThinPath;
                        }
                    }

                    // Resets b path of all nodes
                    if(data.discoveredNodes.Contains(adjacentNode))
                    {
                        if(child1)
                        {
                            paths.transform.Find(adjacentNode + node.name + "b").GetComponent<Image>().color = new(1, 1, 1, 0);
                        }
                        else if(child2)
                        {
                            paths.transform.Find(node.name + adjacentNode + "b").GetComponent<Image>().color = new(1, 1, 1, 0);
                        }
                    }
                }
            );
            // Display Big Blue Line on actualNode's adjacent nodes.
            allNodes[nodeIndex].adjacentNodes.ForEach(adjacentNode =>
            {
                int nodeIndex2 = Array.FindIndex(allNodes, nodeData => nodeData.name == adjacentNode);

                // if(allNodes[nodeIndex2].discovered)
                // {
                    Transform child1 = paths.transform.Find(adjacentNode + data.actualNode + "b");
                    Transform child2 = paths.transform.Find(data.actualNode + adjacentNode + "b");

                    if(child1)
                    {
                        child1.GetComponent<Image>().color = new(1, 1, 1, 1);
                        child1.GetComponent<Image>().sprite = bluePath;

                        if (!resizedPaths.Contains(child1.name) && allNodes[nodeIndex2].discovered)
                        {
                            Vector2 sizeDelta = child1.GetComponent<RectTransform>().sizeDelta;
                            sizeDelta.x = child1.GetComponent<RectTransform>().sizeDelta.x - 14;
                            child1.GetComponent<RectTransform>().sizeDelta = sizeDelta;
                            resizedPaths.Add(child1.name);
                        }
                    }
                    else if(child2)
                    {
                        child2.GetComponent<Image>().color = new(1, 1, 1, 1);
                        child2.GetComponent<Image>().sprite = bluePath;

                        if (!resizedPaths.Contains(child2.name) && allNodes[nodeIndex2].discovered)
                        {
                            Vector2 sizeDelta = child2.GetComponent<RectTransform>().sizeDelta;
                            sizeDelta.x = child2.GetComponent<RectTransform>().sizeDelta.x - 14;
                            child2.GetComponent<RectTransform>().sizeDelta = sizeDelta;
                            resizedPaths.Add(child2.name);
                        }
                    }
                }
            // }
            );
        }
    }

    public void NodeHoverEnter(string nodeName)
    {
        MusicPlayerManager.Instance.PlayMusic("Button Hover");
        int nodeIndex = System.Array.FindIndex(allNodes, nodeData => nodeData.name == nodeName);
        Image nodeImage = nodes.transform.Find(nodeName).GetComponent<Image>();

        foreach(NodeData node in allNodes)
        {
            if(node.name == data.actualNode && node.adjacentNodes.Contains(nodeName))
            {
                Transform child1 = paths.transform.Find(data.actualNode + nodeName + "b");
                Transform child2 = paths.transform.Find(nodeName + data.actualNode + "b");

                if(child1)
                {
                    child1.GetComponent<Image>().color = new(1, 1, 1, 1);
                    child1.GetComponent<Image>().sprite = magentaPath;
                } 
                else if(child2)
                {
                    child2.GetComponent<Image>().color = new(1, 1, 1, 1);
                    child2.GetComponent<Image>().sprite = magentaPath;
                }

                if(nodeName == "A1")
                    nodeImage.sprite = hoverPlanet1;
                else if(nodeName == "A5" && allNodes[nodeIndex].discovered)
                    nodeImage.sprite = hoverPlanet2;
                else if(nodeName == "A9" && allNodes[nodeIndex].discovered)
                    nodeImage.sprite = hoverPlanet3;
                else if (allNodes[nodeIndex].discovered && allNodes[nodeIndex].name != "A1" && allNodes[nodeIndex].name != "A5" && allNodes[nodeIndex].name != "A9")
                    nodeImage.sprite = discoveredHoverNode;
                else if (allNodes[nodeIndex].name != "A1" && allNodes[nodeIndex].name != "A5" && allNodes[nodeIndex].name != "A9")
                    nodeImage.sprite = undiscoveredHoverNode;

                break;
            }
        };
    }

    public void NodeHoverExit(string nodeName)
    {
        Image nodeImage = nodes.transform.Find(nodeName).GetComponent<Image>();
        int nodeIndex = System.Array.FindIndex(allNodes, nodeData => nodeData.name == nodeName);
        int actualNodeIndex = System.Array.FindIndex(allNodes, nodeData => nodeData.name == data.actualNode);
        
        Transform child1 = paths.transform.Find(data.actualNode + nodeName + "b");
        Transform child2 = paths.transform.Find(nodeName + data.actualNode + "b");

        if(child1)
        {
            if(allNodes[actualNodeIndex].adjacentNodes.Contains(nodeName))
            {
                child1.GetComponent<Image>().color = new(1, 1, 1, 1);
                child1.GetComponent<Image>().sprite = bluePath;
            }
            else
            {
                child1.GetComponent<Image>().color = new(1, 1, 1, 0);
                child1.GetComponent<Image>().sprite = bluePath;
            }

        } 
        else if(child2)
        {
            if(allNodes[actualNodeIndex].adjacentNodes.Contains(nodeName))
            {
                child2.GetComponent<Image>().color = new(1, 1, 1, 1);
                child2.GetComponent<Image>().sprite = bluePath;
            }
            else
            {
                child2.GetComponent<Image>().color = new(1, 1, 1, 0);
                child2.GetComponent<Image>().sprite = bluePath;
            }
        }

        if(nodeName == "A1")
            nodeImage.sprite = defaultPlanet1;
        else if(nodeName == "A5" && allNodes[nodeIndex].discovered)
            nodeImage.sprite = defaultPlanet2;
        else if(nodeName == "A9" && allNodes[nodeIndex].discovered)
            nodeImage.sprite = defaultPlanet3;
        else if (nodeName == data.actualNode)
            nodeImage.sprite = actualNode;
        else if(allNodes[nodeIndex].discovered && allNodes[nodeIndex].name != "A1" && allNodes[nodeIndex].name != "A5" && allNodes[nodeIndex].name != "A9")
            nodeImage.sprite = discoveredDefaultNode;
        else if (allNodes[nodeIndex].name != "A1" && allNodes[nodeIndex].name != "A5" && allNodes[nodeIndex].name != "A9")
            nodeImage.sprite = undiscoveredDefaultNode;
    }

    public void CockPitView(string nodeName)
    {
        MusicPlayerManager.Instance.PlayMusic("Node Clicked");
        int nodeIndex = Array.FindIndex(allNodes, nodeData => nodeData.name == nodeName);
        int lastNodeIndex = Array.FindIndex(allNodes, nodeData => nodeData.name == data.actualNode);

        data.shipFunNames.Clear();

        if(nodeName == data.actualNode)
        {
            mapView.SetActive(false);
            return;
        }

        // If node A13 is cliked, change scene to Multiplayer
        if(nodeName == "A13")
        {
            foreach(NodeData node in allNodes)
            {
                if(node.name == data.actualNode && node.adjacentNodes.Contains(nodeName))
                {
                    // data.actualNode = nodeName;
                    data.whichScene = "Multiplayer";
                    data.previousScene = "GamePlay";
                    SceneManager.LoadScene(2);
                    return;
                }
            }
        }

        // If player is exiting A13, change scene to Gameplay
        if(data.actualNode == "A13" || data.whichScene == "Multiplayer")
        {
            foreach(NodeData node in allNodes)
            {
                if(node.name == data.actualNode && node.adjacentNodes.Contains(nodeName))
                {
                    data.nodeNpcShips.Clear();

                    foreach(GameObject npcShip in allNodes[nodeIndex].npcShips)
                    {
                        for(int i = 0; i < allNodes[nodeIndex].totalNpcsPerShipClass; i++)
                        {
                            foreach(ShipData ship in shipManager.allShipsData)
                            {
                                if(ship.name == npcShip.name)
                                {
                                    data.shipFunNames.Add(ship.classShipNames[i]);
                                }

                            }
                            // int randomIndex = UnityEngine.Random.Range(0, allNodes[nodeIndex].npcShips.Count);
                            data.nodeNpcShips.Add(npcShip);
                            
                        }
                    }

                    if(data.actualNode == "A13"){
                        CockPitMultiplayerManager.Instance.DisconnectGame();
                    }

                    data.actualNode = nodeName;
                    SceneManager.LoadScene(1);
                    return;
                }
            }
        }
        else
        {
            foreach(NodeData node in allNodes)
            {
                if(node.name == data.actualNode && node.adjacentNodes.Contains(nodeName))
                {
                    visitedPlanetsManager.UpdateVisitedPlanets(nodeName);
                    
                    if(!allNodes[nodeIndex].discovered)
                    {
                        data.discoveredNodes.Add(nodeName);
                        allNodes[nodeIndex].discovered = true;
                    }

                    // Remove image from previous node
                    nodes.transform.Find(data.actualNode).GetChild(0).GetComponent<Image>().sprite = null;
                    nodes.transform.Find(data.actualNode).GetChild(0).GetComponent<Image>().color = new(1, 1, 1, 0);
                    nodes.transform.Find(data.actualNode).GetChild(0).GetChild(0).gameObject.SetActive(false);

                    data.actualNode = nodeName;
                    data.nodeNpcShips.Clear();

                    foreach(GameObject npcShip in allNodes[nodeIndex].npcShips)
                    {
                        for(int i = 0; i < allNodes[nodeIndex].totalNpcsPerShipClass; i++)
                        {
                            foreach(ShipData ship in shipManager.allShipsData)
                            {
                                print("-------- ship.name == npcShip.name / " + ship.name + " - " +npcShip.name );
                                if(ship.name == npcShip.name)
                                {
                                    data.shipFunNames.Add(ship.classShipNames[i]);
                                }

                            }
                            // int randomIndex = UnityEngine.Random.Range(0, allNodes[nodeIndex].npcShips.Count);
                            data.nodeNpcShips.Add(npcShip);
                            
                        }
                    }

                    // for(int i = 0; i < allNodes[nodeIndex].totalNodeShips; i++)
                    // {
                    //     int randomIndex = UnityEngine.Random.Range(0, allNodes[nodeIndex].npcShips.Count);
                    //     data.nodeNpcShips.Add(allNodes[nodeIndex].npcShips[randomIndex]);
                    // }

                    RemoveObjectByName(allNodes[lastNodeIndex].name);
                    allNodes[lastNodeIndex].currentShips.Clear();

                    for (int i = 0; i < CockPitManager.Instance.enemyShipsParent.transform.childCount; i++)
                    {
                        // Stores in All Datta all current ships in the node the player is leaving.
                        npcNodeData = new NpcNodeData
                        {
                            ship = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().playerShip,
                            pilot = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().playerPilot,
                            shipCombatStats = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().playerStats,
                            spawnPlace = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().spawnPlace,
                            shipName = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).name.Replace("(Clone)", ""),
                            shipFunName = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<Enemy>().enemyFunName
                        };
                        allNodes[lastNodeIndex].currentShips.Add(npcNodeData);
                    }
                    for (int i = 0; i < CockPitManager.Instance.enemyShipsParent.transform.childCount; i++)
                        Destroy(CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).gameObject);
                    
                    if(DateTime.Now > allNodes[lastNodeIndex].resetTime)
                    {
                        int randomSeconds = UnityEngine.Random.Range(60, 300); // 1 to 5 minutes from Now
                        allNodes[lastNodeIndex].resetTime = DateTime.Now.AddSeconds(randomSeconds);
                    }

                    data.nodesData.Add(allNodes[lastNodeIndex]);
                    npcNodeData = null;
                    CockPitManager.Instance.RefreshScene();

                    mapView.SetActive(false);
                    // MusicPlayerManager.Instance.PlayRandomTrack();
                    break;
                }
            }
        }
    }

    private void RemoveObjectByName(string objectName)
    {
        // Check if an object with the given name exists in the list
        NodeData objectToRemove = data.nodesData.Find(obj => obj.name == objectName);

        if (objectToRemove != null)
        {
            // Remove the object from the list
            data.nodesData.Remove(objectToRemove);
        }
    }

    public void UpdateNpcNodes()
    {
        int lastNodeIndex = Array.FindIndex(allNodes, nodeData => nodeData.name == data.actualNode);

        RemoveObjectByName(allNodes[lastNodeIndex].name);
        allNodes[lastNodeIndex].currentShips.Clear();
        
        for (int i = 0; i < CockPitManager.Instance.enemyShipsParent.transform.childCount; i++)
        {
            // Stores in All Datta all current ships in the node the player is leaving.
            npcNodeData = new NpcNodeData
            {
                ship = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().playerShip,
                pilot = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().playerPilot,
                shipCombatStats = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().playerStats,
                spawnPlace = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<BattleManager>().spawnPlace,
                shipName = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).name.Replace("(Clone)", ""),
                shipFunName = CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).GetComponent<Enemy>().enemyFunName
            };
            allNodes[lastNodeIndex].currentShips.Add(npcNodeData);
        }
        for (int i = 0; i < CockPitManager.Instance.enemyShipsParent.transform.childCount; i++)
            Destroy(CockPitManager.Instance.enemyShipsParent.transform.GetChild(i).gameObject);
        
        if(DateTime.Now > allNodes[lastNodeIndex].resetTime)
        {
            int randomSeconds = UnityEngine.Random.Range(60, 300); // 1 to 5 minutes from Now
            allNodes[lastNodeIndex].resetTime = DateTime.Now.AddSeconds(randomSeconds);
        }

        data.nodesData.Add(allNodes[lastNodeIndex]);
        npcNodeData = null;
    }

    public void TriggerFullscreen()
    {
        MusicPlayerManager.Instance.PlayMusic("Button Click");
        if(Screen.fullScreen)
        {
            Screen.fullScreen = false;
            maximizeButton.SetActive(true);
            minimizeButton.SetActive(false);
        }
        else
        {
            Screen.fullScreen = true;
            maximizeButton.SetActive(false);
            minimizeButton.SetActive(true);
        }
    }

}
