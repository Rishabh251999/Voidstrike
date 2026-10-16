using UnityEngine;
using System.IO;
using System.Text;
using LootLocker.Requests;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;
using UnityEngine.UI;
using TMPro;
using System.Globalization;

[Serializable]
public class PlayerSessionData
{
    public string pilot;
    public SettingsData settings;
}

[Serializable]
public class SettingsData
{
    public float backgroundVolume;
    public float sfxVolume;
}

[Serializable]
public class ShipObject
{
    public string shipId;
    public List<string> pods;
    public List<string> guns;
    public bool selected;
}

[Serializable]
public class ShipObjectWrapper : IEnumerable<ShipObject>
{
    public ShipObject[] ships;
    public bool isDead;
    // Implementation of the IEnumerable interface
    public IEnumerator<ShipObject> GetEnumerator()
    {
        return ((IEnumerable<ShipObject>)ships).GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return ships.GetEnumerator();
    }
}

public class PlayerSessionManager : MonoBehaviour
{

    [Header("Managers")]
    public PilotManager pilotManager;
    public ShipManager shipManager;

    [Header("Images")]
    [SerializeField] private Image pilotDisplayArea;
    public Image shipDisplayArea;

    [Header("Sprites")]
    [SerializeField] private Sprite planet1;
    [SerializeField] private Sprite planet2;
    [SerializeField] private Sprite planet3;
    [SerializeField] private Sprite planet1Extended;
    [SerializeField] private Sprite planet2Extended;
    [SerializeField] private Sprite planet3Extended;
    [SerializeField] private Sprite shipPlansClass2ShipImage;
    [SerializeField] private Sprite shipPlansClass3ShipImage;
    [SerializeField] private Sprite shipFrame;

    [Header("Game Objects")]
    [SerializeField] private GameObject background;
    [SerializeField] private GameObject backgroundExtended;
    [SerializeField] private GameObject menuPilotButton;
    [SerializeField] private GameObject menuHangarButton;
    public GameObject planetPlansPanel;
    public GameObject quickRepair;
    public GameObject shipDestroyed;

    [Header("InputField")]
    [SerializeField] private TMP_InputField playerName;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI planetName;

    [Header("SO")]
    public Data allData;
    public NodeData[] allNodes;
    private bool isPlayerDead = false;
    public VisitedPlanetsManager visitedPlanetsManager;

    public void UpdateLoginTimestamp()
    {
        Leaderboard.Instance.SetLeaderboardLastLogin();
    }

    public void CreateInitialPlayerFiles(Action callback)
    {
        int operationsToComplete = 4; // Number of asynchronous operations to complete
        int operationsCompleted = 0;  // Number of completed asynchronous operations

        // Define a helper method to check if all operations are completed and invoke the callback
        void CheckCompletion()
        {
            operationsCompleted++;
            if (operationsCompleted == operationsToComplete)
            {
                // All asynchronous operations are completed, invoke the callback
                callback?.Invoke();
            }
        }

        // Owned Ships File
        ShipObjectWrapper shipWrapper = new ShipObjectWrapper();
        shipWrapper.ships = new ShipObject[4];
        string shipId = "";
        List<string> pods = new List<string>();
        List<string> guns = new List<string>();
        bool selected = false;
        for (int i = 0; i < 4; i++)
        {
            ShipObject ship = new ShipObject
            {
                shipId = shipId,
                pods = pods,
                guns = guns,
                selected = selected
            };

            shipWrapper.ships[i] = ship;
        }
        string jsonOwnedShips = JsonUtility.ToJson(shipWrapper);

        // Player Session file
        PlayerSessionData dataPlayerSession = new()
        {
            pilot = "",
            settings = new SettingsData()
        };
        dataPlayerSession.settings.backgroundVolume = 0.3f;
        dataPlayerSession.settings.sfxVolume = 0.3f;
        // Convert PlayerSessionData to JSON string
        string jsonPlayerSession = JsonUtility.ToJson(dataPlayerSession);

        // Visited Planets File
        string initial_planet = "A1";
        // Create a new instance of PlanetData
        PlanetsData dataVisitedPlanets = new()
        {
            // Add planets to the array
            visited_planets = new List<string> { initial_planet },
            actual_node = initial_planet,
            ship_plans = new List<string>()
        };
        // Convert PlanetData to JSON string
        string jsonVisitedPlanets = JsonUtility.ToJson(dataVisitedPlanets);
        
        // Missions File
        // Create a new instance of MissionsData
        MissionsData missionsData = new();
        // Add missions
        missionsData.missions.Add(new Mission
            {
                id = 1,
                description = "Buy ship plans at Ebisu",
                reward = 0,
                scene = "MainMenu",
                action = "Buy",
                target = new TargetData { type = "plan", name = "A5" },
                enabled = true,
                completed = false
            }
        );

        missionsData.missions.Add(new Mission
            {
                id = 2,
                description = "Buy ship plans at Enki",
                reward = 0,
                scene = "MainMenu",
                action = "Buy",
                target = new TargetData { type = "plan", name = "A9" },
                enabled = false,
                completed = false
            }
        );

        missionsData.missions.Add(new Mission
            {
                id = 3,
                description = "Kill a Class 1 Scout ship\nto get 20K M                <color=#00000000>..</color>",
                reward = 20000,
                scene = "CockPit View",
                action = "Kill",
                target = new TargetData { type = "npc", name = "Class 1 Scout" },
                enabled = true,
                completed = false
            }
        );
        // Convert MissionsData to JSON string
        string jsonMissions = JsonUtility.ToJson(missionsData);

        UploadPlayerFile("owned_ships", jsonOwnedShips, () => CheckCompletion());
        UploadPlayerFile("player_session", jsonPlayerSession, () => CheckCompletion());
        UploadPlayerFile("visited_planets", jsonVisitedPlanets, () => CheckCompletion());
        UploadPlayerFile("missions", jsonMissions, () => CheckCompletion());
    }

    private void UploadPlayerFile(string file_name, string json, Action onComplete)
    {
        bool isPublic = true;
        string filePath = Path.Combine(Application.persistentDataPath, file_name + ".json");
        File.WriteAllText(filePath, json);
        var file = File.Open(filePath, FileMode.Open);
        LootLockerSDKManager.UploadPlayerFile(file, file_name, isPublic, response =>
        {
            if (response.success)
            {
                Debug.Log("Successfully uploaded player file, url: " + response.url);
            }
            else
            {
                Debug.Log("Error uploading player file");
            }
            file.Close();
            File.Delete(filePath);
            
            // Notify that the upload is done
            onComplete?.Invoke();
        });
    }

    // This is used for refreshing the Main Menu scene depending on the player actual planet
    public void SetMainMenu()
    {
        SetHomePlanetProps(allData.actualNode);

        if(allData.shipData != null)
        {
            shipDisplayArea.sprite = allData.shipData.shipAvatar;
            shipDisplayArea.GetComponent<Image>().enabled = true;
            quickRepair.transform.Find("HealthBar").GetChild(1).GetComponent<Image>().fillAmount = allData.shipHealth;
            quickRepair.SetActive(true);
        }
        else if(allData.previousScene == "GamePlay")
        {
            shipDestroyed.GetComponent<Image>().enabled = true;
        }

        pilotDisplayArea.sprite = allData.pilotData.pilotAvatar;
        GameManager.Instance.currencyText.text = allData.currency.ToString("#,#", CultureInfo.InvariantCulture);
        GameManager.Instance.currency = allData.currency;
        pilotDisplayArea.GetComponent<Image>().enabled = true;

    }

    /// <summary>
    /// This function returns the PlayerSessionData object
    /// TODO: Make 1 function out of these 3 that are very similar.
    /// </summary>
    public void GetPlayerSession(Action<PlayerSessionData> callback)
    {
        string fileUrl = "";
        int fileId = 0;

        // Get the player session file from lootlocker
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
            StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
            {
                PlayerSessionData data = JsonUtility.FromJson<PlayerSessionData>(jsonString);
                callback(data);
            }));
        }
        else
        {
            Debug.Log("Error retrieving player storage");
        }
        });
    }

    /// <summary>
    /// This function returns the Ships and their pods and guns inside a ShipObjectWrapper object, which is an array of ShipObject.
    /// </summary>
    public void GetPlayerOwnedShips(Action<ShipObjectWrapper> callback)
    {
        string fileUrl = "";
        int fileId = 0;

        LootLockerSDKManager.GetAllPlayerFiles((response) =>
        {
        if (response.success)
        {
            for (int i = 0; i < response.items.Length; i++)
            {
                if (response.items[i].purpose == "OWNED_SHIPS")
                {
                    fileUrl = response.items[i].url;
                    fileId = response.items[i].id;
                    break;
                }
            }
            StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
            {
                ShipObjectWrapper data = JsonUtility.FromJson<ShipObjectWrapper>(jsonString);
                callback(data);
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
    public void UpdatePlayerSelectedPilot(string pilot)
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

                    // Replaces the stored pilot for the actual pilot
                    storedData.pilot = pilot;

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
    /// This function updates the plan.
    /// </summary>
    public void UpdatePlayerShipPlans(string plan)
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
                    if (response.items[i].purpose == "VISITED_PLANETS")
                    {
                        fileUrl = response.items[i].url;
                        fileId = response.items[i].id;
                        break;
                    }
                }
                
                // Get session file
                StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
                {
                    PlanetsData storedData = JsonUtility.FromJson<PlanetsData>(jsonString);

                    // Replaces the stored pilot for the actual pilot
                    storedData.ship_plans.Add(plan);

                    string updatedData = JsonUtility.ToJson(storedData);

                    // Create a temporary file path
                    string filePath = Path.Combine(Application.persistentDataPath, "visited_planets.json");

                    // Write the JSON string to the temporary file
                    File.WriteAllText(filePath, updatedData);

                    var file = File.Open(filePath, FileMode.Open);

                    LootLockerSDKManager.UpdatePlayerFile(fileId, file, response =>
                    {
                        if (response.success)
                        {
                            allData.shipPlans.Add(plan);
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
    /// This function updates the selected ship, pods and guns in LootLocker.
    /// </summary>
    public void UpdatePlayerSelectedShipPodsGuns(int slot)
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
                    if (response.items[i].purpose == "OWNED_SHIPS")
                    {
                        fileUrl = response.items[i].url;
                        fileId = response.items[i].id;
                        break;
                    }
                }
                
                // Get session file
                StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
                {
                    ShipObjectWrapper storedData = JsonUtility.FromJson<ShipObjectWrapper>(jsonString);

                    // Reset all selected to false
                    for(int x = 0;x < storedData.ships.Length;x++)
                        storedData.ships[x].selected = false;

                    // Set the selected ship as selected
                    storedData.ships[slot].selected = true;

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
    /// This function will update owned ships file in LootLocker
    /// </summary>
    public void UpdatePlayerOwnedShipsFile(int slot, string shipId, List<string> pods, List<string> guns, bool selected)
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

                    if(selected)
                    {
                        for(int x = 0;x < storedData.ships.Length;x++)
                            storedData.ships[x].selected = false;
                    }
                    // Updates the specific slot where the ship was bought with the new ship.
                    ShipObject ship = new ShipObject
                    {
                        shipId = shipId,
                        pods = pods,
                        guns = guns,
                        selected = selected
                    };

                    storedData.ships[slot] = ship;
                    storedData.isDead = false;
                    
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
                    storedData.settings.backgroundVolume = 
                    (float)allData.volumeValue;
                    // (double)AudioManager.Instance.musicVolumeBar.GetComponent<Scrollbar>().value;
                    storedData.settings.sfxVolume = 
                    (float)AudioManager.Instance.sfxVolumeBar.GetComponent<Scrollbar>().value;

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
    /// Get Player Visited Planets
    /// </summary>
    public void GetPlayerVisitedPlanets(Action<PlanetsData> callback)
    {
        string fileUrl = "";
        int fileId = 0;

        LootLockerSDKManager.GetAllPlayerFiles((response) =>
        {
        if (response.success)
        {
            Debug.Log("Successfully retrieved player files: " + response.items.Length);
            for (int i = 0; i < response.items.Length; i++)
            {
                if (response.items[i].purpose == "VISITED_PLANETS")
                {
                    fileUrl = response.items[i].url;
                    fileId = response.items[i].id;
                    break;
                }
            }
            StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
            {
                PlanetsData data = JsonUtility.FromJson<PlanetsData>(jsonString);
                callback(data);
            }));
        }
        else
        {
            Debug.Log("Error retrieving player storage");
        }
        });
    }

    /// <summary>
    /// Get Player Missions
    /// </summary>
    public void GetPlayerMissions(Action<MissionsData> callback)
    {
        string fileUrl = "";
        int fileId = 0;

        LootLockerSDKManager.GetAllPlayerFiles((response) =>
        {
        if (response.success)
        {
            Debug.Log("Successfully retrieved player files: " + response.items.Length);
            for (int i = 0; i < response.items.Length; i++)
            {
                if (response.items[i].purpose == "MISSIONS")
                {
                    fileUrl = response.items[i].url;
                    fileId = response.items[i].id;
                    break;
                }
            }
            StartCoroutine(GetJSONFromURL(fileUrl, (jsonString) =>
            {
                MissionsData data = JsonUtility.FromJson<MissionsData>(jsonString);
                callback(data);
            }));
        }
        else
        {
            Debug.Log("Error retrieving player storage");
        }
        });
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
                        {
                            if(mission.id == 1)
                                storedData.missions[1].enabled = true; // Enables Planet2 Plan Mission
                                
                            storedData.missions[mission_id - 1].completed = true;
                        }
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

    /// <summary>
    /// This function runs at login. It sets the selected pilot, ship, pods, guns. It also sets Currency and Owned Ships.
    /// </summary>
    public void StartPlayerSession(string player_id, Action callback)
    {
        int operationsToComplete = 5; // Number of asynchronous operations to complete
        int operationsCompleted = 0;  // Number of completed asynchronous operations

        allData.playerId = player_id;
        // Define a helper method to check if all operations are completed and invoke the callback
        void CheckCompletion()
        {
            operationsCompleted++;
            if (operationsCompleted == operationsToComplete)
            {
                GameManager.Instance.RefreshMissions();
                // All asynchronous operations are completed, invoke the callback
                callback?.Invoke();
            }
        }
        // Set player name
        SetPlayerName();
        // Set the amount of Player's Myth (Currency)
        Leaderboard.Instance.GetPlayerCurrency(player_id);
        Leaderboard.Instance.GetPlayerKills(player_id);
        Leaderboard.Instance.GetPlayerPvpKills(player_id);
        string selectedShip = "";
        // Set Selected Pilot and Ship
        GetPlayerSession((data) =>
        {
            foreach (PilotData pilot in pilotManager.pilotsData)
            {
                if(pilot.pilotName == data.pilot)
                {
                    pilotDisplayArea.sprite = pilot.pilotAvatar;
                    pilotDisplayArea.GetComponent<Image>().enabled = true;
                    GameManager.Instance.SetPilot(pilot);
                    allData.pilotData = pilot;
                    break;
                }
            }

            if((float)allData.volumeValue != (float)data.settings.backgroundVolume && allData.previousScene == "GamePlay")
            {
                AudioManager.Instance.backgroundAudioSource.volume = (float)allData.volumeValue;
                AudioManager.Instance.musicVolumeBar.GetComponent<Scrollbar>().value = (float)allData.volumeValue;
                UpdatePlayerSoundVolume();
            }
            else
            {
                AudioManager.Instance.backgroundAudioSource.volume = (float)data.settings.backgroundVolume;
                AudioManager.Instance.musicVolumeBar.GetComponent<Scrollbar>().value = (float)data.settings.backgroundVolume;
            }
            
            AudioManager.Instance.sfxAudioSource.volume = (float)data.settings.sfxVolume;
            AudioManager.Instance.sfxVolumeBar.GetComponent<Scrollbar>().value = (float)data.settings.sfxVolume;
            allData.sfxVolumeValue = (float)data.settings.sfxVolume;

            CheckCompletion();
        });

        int slot = 0;
        float finalPrice = 0;
        float discount = GameManager.Instance.DISCOUNT;
        int selectedSlot = -1;
        // bool finishedGettingShipsHealth = false;
        // Set Player owned ship pods and guns. Then set them in GameManager
        GetPlayerOwnedShips((ownedShips) =>
        {
            if(ownedShips.isDead)
            {
                shipDestroyed.GetComponent<Image>().enabled = true;
                isPlayerDead = true;
            }

            foreach (ShipObject ownedShip in ownedShips.ships)
            {
                // finalPrice will set the price of the ship with the discount applied
                finalPrice = 0;
                if (ownedShip.shipId != "")
                {
                    foreach(ShipData gameShip in shipManager.shipData)
                    {
                        if(ownedShip.shipId == gameShip.id)
                        {
                            if(ownedShip.selected)
                            {
                                selectedSlot = slot;
                                GameManager.Instance.shipSelectedIndex = slot;
                                selectedShip = gameShip.id;
                                shipDisplayArea.sprite = gameShip.shipAvatar;
                                shipDisplayArea.GetComponent<Image>().enabled = true;
                                shipDestroyed.GetComponent<Image>().enabled = false;
                                quickRepair.SetActive(true);
                                GameManager.Instance.selectedShip = gameShip;
                                allData.shipData = gameShip;
                            }

                            GameManager.Instance.OwnedShips.Add(gameShip);
                            finalPrice += gameShip.price * (1 - discount);
                            GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().hasShip = true;
                            GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().slotShipData.shipData = gameShip;
                            // GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().slotShipData.shipHealth = ownedShip.health;
                            
                            // Add pods to ownedShipsSlots
                            foreach(string ownedPod in ownedShip.pods)
                            {
                                foreach(PodData gamePod in shipManager.podData)
                                {
                                    if (ownedPod == gamePod.pod_Name)
                                    {
                                        finalPrice += gamePod.price * (1 - discount);
                                        GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().slotShipData.podData.Add(gamePod);
                                        // Add stats specifications to extraSpecsData
                                        for (int l = 0; l < 5; l++)
                                        {
                                            if (GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().extraSpecsData[l].
                                                skill_Type.ToString().Equals(gamePod.data.data_Type.ToString()))
                                            {
                                                GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().extraSpecsData[l].value +=
                                                gamePod.data.value;
                                                break;
                                            }
                                        }
                                        // If the ship is the selected one, set the selected pods and guns
                                        if (ownedShip.selected)
                                        {
                                            GameManager.Instance.selectedPods.Add(gamePod);
                                            allData.podDatas.Add(gamePod);
                                        }
                                    }
                                }
                            }
                            // Add weapons to ownedShipsSlots
                            foreach(string ownedWeapon in ownedShip.guns)
                            {
                                foreach(WeaponData gameWeapon in shipManager.weaponData)
                                {
                                    if (ownedWeapon == gameWeapon.weapon_Name)
                                    {
                                        finalPrice += gameWeapon.price * (1 - discount);
                                        GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().slotShipData.weaponData.Add(gameWeapon);
                                        // If the ship is the selected one, set the selected pods and guns
                                        if (ownedShip.selected)
                                        {
                                            GameManager.Instance.selectedWeapons.Add(gameWeapon);
                                            allData.weaponDatas.Add(gameWeapon);
                                        }
                                    }
                                }
                            }
                            GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().slotShipData.finalPrice = Mathf.RoundToInt(finalPrice);
                            break;
                        }
                    }
                    GameManager.Instance.ownedShipSlots[slot].GetComponent<BuyManager>().RefreshData();
                }
                slot++;
                if(slot == ownedShips.ships.Length)
                {
                    StartCoroutine(GetOwnedShipsHealth(player_id, selectedSlot, () => 
                    {
                        allData.discoveredNodes.Clear();
                        allData.shipPlans.Clear();
                        GetPlayerVisitedPlanets((data) => {

                            if(isPlayerDead)
                            {
                                data.actual_node = "A1";
                                visitedPlanetsManager.UpdateVisitedPlanets("A1");
                                allData.shipData = null;
                            }
                            // Do something with data here
                            allData.actualNode = data.actual_node;
                            allData.lastVisitedPlanet = data.actual_node;
                            allData.nodeNpcShips.Clear();
                            allData.shipFunNames.Clear();

                            foreach(NodeData node in allNodes)
                            {
                                if(node.name == data.actual_node)
                                {
                                    int nodeIndex = Array.FindIndex(allNodes, nodeData => nodeData.name == node.name);

                                    foreach(GameObject npcShip in allNodes[nodeIndex].npcShips)
                                    {
                                        for(int i = 0; i < allNodes[nodeIndex].totalNpcsPerShipClass; i++)
                                        {
                                            // int randomIndex = UnityEngine.Random.Range(0, allNodes[nodeIndex].npcShips.Count);
                                            foreach(ShipData ship in shipManager.allShipsData)
                                            {
                                                if(ship.name == npcShip.name)
                                                {
                                                    if(node.name == "A1")
                                                        allData.shipFunNames.Add(ship.classShipNames[UnityEngine.Random.Range(0, 2)]);
                                                    else
                                                        allData.shipFunNames.Add(ship.classShipNames[i]);
                                                }

                                            }
                                            allData.nodeNpcShips.Add(npcShip);
                                        }
                                    }
                                }
                            }
                                
                            for (int i = 0;i < data.visited_planets.Count;i++)
                            {
                                allData.discoveredNodes.Add(data.visited_planets[i]);
                            }

                            foreach(string plan in data.ship_plans)
                            {
                                // GameManager.Instance.shipPlans.Add(plan);
                                allData.shipPlans.Add(plan);
                            }

                            SetHomePlanetProps(data.actual_node);

                            CheckCompletion();
                        });

                        CheckCompletion();
                    }));
                }
            }

            CheckCompletion();
        });

        GetPlayerMissions((data) => {

            allData.missionsData.Clear();
            int incompleteMissions = 0;

            if(data.missions.Count > 0)
            {
                // This will create the notifications in Planet 1.
                foreach(Mission mission in data.missions)
                {
                    if(!mission.completed)
                    {
                        GameManager.Instance.missionsData.Add(mission);
                        if(mission.scene == "CockPit View")
                            allData.missionsData.Add(mission);
                        if(mission.enabled)
                            incompleteMissions++;
                    }
                }

                if(incompleteMissions > 0)
                    planetPlansPanel.transform.Find("Notification/Notification Button").GetChild(0).GetComponent<TextMeshProUGUI>().text = incompleteMissions.ToString();
            }

            // This function continues in RefreshMissions() in GameManager.

            CheckCompletion();
        });
    }

    private IEnumerator GetOwnedShipsHealth(string player_id, int slot, Action callback)
    {
        int checks = 0;

        List<string> lootlockerStoreNames = new List<string> {"17059", "17061", "17062", "17063"};
        
        foreach (string shipSlot in lootlockerStoreNames)
        {
            int currentCheck = checks;

            LootLockerSDKManager.GetMemberRank(shipSlot, player_id, (response) =>
            {
                if (response.statusCode == 200) {
                    // 99999 represents no ship in that slot
                    if(response.score > 0 && response.score <= 10000)
                    {
                        GameManager.Instance.ownedShipSlots[currentCheck].GetComponent<BuyManager>().slotShipData.shipHealth = (float)response.score/10000;
                        GameManager.Instance.ownedShipSlots[currentCheck].GetComponent<BuyManager>().RefreshData();
                        if(currentCheck == slot)
                            quickRepair.transform.Find("HealthBar").GetChild(1).GetComponent<Image>().fillAmount = (float)response.score/10000;
                    } else if (currentCheck == slot){
                        isPlayerDead = true;
                        GameManager.Instance.ResetBuyShipSlot(currentCheck, true);
                    } else {
                        GameManager.Instance.ResetBuyShipSlot(currentCheck, false);
                    }
                } else {
                    Debug.Log("failed: " + response.Error);
                }   
            });
            checks++;
        }
        yield return new WaitUntil(() => checks >= 4);
        // Notify that CheckFirstTimeLogin is complete
        callback.Invoke();
    }

    public void SetPlayerName()
    {
        LootLockerSDKManager.GetPlayerName((response) => 
        {
            if (response.success)
            {
                playerName.text = response.name;
                allData.playerName = response.name;
            }
        });
    }

    private void SetHomePlanetProps(string actual_node)
    {
        if(allData.previousScene == "GamePlay")
        {
            if(allData.shipData != null)
            {
                GameManager.Instance.selectedShip = allData.shipData;
            }
            if(allData.pilotData != null)
            {
                GameManager.Instance.selectedPilot = allData.pilotData;
            }
            GameManager.Instance.ownedShipSlots[allData.slot].GetComponent<BuyManager>().slotShipData.shipHealth = allData.shipHealth;
            foreach(PodData pod in allData.podDatas)
            {
                GameManager.Instance.selectedPods.Add(pod);
            }

            foreach(WeaponData weapon in allData.weaponDatas)
            {
                GameManager.Instance.selectedWeapons.Add(weapon);
            }
            GameManager.Instance.shipSelectedIndex = allData.slot;
        }

        if(actual_node == "A1")
        {
            background.GetComponent<Image>().sprite = planet1;
            backgroundExtended.GetComponent<Image>().sprite = planet1Extended;
            planetName.text = "Welcome to Aether";
        }
        else if(actual_node == "A5")
        {
            background.GetComponent<Image>().sprite = planet2;
            backgroundExtended.GetComponent<Image>().sprite = planet2Extended;
            planetName.text = "Welcome to Ebisu";
            menuPilotButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
            menuPilotButton.GetComponent<Button>().interactable = false;
            menuHangarButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
            menuHangarButton.GetComponent<Button>().interactable = false;
            planetPlansPanel.SetActive(true);
            if(!allData.shipPlans.Contains("plan1"))
            {
                planetPlansPanel.transform.GetChild(1).gameObject.SetActive(true);
                // planetPlansPanel.transform.GetChild(0).Find("Frame").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Plans need to be purchased.";
                planetPlansPanel.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<Image>().sprite = shipPlansClass2ShipImage;
                planetPlansPanel.transform.GetChild(1).GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = "Class II plans";
                planetPlansPanel.transform.GetChild(1).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = "9.000";
            }
        }
        else if(actual_node == "A9")
        {
            background.GetComponent<Image>().sprite = planet3;
            backgroundExtended.GetComponent<Image>().sprite = planet3Extended;
            planetName.text = "Welcome to Enki";
            menuPilotButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
            menuPilotButton.GetComponent<Button>().interactable = false;
            menuHangarButton.GetComponent<Button>().transition = Selectable.Transition.ColorTint;
            menuHangarButton.GetComponent<Button>().interactable = false;
            planetPlansPanel.SetActive(true);
            if(!allData.shipPlans.Contains("plan2"))
            {
                planetPlansPanel.transform.GetChild(1).gameObject.SetActive(true);
                // planetPlansPanel.transform.GetChild(0).Find("Frame").GetChild(0).GetComponent<TextMeshProUGUI>().text = "Plans need to be purchased.";
                planetPlansPanel.transform.GetChild(1).GetChild(1).GetChild(1).GetComponent<Image>().sprite = shipPlansClass3ShipImage;
                planetPlansPanel.transform.GetChild(1).GetChild(1).GetChild(2).GetComponent<TextMeshProUGUI>().text = "Class III plans";
                planetPlansPanel.transform.GetChild(1).GetChild(1).GetChild(3).GetComponent<TextMeshProUGUI>().text = "9.000";
            }
        }
    }
}
