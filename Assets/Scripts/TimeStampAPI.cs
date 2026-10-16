using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public class WorldTimeData
{
    public string utc_datetime;
}

public class TimeStampAPI : MonoBehaviour
{
    private const string apiUrl = "https://worldtimeapi.org/api/timezone/Asia/Kolkata";

    // Define a delegate to handle the timestamp response
    public delegate void WorldTimeDataCallback(DateTime timestamp);

    public void FetchTimestamp(WorldTimeDataCallback callback)
    {
        StartCoroutine(FetchTimestampRoutine(callback));
    }
    
    private IEnumerator FetchTimestampRoutine(WorldTimeDataCallback callback)
    {
        using (UnityWebRequest webRequest = UnityWebRequest.Get(apiUrl))
        {
            yield return webRequest.SendWebRequest();

            if (webRequest.result == UnityWebRequest.Result.ConnectionError || webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                Debug.LogError("Failed to fetch timestamp: " + webRequest.error);
            }
            else
            {
                string jsonResponse = webRequest.downloadHandler.text;
                WorldTimeData worldTimeData = JsonUtility.FromJson<WorldTimeData>(jsonResponse);

                // Use DateTimeStyles to adjust the parsing to UTC
                DateTime currentTimestamp = DateTime.ParseExact(
                    worldTimeData.utc_datetime, 
                    "yyyy-MM-ddTHH:mm:ss.ffffffK", 
                    CultureInfo.InvariantCulture, 
                    DateTimeStyles.AdjustToUniversal);

                callback(currentTimestamp);
            }
        }
    }
}