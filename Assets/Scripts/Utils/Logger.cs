using System;
using System.Diagnostics;
using UnityEngine;

namespace Hampossible.Utils
{
    public static class HLogger
    {
        public enum LogLevel { Info, Warning, Error, Debug }
        public enum LogCategory { General, Player, Skill, UI }

        public static LogLevel MinimumLogLevel = LogLevel.Info;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        private static void Initialize()
        {
#if UNITY_EDITOR
            MinimumLogLevel = LogLevel.Info;
#else
            MinimumLogLevel = LogLevel.Warning;
#endif

            UnityEngine.Debug.Log("현재 MinimumLogLevel: " + MinimumLogLevel);
        }

        public static void Info(string message, UnityEngine.Object context = null)
        {
            if (MinimumLogLevel <= LogLevel.Info)
                UnityEngine.Debug.Log(Format("INFO", message, "#00BFFF"), context); // DeepSkyBlue
        }

        public static void Warning(string message, UnityEngine.Object context = null)
        {
            if (MinimumLogLevel <= LogLevel.Warning)
                UnityEngine.Debug.LogWarning(Format("WARN", message, "#FFA500"), context); // Orange
        }

        public static void Error(string message, UnityEngine.Object context = null)
        {
            if (MinimumLogLevel <= LogLevel.Error)
                UnityEngine.Debug.LogError(Format("ERROR", message, "#FF4500"), context); // OrangeRed
        }

        [Conditional("UNITY_EDITOR"), Conditional("DEVELOPMENT_BUILD")]
        public static void DebugLog(string message, UnityEngine.Object context = null)
        {
            if (MinimumLogLevel <= LogLevel.Debug)
                UnityEngine.Debug.Log(Format("DEBUG", message, "#808080"), context); // Gray
        }

        public static void Log(LogCategory? category = null, LogLevel level = LogLevel.Info, string message = "", UnityEngine.Object context = null)
        {
            if (MinimumLogLevel > level) return;

            string prefix = $"[{(category ?? LogCategory.General)}]";
            switch (level)
            {
                case LogLevel.Info:
                    Info($"{prefix} {message}", context);
                    break;
                case LogLevel.Warning:
                    Warning($"{prefix} {message}", context);
                    break;
                case LogLevel.Error:
                    Error($"{prefix} {message}", context);
                    break;
                case LogLevel.Debug:
                    DebugLog($"{prefix} {message}", context);
                    break;
            }
        }

        private static string Format(string level, string message, string colorHex)
        {
            var frame = new StackFrame(2, true);
            string file = frame.GetFileName();
            int line = frame.GetFileLineNumber();
            string callerInfo = string.IsNullOrEmpty(file) ? "" : $" ({System.IO.Path.GetFileName(file)}:{line})";
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff");
            return $"<color={colorHex}>[Hampossible] [{level}] [{timestamp}]{callerInfo} - {message}</color>";
        }

        public static class Player
        {
            public static void Info(string message, UnityEngine.Object context = null) => Log(LogCategory.Player, LogLevel.Info, message, context);
            public static void Warning(string message, UnityEngine.Object context = null) => Log(LogCategory.Player, LogLevel.Warning, message, context);
            public static void Error(string message, UnityEngine.Object context = null) => Log(LogCategory.Player, LogLevel.Error, message, context);
            public static void Debug(string message, UnityEngine.Object context = null) => Log(LogCategory.Player, LogLevel.Debug, message, context);
        }

        public static class Skill
        {
            public static void Info(string message, UnityEngine.Object context = null) => Log(LogCategory.Skill, LogLevel.Info, message, context);
            public static void Warning(string message, UnityEngine.Object context = null) => Log(LogCategory.Skill, LogLevel.Warning, message, context);
            public static void Error(string message, UnityEngine.Object context = null) => Log(LogCategory.Skill, LogLevel.Error, message, context);
            public static void Debug(string message, UnityEngine.Object context = null) => Log(LogCategory.Skill, LogLevel.Debug, message, context);
        }

        public static class UI
        {
            public static void Info(string message, UnityEngine.Object context = null) => Log(LogCategory.UI, LogLevel.Info, message, context);
            public static void Warning(string message, UnityEngine.Object context = null) => Log(LogCategory.UI, LogLevel.Warning, message, context);
            public static void Error(string message, UnityEngine.Object context = null) => Log(LogCategory.UI, LogLevel.Error, message, context);
            public static void Debug(string message, UnityEngine.Object context = null) => Log(LogCategory.UI, LogLevel.Debug, message, context);
        }

        public static class General
        {
            public static void Info(string message, UnityEngine.Object context = null) => Log(LogCategory.General, LogLevel.Info, message, context);
            public static void Warning(string message, UnityEngine.Object context = null) => Log(LogCategory.General, LogLevel.Warning, message, context);
            public static void Error(string message, UnityEngine.Object context = null) => Log(LogCategory.General, LogLevel.Error, message, context);
            public static void Debug(string message, UnityEngine.Object context = null) => Log(LogCategory.General, LogLevel.Debug, message, context);
        }
    }


}
