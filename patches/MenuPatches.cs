using HarmonyLib;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using System;
using System.Reflection;
using System.Linq;
using BepInEx.Logging;

namespace tech.polyeons.euphoryx.patches
{
    public class MenuPatches
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Euphoryx.MenuPatches");

        // Instead of patching MonoBehaviour.Start, let's patch SceneManager.Internal_SceneLoaded
        [HarmonyPatch(typeof(SceneManager), "Internal_SceneLoaded")]
        public static class SceneLoadedPatch
        {
            static void Postfix(Scene scene)
            {
                Logger.LogInfo($"Scene loaded: {scene.name}");
                
                if (scene.name == "Menu" || scene.name == "MainMenu")
                {
                    // Wait a frame to ensure everything is initialized
                    UnityEngine.Object.FindObjectOfType<MonoBehaviour>().StartCoroutine(FindMainMenuAfterDelay());
                }
            }
            
            static System.Collections.IEnumerator FindMainMenuAfterDelay()
            {
                // Wait for 2 frames to ensure everything is loaded
                yield return null;
                yield return null;
                
                // Find main menu objects
                var mainMenuObjects = UnityEngine.Object.FindObjectsOfType<MonoBehaviour>(true)
                    .Where(mb => mb.gameObject.name.Contains("Menu") || mb.GetType().Name.Contains("Menu"))
                    .ToArray();
                
                Logger.LogInfo($"Found {mainMenuObjects.Length} potential menu objects");
                
                foreach (var menuObject in mainMenuObjects)
                {
                    Logger.LogInfo($"Menu object: {menuObject.gameObject.name} of type {menuObject.GetType().FullName}");
                    LogHierarchy(menuObject.transform, 0);
                    FindAndAddModButton(menuObject.transform);
                }
            }
            
            static void LogHierarchy(Transform transform, int depth)
            {
                string indent = new string(' ', depth * 2);
                Logger.LogInfo($"{indent}- {transform.name}");
                
                for (int i = 0; i < transform.childCount; i++)
                {
                    LogHierarchy(transform.GetChild(i), depth + 1);
                }
            }
            
            static void FindAndAddModButton(Transform menuTransform)
            {
                // Check if we're in the MainMenu object
                if (!menuTransform.name.Equals("MainMenu"))
                    return;
                
                // Check if we already added a Mods button
                Transform existingModsButton = menuTransform.Find("Home/Bank/Mods");
                if (existingModsButton != null)
                {
                    Logger.LogInfo("Mods button already exists, skipping creation");
                    return;
                }
                
                // Find the settings button specifically in the MainMenu/Home/Bank path
                Transform homeTransform = menuTransform.Find("Home");
                Transform bankTransform = homeTransform?.Find("Bank");
                Transform settingsTransform = bankTransform?.Find("Settings");
                
                if (settingsTransform != null)
                {
                    var settingsButton = settingsTransform.GetComponent<Button>();
                    if (settingsButton != null)
                    {
                        Logger.LogInfo("Found settings button in correct location");
                        
                        var modmenuButton = UnityEngine.Object.Instantiate(settingsTransform.gameObject, bankTransform);
                        modmenuButton.name = "Mods";
                        
                        // Try to find and update text components
                        var texts = modmenuButton.GetComponentsInChildren<Component>(true);
                        foreach (var component in texts)
                        {
                            if (component.GetType().Name.Contains("Text"))
                            {
                                Logger.LogInfo($"Found text component: {component.GetType().Name}");
                                try
                                {
                                    var textProperty = AccessTools.Property(component.GetType(), "text");
                                    if (textProperty != null)
                                    {
                                        textProperty.SetValue(component, "Mods", null);
                                        Logger.LogInfo("Updated button text");
                                    }
                                }
                                catch (Exception ex)
                                {
                                    Logger.LogError($"Error setting text: {ex.Message}");
                                }
                            }
                        }
                        
                        var button = modmenuButton.GetComponent<Button>();
                        if (button != null)
                        {
                            button.onClick.RemoveAllListeners();
                            button.onClick.AddListener(() => {
                                Logger.LogInfo("Mod menu clicked!");
                                ShowModMenu();
                            });
                        }
                        
                        // Position it after Settings (index 5)
                        modmenuButton.transform.SetSiblingIndex(5);
                        Logger.LogInfo("Mod button added to menu");
                    }
                }
                else
                {
                    Logger.LogError("Could not find settings button in MainMenu/Home/Bank path");
                }
            }
            
            static void ShowModMenu()
            {
                // This will be implemented later to show the mod menu UI
                Logger.LogInfo("TODO: Implement mod menu UI");
            }
        }
    }
}