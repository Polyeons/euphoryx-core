using UnityEngine;
using BepInEx.Logging;

namespace tech.polyeons.euphoryx.utils
{
    public class SceneAnalyzer
    {
        private static ManualLogSource Logger;

        public static void Initialize(ManualLogSource logger)
        {
            Logger = logger;
        }
    }
}