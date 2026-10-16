using System;
using System.Collections;
using System.Globalization;
using UnityEngine;
using UnityEngine.Networking;

/// <summary>
/// Fetches the current UTC time without relying on worldtimeapi.org.
/// Order: Cloudflare trace -> HTTP "Date" header from several hosts -> device clock.
/// The callback is ALWAYS invoked, so callers never hang.
/// </summary>
public class TimeStampAPI : MonoBehaviour
{
    private const string cloudflareTraceUrl = "https://www.cloudflare.com/cdn-cgi/trace";

    // Any HTTPS server returns a "Date" header (UTC, 1-second precision).
    private static readonly string[] dateHeaderHosts =
    {
        "https://api.lootlocker.io",
        "https://www.google.com"
    };

    private const int requestTimeoutSeconds = 5;

    // Kept the same name/signature so Login_Register.cs doesn't need changes.
    public delegate void WorldTimeDataCallback(DateTime timestamp);

    public void FetchTimestamp(WorldTimeDataCallback callback)
    {
        StartCoroutine(FetchTimestampRoutine(callback));
    }

    private IEnumerator FetchTimestampRoutine(WorldTimeDataCallback callback)
    {
        // 1) Cloudflare trace: plain text containing "ts=<unix seconds.fraction>"
        using (UnityWebRequest req = UnityWebRequest.Get(cloudflareTraceUrl))
        {
            req.timeout = requestTimeoutSeconds;
            yield return req.SendWebRequest();

            if (req.result == UnityWebRequest.Result.Success)
            {
                if (TryParseCloudflareTrace(req.downloadHandler.text, out DateTime cfTime))
                {
                    callback(cfTime);
                    yield break;
                }
                Debug.LogWarning("Cloudflare trace response had no valid 'ts' line.");
            }
            else
            {
                Debug.LogWarning("Cloudflare time fetch failed: " + req.error);
            }
        }

        // 2) Fallback: "Date" header from reliable HTTPS hosts
        foreach (string url in dateHeaderHosts)
        {
            using (UnityWebRequest req = UnityWebRequest.Head(url))
            {
                req.timeout = requestTimeoutSeconds;
                yield return req.SendWebRequest();

                // Even a 404/403 response carries a valid Date header,
                // so only skip when the connection itself failed.
                if (req.result == UnityWebRequest.Result.ConnectionError)
                {
                    Debug.LogWarning($"Date header fetch failed for {url}: {req.error}");
                    continue;
                }

                string dateHeader = req.GetResponseHeader("Date");
                if (!string.IsNullOrEmpty(dateHeader) &&
                    DateTime.TryParse(
                        dateHeader,
                        CultureInfo.InvariantCulture,
                        DateTimeStyles.AdjustToUniversal | DateTimeStyles.AssumeUniversal,
                        out DateTime headerTime))
                {
                    callback(headerTime);
                    yield break;
                }
            }
        }

        // 3) Last resort: device clock, so the caller never hangs
        Debug.LogWarning("All time sources failed. Using device UTC time.");
        callback(DateTime.UtcNow);
    }

    private static bool TryParseCloudflareTrace(string body, out DateTime utcTime)
    {
        utcTime = default;
        if (string.IsNullOrEmpty(body)) return false;

        foreach (string rawLine in body.Split('\n'))
        {
            string line = rawLine.Trim();
            if (!line.StartsWith("ts=")) continue;

            string value = line.Substring(3);
            if (double.TryParse(value, NumberStyles.Float, CultureInfo.InvariantCulture, out double seconds))
            {
                utcTime = DateTimeOffset.FromUnixTimeMilliseconds((long)(seconds * 1000)).UtcDateTime;
                return true;
            }
        }
        return false;
    }
}