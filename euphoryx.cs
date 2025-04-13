using BepInEx;
using HarmonyLib;
using UnityEngine;
using UnityEngine.SceneManagement;
using System.Reflection;

namespace tech.polyeons.euphoryx
{
    [BepInPlugin("tech.polyeons.euphoryxcore", "Euphoryx", "1.0.0")]
    public class Euphoryx : BaseUnityPlugin
    {
        private Harmony harmony;

        private void Awake()
        {
            Logger.LogInfo("Euphoryx initialized");
            
            // Initialize Harmony
            harmony = new Harmony("tech.polyeons.euphoryxcore");
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            
            SceneManager.sceneLoaded += OnSceneLoaded;
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
        {
            Logger.LogInfo($"Scene loaded: {scene.name}");
        }

        private void OnDestroy()
        {
            SceneManager.sceneLoaded -= OnSceneLoaded;
            harmony?.UnpatchSelf();
        }
    }
}