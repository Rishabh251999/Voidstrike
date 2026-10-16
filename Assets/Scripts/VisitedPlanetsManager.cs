using UnityEngine;
using System.IO;
using System.Text;
using LootLocker.Requests;
using System.Collections;
using UnityEngine.Networking;
using System;
using System.Collections.Generic;

[Serializable]
public class PlanetsData
{
    public List<string> visited_planets;
    public string actual_node;
    public List<string> ship_plans;
}

// Use this to GetPlayerFile() from another file
// visitedPlanetsManager.GetPlayerFile((data) =>
// {
//     // Do something with data here
//     for (int i = 0;i < data.visited_planets.Count;i++)
//     {
//         Debug.Log(data.visited_planets[i]);
//     }
// });

public class VisitedPlanetsManager : MonoBehaviour
{
    // public void CreateVisitedPlanetsFile()
    // {
    //     bool isPublic = true;
    //     string initial_planet = "A1";

    //     // Create a new instance of PlanetData
    //     PlanetsData data = new();
    //     // Add planets to the array
    //     data.visited_planets = new List<string> { initial_planet };
    //     data.actual_node = "A1";
    //     // Convert PlanetData to JSON string
    //     string json = JsonUtility.ToJson(data);

    //     // Create a temporary file path
    //     string filePath = Path.Combine(Application.persistentDataPath, "visited_planets.json");
    //     // Write the JSON string to the temporary file
    //     File.WriteAllText(filePath, json);
    //     var file = File.Open(filePath, FileMode.Open);
    //     LootLockerSDKManager.UploadPlayerFile(file, "visited_planets", isPublic, response =>
    //     {
    //         if (response.success)
    //         {
    //             Debug.Log("Successfully uploaded player file, url: " + response.url);
    //             // Delete the temporary file
    //             file.Close();
    //             File.Delete(filePath);
    //         }
    //         else
    //         {
    //             Debug.Log("Error uploading player file");
    //         }
    //     });
    // }

    private void UpdatePlayerFile(string json, int fileId)
    {
        // Create a temporary file path
        string filePath = Path.Combine(Application.persistentDataPath, "visited_planets.json");

        // Write the JSON string to the temporary file
        File.WriteAllText(filePath, json);

        var file = File.Open(filePath, FileMode.Open);

        LootLockerSDKManager.UpdatePlayerFile(fileId, file, response =>
        {
            if (response.success)
            {
                Debug.Log("Successfully updated player file, url: " + response.url);
                // Delete the temporary file
                file.Close();
                File.Delete(filePath);
                if (CockPitManager.Instance != null)
                {
                    CockPitManager.Instance.processesToFinish++;
                }
                
                if (CockPitMultiplayerManager.Instance != null)
                {
                    CockPitMultiplayerManager.Instance.processesToFinish++;
                }
            }
            else
            {
                Debug.Log("Error updating player file");
            }
        });
    }

    public void GetPlayerFile(Action<PlanetsData> callback)
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
            print("----------- URL" + fileUrl);
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

    public void UpdateVisitedPlanets(string planet)
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

                    if(planet == "A1" || planet == "A5" || planet == "A9")
                        data.actual_node = planet;

                    string planetToFind = planet;
                    if (data.visited_planets.Contains(planetToFind))
                    {
                        if(planet == "A1" || planet == "A5" || planet == "A9")
                        {
                            string updatedJsonString = JsonUtility.ToJson(data);
                            UpdatePlayerFile(updatedJsonString, fileId);
                        }
                        Debug.Log($"{planetToFind} has been visited.");
                    }
                    else
                    {
                        Debug.Log($"{planetToFind} has not been visited.");
                        // Add the new planet to the list
                        data.visited_planets.Add(planetToFind);
                        // Serialize the data object back to a JSON string
                        string updatedJsonString = JsonUtility.ToJson(data);
                        Debug.Log("Updated JSON: " + updatedJsonString);

                        UpdatePlayerFile(updatedJsonString, fileId);
                    }
                }));
            }
            else
            {
                Debug.Log("Error retrieving player storage");
            }
        });
    }

    public void UpdateActualNode(string planet)
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

                        data.actual_node = planet;
                        // Serialize the data object back to a JSON string
                        string updatedJsonString = JsonUtility.ToJson(data);
                        Debug.Log("Updated JSON: " + updatedJsonString);

                        UpdatePlayerFile(updatedJsonString, fileId);
                    }
                ));
            }
            else
            {
                Debug.Log("Error retrieving player storage");
            }
        });
    }

    private IEnumerator GetJSONFromURL(string url, Action<string> callback)
    {
        print("------------ URL " + url);
        using (UnityWebRequest webRequest = UnityWebRequest.Get(url))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result != UnityWebRequest.Result.Success)
            {
                Debug.LogError("Failed to retrieve JSON from URL: " + webRequest.error);
                callback(null);
            }

            string json = webRequest.downloadHandler.text;

            // Process the JSON data
            Debug.Log("JSON Data: " + json);
            callback(json);
        }
    }
}