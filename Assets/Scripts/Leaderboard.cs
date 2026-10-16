using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using LootLocker;
using LootLocker.Requests;
using TMPro;
using System.Globalization;
using System;
using Mirror.Examples.MultipleMatch;

public class Leaderboard : MonoBehaviour
{

    public static Leaderboard Instance { get; private set; }

    public TextMeshProUGUI playerCurrencyNames;
    public TextMeshProUGUI playerCurrencyScores;
    public TextMeshProUGUI playerKillsNames;
    public TextMeshProUGUI playerPvpKillsNames;
    public TextMeshProUGUI playerKillsScores;
    public TextMeshProUGUI playerPvpKillsScores;
    public TextMeshProUGUI playerHoursNames;
    public TextMeshProUGUI playerHoursScores;

    private string leaderboardCurrencyID = "20539"; // "15483";
    private string leaderboardKillsID = "15679";
    private string leaderboardPvpKillsID = "20160";
    private string leaderboardHoursID = "15680";
    private string leaderboardLastLoginID = "18325";
    private readonly string key = "AlreadyTwitterRewarded";
    private readonly int TWITTER_REWARD = 100000;
    // Ship Slot Health Leaderboards. Use for storing ships health, each one corresponds to a slot.
    List<string> lootlockerStoreNames = new List<string> {"17059", "17061", "17062", "17063"};

    [Header("SO")]
    public Data allData;

    // private TimeAPI timeAPI;
    private TimeStampAPI timeAPI;
    private void Awake()
    {
        timeAPI = GetComponent<TimeStampAPI>();
        if (timeAPI == null)
        {
            Debug.LogError("TimeStampAPI component not found on the GameObject.");
        }
        if (Instance != null && Instance != this)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
    }

    private float timer = 0f;
    private float interval = 60f; // 60 seconds (1 minute)

    private void Update()
    {
        timer += Time.deltaTime;

        if (timer >= interval)
        {
            SetLeaderboardLastLogin();
            timer = 0f;
        }
    }

    /// <summary>
    /// Set and Get Myth Leaderboard
    /// </summary>
    public void SetLeaderboardCurrency(int score)
    {
        int submitScore;
        string metadata;
        if (score == 0){
            submitScore = 1;
            metadata = "true";}
        else
        {
            submitScore = Mathf.RoundToInt(score);
            metadata = "false";
        }
        
        Debug.Log("submitScore: " + submitScore);
        StartCoroutine(SubmitScoreMetadataRoutine(submitScore, leaderboardCurrencyID, metadata));
    }

    public void SetTwitterReward()
    {
        StartCoroutine(CheckAndUpdateAlreadyRedeemedTwitterReward((alreadyRedeemed) => {
            if(GameManager.Instance.currency < 500000 && !alreadyRedeemed)
            {
                GameManager.Instance.currency += TWITTER_REWARD;
                StartCoroutine(SubmitScoreMetadataRoutine(GameManager.Instance.currency, leaderboardCurrencyID, "false"));
                GameManager.Instance.currencyText.text = GameManager.Instance.currency.ToString("#,#", CultureInfo.InvariantCulture);
            }
        }));
    }

    public IEnumerator CheckAndUpdateAlreadyRedeemedTwitterReward(Action<bool> callback)
    {
        timeAPI = GetComponent<TimeStampAPI>();

        bool hasFinishedAsyncOperation = false;
        bool alreadyRedeemed = false;
        DateTime currentTime = DateTime.MinValue;

        timeAPI.FetchTimestamp((DateTime timestamp) => {

            // Calculate the Unix timestamp
            TimeSpan unixTimeSpan = timestamp - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timestampAsUnixTime = (long)unixTimeSpan.TotalSeconds;
            int timestampAsInteger = (int)timestampAsUnixTime;

            LootLockerSDKManager.GetSingleKeyPersistentStorage(key, (response) =>
                {
                    if (response.success)
                    {
                        if (response.payload != null)
                        {
                            long respToLong = long.Parse(response.payload.value);

                            DateTime storedTimestamp = DateTimeOffset.FromUnixTimeSeconds(respToLong).UtcDateTime;

                            try{
                                timeAPI.FetchTimestamp((apiTime) => {
                                    TimeSpan timeDifference = apiTime - storedTimestamp;
                                    if (timeDifference <= TimeSpan.FromDays(1))
                                        alreadyRedeemed = true;
                                    else
                                    {
                                        TimeSpan unixTimeSpan = apiTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                        long timestampAsUnixTime = (long)unixTimeSpan.TotalSeconds;
                                        int timestampAsInteger = (int)timestampAsUnixTime;

                                        LootLockerSDKManager.UpdateOrCreateKeyValue(key, timestampAsInteger.ToString(), true, (getPersistentStoragResponse) =>
                                        {
                                            if (getPersistentStoragResponse.success)
                                            {
                                                Debug.Log("Successfully updated twitter reward date");
                                            }
                                        });
                                    }
                                    hasFinishedAsyncOperation = true;
                                });
                            }catch(Exception err){
                                Debug.Log("tesst Error: " + err);
                                hasFinishedAsyncOperation = true;
                                alreadyRedeemed = true;
                            }
                        }else{
                            timeAPI.FetchTimestamp((apiTime) => {

                                    TimeSpan unixTimeSpan = apiTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
                                    long timestampAsUnixTime = (long)unixTimeSpan.TotalSeconds;
                                    int timestampAsInteger = (int)timestampAsUnixTime;

                                    LootLockerSDKManager.UpdateOrCreateKeyValue(key, timestampAsInteger.ToString(), true, (getPersistentStoragResponse) =>
                                    {
                                        if (getPersistentStoragResponse.success)
                                        {
                                            Debug.Log("Successfully updated twitter reward date");
                                        }
                                    });
                                    
                                    hasFinishedAsyncOperation = true;
                                });
                        }
                    }
                    else
                    {
                        Debug.Log("Error getting player storage");
                        hasFinishedAsyncOperation = true;
                    }
                });
        });

                yield return new WaitUntil(() => hasFinishedAsyncOperation);
                // Notify that CheckFirstTimeLogin is complete
                callback.Invoke(alreadyRedeemed);
    }

    public void GetLeaderboardCurrency()
    {
        StartCoroutine(FetchTopHighscoresRoutine(leaderboardCurrencyID, playerCurrencyNames, playerCurrencyScores));
    }
    
    /// <summary>
    /// Set and Get Kills Leaderboard
    /// </summary>
    public void SetLeaderboardKills(int score)
    {
        StartCoroutine(SubmitScoreRoutine(score, leaderboardKillsID));
    }
    public void GetLeaderboardKills()
    {
        StartCoroutine(FetchTopHighscoresRoutine(leaderboardKillsID, playerKillsNames, playerKillsScores));
    }

    /// <summary>
    /// Set and Get PvP Kills Leaderboard
    /// </summary>
    public void SetLeaderboardPvpKills(int score)
    {
        StartCoroutine(SubmitScoreRoutine(score, leaderboardPvpKillsID));
    }
    public void GetLeaderboardPvpKills()
    {
        StartCoroutine(FetchTopHighscoresRoutine(leaderboardPvpKillsID, playerPvpKillsNames, playerPvpKillsScores));
    }

    public void SetLeaderboardLastLogin()
    {
        // Get the TimeAPI component from the same GameObject
        timeAPI = GetComponent<TimeStampAPI>();

        if (timeAPI != null)
        {
            // Call the FetchTimestamp function to get the timestamp from TimeAPI
            timeAPI.FetchTimestamp(HandleTimestampFromAPI);
        }
        else
        {
            Debug.LogError("TimeAPI script not found on the same GameObject as Leaderboard.");
        }
    }

    private void HandleTimestampFromAPI(DateTime timestampFromAPI)
    {
        // Check if the timestamp is valid
        if (timestampFromAPI != DateTime.MinValue)
        {
            // Calculate the Unix timestamp
            TimeSpan unixTimeSpan = timestampFromAPI - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timestampAsUnixTime = (long)unixTimeSpan.TotalSeconds;
            int timestampAsInteger = (int)timestampAsUnixTime;
            StartCoroutine(SubmitScoreRoutine(timestampAsInteger, leaderboardLastLoginID));
        }
        else
        {
            Debug.LogError("Failed to retrieve the timestamp from the API. We will use local time.");
            DateTime currentTime = DateTime.Now;
            // Repeat the same process as above with currentTime
            TimeSpan unixTimeSpan = currentTime - new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
            long timestampAsUnixTime = (long)unixTimeSpan.TotalSeconds;
            int timestampAsInteger = (int)timestampAsUnixTime;
            StartCoroutine(SubmitScoreRoutine(timestampAsInteger, leaderboardLastLoginID));
        }
    }

    public void GetPlayerLastLogin(string playerID, Action<int> callback)
    {
        LootLockerSDKManager.GetMemberRank(leaderboardLastLoginID, playerID, (response) =>
        {
            if (response.statusCode == 200) {
                int lastLoginScore = response.score;
                callback(lastLoginScore);
            } else {
            }
        });
    }

    /// <summary>
    /// Set and Get Hours Leaderboard
    /// </summary>
    public void SetLeaderboardHours(int score)
    {
        StartCoroutine(SubmitScoreRoutine(score, leaderboardHoursID));
    }
    public void GetLeaderboardHours()
    {
        StartCoroutine(FetchTopHighscoresRoutine(leaderboardHoursID, playerHoursNames, playerHoursScores));
    }

    /// <summary>
    /// Set Ship slots health
    /// </summary>
    public void SetLeaderboardShipSlotHealth(int slot, int score)
    {
        StartCoroutine(SubmitScoreRoutine(score, lootlockerStoreNames[slot]));
    }

    /// <summary>
    /// Get player's amount of currency from Global Currency Leaderboard
    /// </summary>
    public void GetPlayerCurrency(string playerID)
    {
        LootLockerSDKManager.GetMemberRank(leaderboardCurrencyID, playerID, (response) =>
        {
            if (response.statusCode == 200) {
                if(response.score == 1 && response.metadata == "true")
                {
                    GameManager.Instance.currency = 0;
                    GameManager.Instance.currencyText.text = "0";
                }
                else
                {
                    GameManager.Instance.currency = response.score;
                    GameManager.Instance.currencyText.text = response.score.ToString("#,#", CultureInfo.InvariantCulture);
                }
            } else {
                Debug.Log("failed: " + response.Error);
            }
        });
    }

    public void GetPlayerKills(string playerID)
    {
        LootLockerSDKManager.GetMemberRank(leaderboardKillsID, playerID, (response) =>
        {
            if (response.statusCode == 200) {
                allData.kills = response.score;
            } else {
            }   
        });
    }

    public void GetPlayerPvpKills(string playerID)
    {
        LootLockerSDKManager.GetMemberRank(leaderboardPvpKillsID, playerID, (response) =>
        {
            if (response.statusCode == 200) {
                allData.pvpKills = response.score;
            } else {
            }   
        });
    }

    public IEnumerator SubmitScoreRoutine(int score, string leaderboardID)
    {
        bool done = false;
        LootLockerSDKManager.SubmitScore("20", score, leaderboardID, (response) => {
            if(response.success){
                done = true;
            }
            else
            {
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

    public IEnumerator SubmitScoreMetadataRoutine(int score, string leaderboardID, string metadata)
    {
            bool done = false;
            LootLockerSDKManager.SubmitScore("20", score, leaderboardID, metadata, (response) => {
                if(response.success){
                    done = true;
                }
                else
                {
                    done = true;
                }
            });
            yield return new WaitWhile(() => done == false);
    }

    public IEnumerator FetchTopHighscoresRoutine(string leaderboardID, TextMeshProUGUI names, TextMeshProUGUI scores)
    {
        bool done = false;

        LootLockerSDKManager.GetScoreList(leaderboardID, 10, 0, (response) =>
        {
            if(response.success)
            {
                string tempPlayerNames = "";
                string tempPlayerScores = "";

                LootLockerLeaderboardMember[] members = response.items;

                for (int i = 0; i < members.Length; i++)
                {
                    tempPlayerNames += members[i].rank + ". ";
                    if(members[i].player.name != "")
                    {
                                                if(members[i].player.name.Length > 12)
                        {
                            members[i].player.name = members[i].player.name[..12] + "..";
                        }
                        tempPlayerNames += members[i].player.name;

                    }
                    else
                    {
                        tempPlayerNames += members[i].player.id;
                    }
                    tempPlayerScores += members[i].score.ToString("#,#",CultureInfo.InvariantCulture) + "\n";
                    tempPlayerNames += "\n";
                }
                done = true;
                names.text = tempPlayerNames;
                scores.text = tempPlayerScores;
            }
            else
            {
                done = true;
            }
        });
        yield return new WaitWhile(() => done == false);
    }

}