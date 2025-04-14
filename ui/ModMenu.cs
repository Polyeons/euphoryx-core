using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;
using System.Collections.Generic;
using BepInEx.Logging;

namespace tech.polyeons.euphoryx.ui
{
    public class ModMenuManager : MonoBehaviour
    {
        private static ModMenuManager _instance;
        public static ModMenuManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    try
                    {
                        GameObject go = new GameObject("ModMenuManager");
                        DontDestroyOnLoad(go);
                        _instance = go.AddComponent<ModMenuManager>();
                        Logger.LogInfo("ModMenuManager instance created");
                    }
                    catch (Exception ex)
                    {
                        Logger.LogError($"Failed to create ModMenuManager instance: {ex.Message}");
                    }
                }
                return _instance;
            }
        }

        private static ManualLogSource Logger = BepInEx.Logging.Logger.CreateLogSource("Euphoryx.ModMenuManager");
        
        private GameObject modMenuCanvas;
        private GameObject modListPanel;
        private GameObject modDetailsPanel;
        
        private List<ModEntry> registeredMods = new List<ModEntry>();
        
        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }
            
            _instance = this;
            DontDestroyOnLoad(gameObject);
            
            CreateModMenuUI();
        }
        
        private void CreateModMenuUI()
        {
            try
            {
                // Create canvas
                modMenuCanvas = new GameObject("ModMenuCanvas");
                modMenuCanvas.transform.SetParent(transform);
                modMenuCanvas.transform.localScale = Vector3.one;
                
                try {
                    Canvas canvas = modMenuCanvas.AddComponent<Canvas>();
                    canvas.renderMode = RenderMode.ScreenSpaceOverlay;
                    canvas.sortingOrder = 100;
                    
                    CanvasScaler scaler = modMenuCanvas.AddComponent<CanvasScaler>();
                    scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
                    scaler.referenceResolution = new Vector2(1920, 1080);
                    
                    modMenuCanvas.AddComponent<GraphicRaycaster>();
                }
                catch (Exception ex)
                {
                    Logger.LogError($"Failed to create Canvas components: {ex.Message}. Make sure Unity UI is properly referenced.");
                    return;
                }
                
                // Create background panel
                GameObject backgroundPanel = CreatePanel(modMenuCanvas.transform, "BackgroundPanel");
                RectTransform bgRect = backgroundPanel.GetComponent<RectTransform>();
                bgRect.anchorMin = Vector2.zero;
                bgRect.anchorMax = Vector2.one;
                bgRect.offsetMin = Vector2.zero;
                bgRect.offsetMax = Vector2.zero;
                
                Image bgImage = backgroundPanel.GetComponent<Image>();
                bgImage.color = new Color(0, 0, 0, 0.8f);
                
                // Create main container
                GameObject mainContainer = CreatePanel(backgroundPanel.transform, "MainContainer");
                RectTransform mainRect = mainContainer.GetComponent<RectTransform>();
                mainRect.anchorMin = new Vector2(0.1f, 0.1f);
                mainRect.anchorMax = new Vector2(0.9f, 0.9f);
                mainRect.offsetMin = Vector2.zero;
                mainRect.offsetMax = Vector2.zero;
                
                Image mainImage = mainContainer.GetComponent<Image>();
                mainImage.color = new Color(0.1f, 0.1f, 0.1f, 0.9f);
                
                // Create title
                GameObject titleObj = new GameObject("Title");
                titleObj.transform.SetParent(mainContainer.transform);
                
                // Replace Text with TextMeshProUGUI
                TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
                titleText.text = "MODS";
                titleText.fontSize = 36;
                titleText.alignment = TextAlignmentOptions.Center;
                titleText.color = Color.white;
                
                RectTransform titleRect = titleObj.GetComponent<RectTransform>();
                titleRect.anchorMin = new Vector2(0, 1);
                titleRect.anchorMax = new Vector2(1, 1);
                titleRect.pivot = new Vector2(0.5f, 1);
                titleRect.sizeDelta = new Vector2(0, 60);
                titleRect.anchoredPosition = new Vector2(0, 0);
                
                // Create close button
                GameObject closeButton = CreateButton(mainContainer.transform, "CloseButton", "X");
                RectTransform closeRect = closeButton.GetComponent<RectTransform>();
                closeRect.anchorMin = new Vector2(1, 1);
                closeRect.anchorMax = new Vector2(1, 1);
                closeRect.pivot = new Vector2(1, 1);
                closeRect.sizeDelta = new Vector2(50, 50);
                closeRect.anchoredPosition = new Vector2(0, 0);
                
                Button closeButtonComponent = closeButton.GetComponent<Button>();
                closeButtonComponent.onClick.AddListener(HideModMenu);
                
                // Create content area
                GameObject contentArea = CreatePanel(mainContainer.transform, "ContentArea");
                RectTransform contentRect = contentArea.GetComponent<RectTransform>();
                contentRect.anchorMin = new Vector2(0, 0);
                contentRect.anchorMax = new Vector2(1, 1);
                contentRect.offsetMin = new Vector2(10, 10);
                contentRect.offsetMax = new Vector2(-10, -70);
                
                // Create mod list panel (left side)
                modListPanel = CreatePanel(contentArea.transform, "ModListPanel");
                RectTransform modListRect = modListPanel.GetComponent<RectTransform>();
                modListRect.anchorMin = new Vector2(0, 0);
                modListRect.anchorMax = new Vector2(0.3f, 1);
                modListRect.offsetMin = Vector2.zero;
                modListRect.offsetMax = Vector2.zero;
                
                // Create mod details panel (right side)
                modDetailsPanel = CreatePanel(contentArea.transform, "ModDetailsPanel");
                RectTransform modDetailsRect = modDetailsPanel.GetComponent<RectTransform>();
                modDetailsRect.anchorMin = new Vector2(0.32f, 0);
                modDetailsRect.anchorMax = new Vector2(1, 1);
                modDetailsRect.offsetMin = Vector2.zero;
                modDetailsRect.offsetMax = Vector2.zero;
                
                // Hide the menu initially
                modMenuCanvas.SetActive(false);
                
                Logger.LogInfo("Mod menu UI created successfully");
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error creating mod menu UI: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        private GameObject CreatePanel(Transform parent, string name)
        {
            GameObject panel = new GameObject(name);
            panel.transform.SetParent(parent);
            
            RectTransform rect = panel.AddComponent<RectTransform>();
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.offsetMin = Vector2.zero;
            rect.offsetMax = Vector2.zero;
            
            Image image = panel.AddComponent<Image>();
            // Use a white sprite for the image
            image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            
            return panel;
        }
        
        private GameObject CreateButton(Transform parent, string name, string text)
        {
            GameObject button = new GameObject(name);
            button.transform.SetParent(parent);
            button.transform.localScale = Vector3.one;
            
            RectTransform rect = button.AddComponent<RectTransform>();
            Image image = button.AddComponent<Image>();
            // Fix the syntax error here - remove the "Image" before image.sprite
            image.sprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            Button buttonComponent = button.AddComponent<Button>();
            
            // Set up button colors
            ColorBlock colors = buttonComponent.colors;
            colors.normalColor = new Color(0.2f, 0.2f, 0.2f);
            colors.highlightedColor = new Color(0.3f, 0.3f, 0.3f);
            colors.pressedColor = new Color(0.1f, 0.1f, 0.1f);
            buttonComponent.colors = colors;
            
            // Add text
            GameObject textObj = new GameObject("Text");
            textObj.transform.SetParent(button.transform);
            
            // Replace Text with TextMeshProUGUI
            TextMeshProUGUI textComponent = textObj.AddComponent<TextMeshProUGUI>();
            textComponent.text = text;
            textComponent.fontSize = 24;
            textComponent.alignment = TextAlignmentOptions.Center;
            textComponent.color = Color.white;
            
            RectTransform textRect = textObj.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;
            
            return button;
        }
        
        // Public API methods
        
        public void ShowModMenuDelayed(float delay)
        {
            StartCoroutine(ShowModMenuAfterDelay(delay));
        }
        
        private System.Collections.IEnumerator ShowModMenuAfterDelay(float delay)
        {
            yield return new WaitForSeconds(delay);
            ShowModMenu();
        }
        
        public void ShowModMenu()
        {
            if (modMenuCanvas != null)
            {
                modMenuCanvas.SetActive(true);
                Logger.LogInfo("Mod menu shown");
            }
            else
            {
                Logger.LogError("Cannot show mod menu - canvas is null");
            }
        }
        
        public void HideModMenu()
        {
            if (modMenuCanvas != null)
            {
                modMenuCanvas.SetActive(false);
                Logger.LogInfo("Mod menu hidden");
            }
        }
        
        public void RegisterMod(string modId, string modName, string modVersion, string modDescription, Action onSettingsClicked = null)
        {
            ModEntry entry = new ModEntry
            {
                Id = modId,
                Name = modName,
                Version = modVersion,
                Description = modDescription,
                OnSettingsClicked = onSettingsClicked
            };
            
            // Check if mod is already registered
            int existingIndex = registeredMods.FindIndex(m => m.Id == modId);
            if (existingIndex >= 0)
            {
                registeredMods[existingIndex] = entry;
            }
            else
            {
                registeredMods.Add(entry);
            }
            
            Logger.LogInfo($"Registered mod: {modName} v{modVersion}");
        }
        
        private void RefreshModList()
        {
            try
            {
                if (modListPanel == null)
                {
                    Logger.LogError("modListPanel is null, cannot refresh mod list");
                    return;
                }
                
                // Clear existing mod entries
                foreach (Transform child in modListPanel.transform)
                {
                    Destroy(child.gameObject);
                }
                
                // Create scrollview if needed
                GameObject scrollView = modListPanel.transform.Find("ScrollView")?.gameObject;
                if (scrollView == null)
                {
                    scrollView = new GameObject("ScrollView");
                    scrollView.transform.SetParent(modListPanel.transform);
                    
                    ScrollRect scrollRect = scrollView.AddComponent<ScrollRect>();
                    RectTransform scrollRectTransform = scrollView.GetComponent<RectTransform>();
                    scrollRectTransform.anchorMin = Vector2.zero;
                    scrollRectTransform.anchorMax = Vector2.one;
                    scrollRectTransform.offsetMin = Vector2.zero;
                    scrollRectTransform.offsetMax = Vector2.zero;
                    
                    // Create viewport
                    GameObject viewport = new GameObject("Viewport");
                    viewport.transform.SetParent(scrollView.transform);
                    
                    RectTransform viewportRect = viewport.AddComponent<RectTransform>();
                    viewportRect.anchorMin = Vector2.zero;
                    viewportRect.anchorMax = Vector2.one;
                    viewportRect.offsetMin = Vector2.zero;
                    viewportRect.offsetMax = Vector2.zero;
                    
                    Image viewportImage = viewport.AddComponent<Image>();
                    viewportImage.color = new Color(0.1f, 0.1f, 0.1f, 0.5f);
                    
                    Mask viewportMask = viewport.AddComponent<Mask>();
                    viewportMask.showMaskGraphic = false;
                    
                    // Create content
                    GameObject content = new GameObject("Content");
                    content.transform.SetParent(viewport.transform);
                    
                    RectTransform contentRect = content.AddComponent<RectTransform>();
                    contentRect.anchorMin = new Vector2(0, 1);
                    contentRect.anchorMax = new Vector2(1, 1);
                    contentRect.pivot = new Vector2(0.5f, 1);
                    contentRect.sizeDelta = new Vector2(0, 0);
                    
                    VerticalLayoutGroup layout = content.AddComponent<VerticalLayoutGroup>();
                    layout.padding = new RectOffset(5, 5, 5, 5);
                    layout.spacing = 5;
                    layout.childForceExpandWidth = true;
                    layout.childForceExpandHeight = false;
                    layout.childControlWidth = true;
                    layout.childControlHeight = false;
                    
                    ContentSizeFitter fitter = content.AddComponent<ContentSizeFitter>();
                    fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
                    
                    // Set up scroll rect references
                    scrollRect.viewport = viewportRect;
                    scrollRect.content = contentRect;
                    scrollRect.horizontal = false;
                    scrollRect.vertical = true;
                }
                
                // Get content transform
                Transform contentTransform = scrollView.transform.Find("Viewport/Content");
                
                // Add mod entries
                foreach (ModEntry mod in registeredMods)
                {
                    GameObject modEntryObj = CreateModEntryUI(mod);
                    modEntryObj.transform.SetParent(contentTransform);
                }
            }
            catch (Exception ex)
            {
                Logger.LogError($"Error refreshing mod list: {ex.Message}\n{ex.StackTrace}");
            }
        }
        
        private GameObject CreateModEntryUI(ModEntry mod)
        {
            GameObject entryObj = new GameObject(mod.Id);
            entryObj.transform.localScale = Vector3.one; // Add this line
            
            RectTransform rect = entryObj.AddComponent<RectTransform>();
            rect.sizeDelta = new Vector2(0, 60);
            
            Image background = entryObj.AddComponent<Image>();
            background.color = new Color(0.2f, 0.2f, 0.2f, 0.8f);
            
            Button button = entryObj.AddComponent<Button>();
            button.onClick.AddListener(() => ShowModDetails(mod));
            
            // Add mod name text
            GameObject nameObj = new GameObject("Name");
            nameObj.transform.SetParent(entryObj.transform);
            
            // Replace Text with TextMeshProUGUI
            TextMeshProUGUI nameText = nameObj.AddComponent<TextMeshProUGUI>();
            nameText.text = mod.Name;
            nameText.fontSize = 18;
            nameText.alignment = TextAlignmentOptions.Left;
            nameText.color = Color.white;
            
            RectTransform nameRect = nameObj.GetComponent<RectTransform>();
            nameRect.anchorMin = new Vector2(0, 0.5f);
            nameRect.anchorMax = new Vector2(1, 0.5f);
            nameRect.pivot = new Vector2(0, 0.5f);
            nameRect.offsetMin = new Vector2(10, -20);
            nameRect.offsetMax = new Vector2(-10, 10);
            
            // Add version text
            GameObject versionObj = new GameObject("Version");
            versionObj.transform.SetParent(entryObj.transform);
            
            // Change Text to TextMeshProUGUI
            TextMeshProUGUI versionText = versionObj.AddComponent<TextMeshProUGUI>();
            versionText.text = $"v{mod.Version}";
            // Remove font assignment
            versionText.fontSize = 14;
            versionText.alignment = TextAlignmentOptions.Left;
            versionText.color = new Color(0.7f, 0.7f, 0.7f);
            
            RectTransform versionRect = versionObj.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(0, 0.5f);
            versionRect.anchorMax = new Vector2(1, 0.5f);
            versionRect.pivot = new Vector2(0, 0.5f);
            versionRect.offsetMin = new Vector2(10, -40);
            versionRect.offsetMax = new Vector2(-10, -20);
            
            return entryObj;
        }
        
        private void ShowModDetails(ModEntry mod)
        {
            // Clear existing details
            foreach (Transform child in modDetailsPanel.transform)
            {
                Destroy(child.gameObject);
            }
            
            // Create mod details UI
            GameObject titleObj = new GameObject("Title");
            titleObj.transform.SetParent(modDetailsPanel.transform);
            
            // Replace Text with TextMeshProUGUI
            TextMeshProUGUI titleText = titleObj.AddComponent<TextMeshProUGUI>();
            titleText.text = mod.Name;
            titleText.fontSize = 28;
            titleText.alignment = TextAlignmentOptions.Left;
            titleText.color = Color.white;
            
            RectTransform titleRect = titleObj.GetComponent<RectTransform>();
            titleRect.anchorMin = new Vector2(0, 1);
            titleRect.anchorMax = new Vector2(1, 1);
            titleRect.pivot = new Vector2(0, 1);
            titleRect.sizeDelta = new Vector2(0, 40);
            titleRect.anchoredPosition = new Vector2(10, 0);
            
            // Version
            GameObject versionObj = new GameObject("Version");
            versionObj.transform.SetParent(modDetailsPanel.transform);
            
            // Change Text to TextMeshProUGUI
            TextMeshProUGUI versionText = versionObj.AddComponent<TextMeshProUGUI>();
            versionText.text = $"Version: {mod.Version}";
            // Remove font assignment
            versionText.fontSize = 18;
            versionText.alignment = TextAlignmentOptions.Left;
            versionText.color = new Color(0.7f, 0.7f, 0.7f);
            
            RectTransform versionRect = versionObj.GetComponent<RectTransform>();
            versionRect.anchorMin = new Vector2(0, 1);
            versionRect.anchorMax = new Vector2(1, 1);
            versionRect.pivot = new Vector2(0, 1);
            versionRect.sizeDelta = new Vector2(0, 30);
            versionRect.anchoredPosition = new Vector2(10, -40);
            
            // Description
            GameObject descObj = new GameObject("Description");
            descObj.transform.SetParent(modDetailsPanel.transform);
            
            // Change Text to TextMeshProUGUI
            TextMeshProUGUI descText = descObj.AddComponent<TextMeshProUGUI>();
            descText.text = mod.Description;
            // Remove font assignment
            descText.fontSize = 16;
            descText.alignment = TextAlignmentOptions.TopLeft;
            descText.color = Color.white;
            
            RectTransform descRect = descObj.GetComponent<RectTransform>();
            descRect.anchorMin = new Vector2(0, 0.5f);
            descRect.anchorMax = new Vector2(1, 0.9f);
            descRect.offsetMin = new Vector2(10, 0);
            descRect.offsetMax = new Vector2(-10, -80);
            
            // Settings button (if available)
            if (mod.OnSettingsClicked != null)
            {
                GameObject settingsButton = CreateButton(modDetailsPanel.transform, "SettingsButton", "Settings");
                RectTransform settingsRect = settingsButton.GetComponent<RectTransform>();
                settingsRect.anchorMin = new Vector2(0, 0);
                settingsRect.anchorMax = new Vector2(0, 0);
                settingsRect.pivot = new Vector2(0, 0);
                settingsRect.sizeDelta = new Vector2(150, 40);
                settingsRect.anchoredPosition = new Vector2(10, 10);
                
                Button settingsButtonComponent = settingsButton.GetComponent<Button>();
                settingsButtonComponent.onClick.AddListener(() => mod.OnSettingsClicked());
            }
        }
    }
    
    public class ModEntry
    {
        public string Id { get; set; }
        public string Name { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public Action OnSettingsClicked { get; set; }
    }
}