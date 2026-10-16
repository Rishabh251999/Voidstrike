using System;
using System.Collections;
using System.IO;
using UnityEngine;

public class Logger : MonoBehaviour
{
    private static string logFilePath;

    private void Start()
    {
        // Check if the application is running on Linux
        if (IsLinux())
        {
            // Use the specific Linux path
            logFilePath = "/home/ubuntu/server/log/log.txt";
        }
        else
        {
            // Use the persistent data path for non-Linux systems
            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            logFilePath = Path.Combine(Application.persistentDataPath, $"Log_{timestamp}.txt");
        }

        // Ensure the directory exists
        string logDirectory = Path.GetDirectoryName(logFilePath);
        if (!Directory.Exists(logDirectory))
        {
            Directory.CreateDirectory(logDirectory);
        }

        // Subscribe to Unity's log messages
        Application.logMessageReceived += LogMessageHandler;
    }

    private void OnDestroy()
    {
        // Unsubscribe from Unity's log messages
        Application.logMessageReceived -= LogMessageHandler;
    }

    private static void LogMessageHandler(string condition, string stackTrace, LogType type)
    {
        LogLevel logLevel = LogLevel.Info;

        switch (type)
        {
            case LogType.Warning:
                logLevel = LogLevel.Warning;
                break;
            case LogType.Error:
            case LogType.Exception:
                logLevel = LogLevel.Error;
                break;
        }

        Log($"{condition}\n{stackTrace}", logLevel);
    }

    private static void Log(string message, LogLevel level = LogLevel.Info)
    {
        string logLine = $"{DateTime.Now} [{level}] {message}";

        // Append the log to the file
        File.AppendAllText(logFilePath, logLine + Environment.NewLine);
    }

    public static void LogWarning(string message)
    {
        Log(message, LogLevel.Warning);
    }

    public static void LogError(string message)
    {
        Log(message, LogLevel.Error);
    }

    public enum LogLevel
    {
        Info,
        Warning,
        Error
    }

    private static bool IsLinux()
    {
        return SystemInfo.operatingSystem.Contains("Linux");
    }
}
