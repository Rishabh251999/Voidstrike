using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System;

[Serializable]
public class EmailCheckResponse
{
    public bool isInWhitelist;
    public bool isWhitelistEnabled;
    public bool isLoginEnabled;
}

[Serializable]
public class CurrencyCheckResponse
{
    public int startingCurrencyValue;
}

public class WhitelistAPI : MonoBehaviour
{   
    public delegate void WhitelistCallback(bool isInWhitelist);
    public delegate void CurrencyCallback(int startingCurrency);

    public IEnumerator FetchWhitelist(string email, WhitelistCallback callback)
    {
        // string emailToCheck = "test@example.com"; // The email to check
        // string whitelistUrl = $"https://api.playmythos.com/emails?email={email}";
        // URL-encode the email address
        string encodedEmail = Uri.EscapeDataString(email);
        string whitelistUrl = $"https://api.playmythos.com/emails?email={encodedEmail}";

        using (UnityWebRequest request = UnityWebRequest.Get(whitelistUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                EmailCheckResponse response = JsonUtility.FromJson<EmailCheckResponse>(jsonResponse);
                callback?.Invoke(response.isInWhitelist);
            }
            else
            {
                Debug.LogError("Failed to check email in whitelist: " + request.error);
                callback?.Invoke(false);
            }
        }
    }

    public IEnumerator FetchWhitelistEnabled(WhitelistCallback callback)
    {
        string whitelistUrl = $"https://api.playmythos.com/whitelist_enabled";

        using (UnityWebRequest request = UnityWebRequest.Get(whitelistUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                EmailCheckResponse response = JsonUtility.FromJson<EmailCheckResponse>(jsonResponse);
                callback?.Invoke(response.isWhitelistEnabled);
            }
            else
            {
                Debug.LogError("Failed to check whitelist enabled: " + request.error);
                callback?.Invoke(false);
            }
        }
    }

    public IEnumerator FetchInitialCurrency(CurrencyCallback callback)
    {
        string currencyUrl = "https://api.playmythos.com/starting_currency";

        using (UnityWebRequest request = UnityWebRequest.Get(currencyUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                CurrencyCheckResponse response = JsonUtility.FromJson<CurrencyCheckResponse>(jsonResponse);
                callback?.Invoke(response.startingCurrencyValue);
            }
            else
            {
                Debug.LogError("Failed to fetch starting currency: " + request.error);
                // You might want to handle the error differently here
                callback?.Invoke(0); // Or use another way to indicate an error
            }
        }
    }

    public IEnumerator FetchLoginEnabled(WhitelistCallback callback)
    {
        string loginUrl = $"https://api.playmythos.com/login_enabled";

        using (UnityWebRequest request = UnityWebRequest.Get(loginUrl))
        {
            yield return request.SendWebRequest();

            if (request.result == UnityWebRequest.Result.Success)
            {
                string jsonResponse = request.downloadHandler.text;
                EmailCheckResponse response = JsonUtility.FromJson<EmailCheckResponse>(jsonResponse);
                callback?.Invoke(response.isLoginEnabled);
            }
            else
            {
                Debug.LogError("Failed to check whitelist enabled: " + request.error);
                callback?.Invoke(false);
            }
        }
    }
}
