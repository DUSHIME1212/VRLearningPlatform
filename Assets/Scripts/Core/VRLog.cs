using System;
using UnityEngine;

namespace VRLearning.Core
{
    /// <summary>
    /// Central logging wrapper. Keeps the project's "[Tag] message" convention in one place so output
    /// can be filtered, silenced, or re-routed (e.g. to Sentry) without touching call sites. Exceptions
    /// go through <see cref="UnityEngine.Debug.LogException"/> so the Sentry SDK auto-captures them with
    /// a full stack trace — replacing the old pattern of logging only <c>e.Message</c>.
    /// </summary>
    public static class VRLog
    {
        /// <summary>Set false in a release build to silence Info spam. Warnings/errors always log.</summary>
        public static bool Verbose = true;

        public static void Info(string tag, string message)
        {
            if (Verbose) Debug.Log($"[{tag}] {message}");
        }

        public static void Warn(string tag, string message) => Debug.LogWarning($"[{tag}] {message}");

        public static void Error(string tag, string message) => Debug.LogError($"[{tag}] {message}");

        /// <summary>Logs an exception with its full stack trace (Sentry auto-captures LogException).</summary>
        public static void Exception(string tag, Exception e)
        {
            Debug.LogError($"[{tag}] {e.GetType().Name}: {e.Message}");
            Debug.LogException(e);
        }
    }
}