using HarmonyLib;
using UnityEngine;
using UnityEngine.UI; 
using TMPro;
using System;
using System.Linq;
using BepInEx.Logging;
using tech.polyeons.euphoryx.ui;

namespace tech.polyeons.euphoryx.patches
{
    public class MenuPatches
    {
        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Euphoryx.MainMenuPatches");
        private static bool modButtonAdded = false;

        [HarmonyPatch(typeof(ScheduleOne.UI.MainMenu.MainMenuScreen), "Awake")]
        public static class MainMenuAwakePatch
        {
            static void Postfix(ScheduleOne.UI.MainMenu.MainMenuScreen __instance)
            {
                Logger.LogInfo("MainMenuScreen.Awake patched");
                __instance.StartCoroutine(AddModButtonAfterDelay(__instance));
            }
            
            static System.Collections.IEnumerator AddModButtonAfterDelay(ScheduleOne.UI.MainMenu.MainMenuScreen menuScreen)
            {
                yield return null;
                yield return null;
                
                if (modButtonAdded)
                {
                    Logger.LogInfo("Mod button already added, skipping");
                    yield break;
                }
                
                Transform bankTransform = FindBankTransform(menuScreen.transform);
                
                if (bankTransform != null)
                {
                    AddModButtonToBank(bankTransform);
                    modButtonAdded = true;
                }
                else
                {
                    Logger.LogError("Could not find Bank transform in MainMenuScreen");
                }
            }
            
            static Transform FindBankTransform(Transform root)
            {
                Transform bank = root.Find("Home/Bank");
                if (bank != null)
                {
                    return bank;
                }
                
                foreach (Transform child in root.GetComponentsInChildren<Transform>(true))
                {
                    if (child.name == "Bank")
                    {
                        return child;
                    }
                }
                
                return null;
            }
            
            static void AddModButtonToBank(Transform bankTransform)
            {
                Transform settingsTransform = null;

                foreach (Transform child in bankTransform)
                {
                    if (child.name.Contains("Settings"))
                    {
                        settingsTransform = child;
                        break;
                    }
                }

                if (settingsTransform == null)
                {
                    Logger.LogError("Settings button not found, can't place mod button");
                    return;
                }

                // Create new button GameObject
                GameObject modmenuButton = new GameObject("Mods", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
                modmenuButton.transform.SetParent(bankTransform, false);

                // Match settings button visuals
                Image originalImage = settingsTransform.GetComponent<Image>();
                Image newImage = modmenuButton.GetComponent<Image>();
                newImage.sprite = originalImage.sprite;
                newImage.color = originalImage.color;
                newImage.type = originalImage.type;
                newImage.pixelsPerUnitMultiplier = originalImage.pixelsPerUnitMultiplier;

                // Position it similarly
                RectTransform modRect = modmenuButton.GetComponent<RectTransform>();
                RectTransform settingsRect = settingsTransform.GetComponent<RectTransform>();
                modRect.sizeDelta = settingsRect.sizeDelta;
                modRect.anchoredPosition = new Vector2(
                    settingsRect.anchoredPosition.x,
                    -173.0f
                );

                // Add ButtonScaler if needed (optional)
                modmenuButton.AddComponent<ScheduleOne.UI.ButtonScaler>();

                // Add text label
                GameObject textObj = new GameObject("Text", typeof(RectTransform), typeof(TextMeshProUGUI));
                textObj.transform.SetParent(modmenuButton.transform, false);
                RectTransform textRect = textObj.GetComponent<RectTransform>();
                textRect.anchorMin = Vector2.zero;
                textRect.anchorMax = Vector2.one;
                textRect.offsetMin = Vector2.zero;
                textRect.offsetMax = Vector2.zero;

                var tmp = textObj.GetComponent<TextMeshProUGUI>();
                tmp.text = "Mods";
                tmp.font = settingsTransform.GetComponentInChildren<TextMeshProUGUI>().font;
                tmp.fontSize = 28;
                tmp.alignment = TextAlignmentOptions.Center;
                tmp.color = Color.white;

                // Hook up the button
                Button button = modmenuButton.GetComponent<Button>();
                button.onClick.AddListener(() => {
                    ShowModMenu();
                });

                modmenuButton.SetActive(true);
                Logger.LogInfo("Custom mod button created successfully");
            }
        }
        
        static void ShowModMenu()
        {
            Logger.LogInfo("Mod menu button clicked!");
            
            // First, find the current active menu screen
            var currentScreen = GameObject.FindObjectOfType<ScheduleOne.UI.MainMenu.MainMenuScreen>();
            if (currentScreen != null && currentScreen.IsOpen)
            {
                // Close the current screen without opening its previous screen
                Logger.LogInfo($"Closing current menu screen: {currentScreen.name}");
                currentScreen.Close(false);
                
                // Small delay to allow the animation to start
                ModMenuManager.Instance.ShowModMenuDelayed(0.1f);
            }
            else
            {
                // If no screen is open or we can't find it, just show the mod menu directly
                ModMenuManager.Instance.ShowModMenu();
            }
        }
    }
}