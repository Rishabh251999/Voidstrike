using System;
using System.Collections;
using LootLocker.Requests;
using UnityEngine;

public class TwitterIntegration : MonoBehaviour
{
    private TimeStampAPI timeAPI;
    public GameObject TwitterMessage;
    private readonly string key = "AlreadyTwitterRewarded";

    public void AuthenticateUser()
    {

        StartCoroutine(CheckAlreadyRedeemedTwitterReward((alreadyRedeemed) => {
            if(GameManager.Instance.currency < 500000 && !alreadyRedeemed)
            {
                #if UNITY_WEBGL && !UNITY_EDITOR
                Application.ExternalEval("authenticateWithTwitter();");
                #endif
            }
            else
                TwitterMessage.SetActive(true);
        }));
    }
    
    public IEnumerator CheckAlreadyRedeemedTwitterReward(Action<bool> callback)
    {
        bool hasFinishedAsyncOperation = false;
        bool alreadyRedeemed = false;
        DateTime currentTime = DateTime.MinValue;
        timeAPI = GetComponent<TimeStampAPI>();

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
                                    currentTime = apiTime;
                                    hasFinishedAsyncOperation = true;
                                
                                    TimeSpan timeDifference = currentTime - storedTimestamp;
                                    if (timeDifference <= TimeSpan.FromDays(1))
                                        alreadyRedeemed = true;
                                });
                            }catch(Exception err){
                                Debug.Log("tesst Error: " + err);
                                hasFinishedAsyncOperation = true;
                                alreadyRedeemed = true;
                            }
                        }else{
                            hasFinishedAsyncOperation = true;
                        }
                    }
                    else
                    {
                        Debug.Log("tesst Error getting player storage");
                        hasFinishedAsyncOperation = true;
                    }
                });
                yield return new WaitUntil(() => hasFinishedAsyncOperation);
                // Notify that CheckFirstTimeLogin is complete
                callback.Invoke(alreadyRedeemed);
    }   
}