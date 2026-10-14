using System;
using UnityEngine;

namespace MoreVehiclesRenew2.Utils
{
    public static class Log
    {
        private const string Prefix = "[MoreVehiclesRenew2] ";

        public static void Info(string text)
        {
            Debug.Log(Prefix + text);
        }

        public static void Warning(string text)
        {
            Debug.LogWarning(Prefix + text);
        }

        public static void Error(string text)
        {
            Debug.LogError(Prefix + text);
        }
    }
}
