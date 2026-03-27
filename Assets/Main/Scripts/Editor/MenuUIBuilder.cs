using UnityEngine;
using UnityEngine.UI;
using UnityEditor;
using TMPro;

// ================================================================
// DARKHOME — Menu UI Builder
// Cách dùng: Menu bar Unity → DarkHome → Build Pause Menu UI
//                                       → Build Main Menu UI
// Chạy ở scene tương ứng trước khi build.
// ================================================================

namespace TheProject.Editor
{
    public static class MenuUIBuilder
    {
        // ------ Palette ------
        private static readonly Color COL_BG_PANEL    = new Color(0.04f, 0.03f, 0.03f, 0.92f);
        private static readonly Color COL_TEXT         = new Color(0.91f, 0.87f, 0.82f, 1f);   // trắng ngà
        private static readonly Color COL_BTN_NORMAL   = new Color(0.12f, 0.10f, 0.10f, 1f);
        private static readonly Color COL_BTN_HOVER    = new Color(0.55f, 0.10f, 0.10f, 1f);   // đỏ máu
        private static readonly Color COL_BTN_CLICK    = new Color(0.35f, 0.05f, 0.05f, 1f);
        private static readonly Color COL_ACCENT_LINE  = new Color(0.55f, 0.10f, 0.10f, 1f);
        private static readonly Color COL_TITLE        = new Color(0.95f, 0.90f, 0.85f, 1f);

        // ================================================================
        //  PAUSE MENU
        // ================================================================
        [MenuItem("DarkHome/Build Pause Menu UI")]
        public static void BuildPauseMenuUI()
        {
            // --- Canvas ---
            GameObject canvasGO = CreateCanvas("Pause Canvas", 10);
            Canvas canvas = canvasGO.GetComponent<Canvas>();

            // --- Backdrop (fullscreen dim) ---
            GameObject backdrop = CreateImage(canvasGO, "Backdrop",
                new Color(0f, 0f, 0f, 0.65f),
                Vector2.zero, new Vector2(1920, 1080));
            StretchFull(backdrop.GetComponent<RectTransform>());

            // --- Center Panel ---
            GameObject panel = CreateImage(canvasGO, "PausePanel",
                COL_BG_PANEL,
                Vector2.zero, new Vector2(380, 480));
            Center(panel.GetComponent<RectTransform>());

            // --- Accent top line ---
            GameObject accent = CreateImage(panel, "AccentLine",
                COL_ACCENT_LINE,
                new Vector2(0, 195), new Vector2(300, 2));
            Center(accent.GetComponent<RectTransform>());
            accent.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 165);

            // --- Title ---
            GameObject title = CreateTMP(panel, "Title_PAUSED", "PAUSED",
                36, FontStyles.Bold, COL_TITLE,
                new Vector2(0, 145), new Vector2(320, 50));

            // --- Buttons ---
            CreateMenuButton(panel, "Btn_Resume",     "RESUME",      new Vector2(0,  60));
            CreateMenuButton(panel, "Btn_Settings",   "SETTINGS",    new Vector2(0,  -20));
            CreateMenuButton(panel, "Btn_QuitToMenu", "QUIT TO MENU",new Vector2(0, -100));

            // --- Accent bottom line ---
            GameObject accentB = CreateImage(panel, "AccentLineBottom",
                COL_ACCENT_LINE,
                new Vector2(0, -155), new Vector2(300, 2));
            Center(accentB.GetComponent<RectTransform>());
            accentB.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -155);

            // --- Add PauseMenuController ---
            PauseMenuController ctrl = canvasGO.AddComponent<PauseMenuController>();
            // Tự gán _pausePanel qua SerializedObject
            SerializedObject so = new SerializedObject(ctrl);
            so.FindProperty("_pausePanel").objectReferenceValue = panel;
            so.ApplyModifiedProperties();

            // Tắt panel lúc đầu
            panel.SetActive(false);

            // Bind buttons
            BindButton(panel, "Btn_Resume",     ctrl, "OnResumeClicked");
            BindButton(panel, "Btn_Settings",   ctrl, "OnSettingsClicked");
            BindButton(panel, "Btn_QuitToMenu", ctrl, "OnQuitToMenuClicked");

            Selection.activeGameObject = canvasGO;
            Debug.Log("[DarkHome] ✅ Pause Menu UI đã được tạo! Kéo 'PausePanel' vào UIManager._pausePanel.");
        }

        // ================================================================
        //  MAIN MENU
        // ================================================================
        [MenuItem("DarkHome/Build Main Menu UI")]
        public static void BuildMainMenuUI()
        {
            // --- Canvas ---
            GameObject canvasGO = CreateCanvas("MainMenu Canvas", 5);

            // --- Background fullscreen ---
            GameObject bg = CreateImage(canvasGO, "Background",
                new Color(0.02f, 0.02f, 0.02f, 1f),
                Vector2.zero, new Vector2(1920, 1080));
            StretchFull(bg.GetComponent<RectTransform>());

            // --- Left dark vignette panel ---
            GameObject sidePanel = CreateImage(canvasGO, "SidePanel",
                new Color(0.03f, 0.02f, 0.02f, 0.90f),
                Vector2.zero, new Vector2(500, 1080));
            RectTransform sidePanelRT = sidePanel.GetComponent<RectTransform>();
            sidePanelRT.anchorMin = new Vector2(0, 0);
            sidePanelRT.anchorMax = new Vector2(0, 1);
            sidePanelRT.pivot     = new Vector2(0, 0.5f);
            sidePanelRT.offsetMin = Vector2.zero;
            sidePanelRT.offsetMax = new Vector2(500, 0);

            // --- Game title ---
            GameObject gameTitle = CreateTMP(sidePanel, "GameTitle", "DARKHOME",
                52, FontStyles.Bold, COL_TITLE,
                new Vector2(250, 0), new Vector2(400, 70));
            RectTransform titleRT = gameTitle.GetComponent<RectTransform>();
            titleRT.anchorMin = new Vector2(0, 1);
            titleRT.anchorMax = new Vector2(0, 1);
            titleRT.pivot     = new Vector2(0, 1);
            titleRT.anchoredPosition = new Vector2(50, -100);
            titleRT.sizeDelta = new Vector2(400, 70);

            // --- Subtitle ---
            GameObject subtitle = CreateTMP(sidePanel, "Subtitle", "A Horror Experience",
                16, FontStyles.Italic, new Color(0.6f, 0.5f, 0.5f, 1f),
                Vector2.zero, new Vector2(400, 30));
            RectTransform subRT = subtitle.GetComponent<RectTransform>();
            subRT.anchorMin = new Vector2(0, 1);
            subRT.anchorMax = new Vector2(0, 1);
            subRT.pivot     = new Vector2(0, 1);
            subRT.anchoredPosition = new Vector2(55, -165);
            subRT.sizeDelta = new Vector2(400, 30);

            // --- Accent line ---
            GameObject accent = CreateImage(sidePanel, "AccentLine",
                COL_ACCENT_LINE, Vector2.zero, new Vector2(300, 2));
            RectTransform accentRT = accent.GetComponent<RectTransform>();
            accentRT.anchorMin = new Vector2(0, 1);
            accentRT.anchorMax = new Vector2(0, 1);
            accentRT.pivot     = new Vector2(0, 1);
            accentRT.anchoredPosition = new Vector2(50, -200);
            accentRT.sizeDelta = new Vector2(300, 2);

            // --- Buttons ---
            float startY = -240f;
            float gap = -75f;
            CreateMainMenuButton(sidePanel, "Btn_Continue",   "CONTINUE",    new Vector2(50, startY + gap * 0));
            CreateMainMenuButton(sidePanel, "Btn_NewGame",    "NEW GAME",    new Vector2(50, startY + gap * 1));
            CreateMainMenuButton(sidePanel, "Btn_Settings",   "SETTINGS",    new Vector2(50, startY + gap * 2));
            CreateMainMenuButton(sidePanel, "Btn_Exit",       "EXIT",        new Vector2(50, startY + gap * 3));

            // --- Version label ---
            GameObject version = CreateTMP(sidePanel, "VersionLabel", "v0.1 — Demo",
                11, FontStyles.Normal, new Color(0.4f, 0.35f, 0.35f, 1f),
                Vector2.zero, new Vector2(300, 25));
            RectTransform verRT = version.GetComponent<RectTransform>();
            verRT.anchorMin = new Vector2(0, 0);
            verRT.anchorMax = new Vector2(0, 0);
            verRT.pivot     = new Vector2(0, 0);
            verRT.anchoredPosition = new Vector2(50, 30);
            verRT.sizeDelta = new Vector2(300, 25);

            // --- Add MainMenuController ---
            MainMenuController ctrl = canvasGO.AddComponent<MainMenuController>();
            SerializedObject so = new SerializedObject(ctrl);
            so.FindProperty("_btnContinue").objectReferenceValue   = sidePanel.transform.Find("Btn_Continue")?.GetComponent<Button>();
            so.FindProperty("_btnNewGame").objectReferenceValue    = sidePanel.transform.Find("Btn_NewGame")?.GetComponent<Button>();
            so.FindProperty("_btnSettings").objectReferenceValue   = sidePanel.transform.Find("Btn_Settings")?.GetComponent<Button>();
            so.FindProperty("_btnExit").objectReferenceValue       = sidePanel.transform.Find("Btn_Exit")?.GetComponent<Button>();
            so.ApplyModifiedProperties();

            // Bind buttons
            BindButton(sidePanel, "Btn_Continue",  ctrl, "OnContinueClicked");
            BindButton(sidePanel, "Btn_NewGame",   ctrl, "OnNewGameClicked");
            BindButton(sidePanel, "Btn_Settings",  ctrl, "OnSettingsClicked");
            BindButton(sidePanel, "Btn_Exit",      ctrl, "OnExitClicked");

            Selection.activeGameObject = canvasGO;
            Debug.Log("[DarkHome] ✅ Main Menu UI đã được tạo!");
        }

        // ================================================================
        //  HELPERS
        // ================================================================

        private static GameObject CreateCanvas(string name, int sortOrder)
        {
            GameObject go = new GameObject(name);
            Canvas c = go.AddComponent<Canvas>();
            c.renderMode = RenderMode.ScreenSpaceOverlay;
            c.sortingOrder = sortOrder;
            go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            ((CanvasScaler)go.GetComponent<CanvasScaler>()).referenceResolution = new Vector2(1920, 1080);
            go.AddComponent<GraphicRaycaster>();
            Undo.RegisterCreatedObjectUndo(go, $"Create {name}");
            return go;
        }

        private static GameObject CreateImage(GameObject parent, string name, Color color,
                                               Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image));
            go.transform.SetParent(parent.transform, false);
            Image img = go.GetComponent<Image>();
            img.color = color;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return go;
        }

        private static GameObject CreateTMP(GameObject parent, string name, string text,
                                             float fontSize, FontStyles style, Color color,
                                             Vector2 anchoredPos, Vector2 size)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(TextMeshProUGUI));
            go.transform.SetParent(parent.transform, false);
            TextMeshProUGUI tmp = go.GetComponent<TextMeshProUGUI>();
            tmp.text = text;
            tmp.fontSize = fontSize;
            tmp.fontStyle = style;
            tmp.color = color;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = anchoredPos;
            rt.sizeDelta = size;
            return go;
        }

        // Button kiểu center (Pause Menu)
        private static GameObject CreateMenuButton(GameObject parent, string name,
                                                    string label, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);

            Image img = go.GetComponent<Image>();
            img.color = COL_BTN_NORMAL;

            Button btn = go.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = COL_BTN_NORMAL;
            cb.highlightedColor = COL_BTN_HOVER;
            cb.pressedColor     = COL_BTN_CLICK;
            cb.selectedColor    = COL_BTN_NORMAL;
            btn.colors = cb;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(280, 48);

            // Label
            GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            TextMeshProUGUI tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 15;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = COL_TEXT;
            tmp.alignment = TextAlignmentOptions.Center;
            RectTransform labelRT = labelGO.GetComponent<RectTransform>();
            StretchFull(labelRT);

            return go;
        }

        // Button kiểu left-align (Main Menu)
        private static GameObject CreateMainMenuButton(GameObject parent, string name,
                                                        string label, Vector2 pos)
        {
            GameObject go = new GameObject(name, typeof(RectTransform), typeof(Image), typeof(Button));
            go.transform.SetParent(parent.transform, false);

            Image img = go.GetComponent<Image>();
            img.color = new Color(0, 0, 0, 0); // transparent

            Button btn = go.GetComponent<Button>();
            ColorBlock cb = btn.colors;
            cb.normalColor      = new Color(0, 0, 0, 0);
            cb.highlightedColor = new Color(0.55f, 0.10f, 0.10f, 0.15f);
            cb.pressedColor     = new Color(0.35f, 0.05f, 0.05f, 0.30f);
            cb.selectedColor    = new Color(0, 0, 0, 0);
            btn.colors = cb;

            RectTransform rt = go.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 1);
            rt.anchorMax = new Vector2(0, 1);
            rt.pivot     = new Vector2(0, 1);
            rt.anchoredPosition = pos;
            rt.sizeDelta = new Vector2(350, 55);

            // Label
            GameObject labelGO = new GameObject("Label", typeof(RectTransform), typeof(TextMeshProUGUI));
            labelGO.transform.SetParent(go.transform, false);
            TextMeshProUGUI tmp = labelGO.GetComponent<TextMeshProUGUI>();
            tmp.text = label;
            tmp.fontSize = 18;
            tmp.fontStyle = FontStyles.Bold;
            tmp.color = COL_TEXT;
            tmp.alignment = TextAlignmentOptions.MidlineLeft;
            RectTransform labelRT = labelGO.GetComponent<RectTransform>();
            StretchFull(labelRT);
            labelRT.offsetMin = new Vector2(15, 0);

            return go;
        }

        private static void StretchFull(RectTransform rt)
        {
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }

        private static void Center(RectTransform rt)
        {
            rt.anchorMin = new Vector2(0.5f, 0.5f);
            rt.anchorMax = new Vector2(0.5f, 0.5f);
            rt.pivot     = new Vector2(0.5f, 0.5f);
        }

        private static void BindButton(GameObject parent, string btnName,
                                        MonoBehaviour target, string methodName)
        {
            Transform t = parent.transform.Find(btnName);
            if (t == null) return;
            Button btn = t.GetComponent<Button>();
            if (btn == null) return;

            UnityEditor.Events.UnityEventTools.AddPersistentListener(
                btn.onClick,
                (UnityEngine.Events.UnityAction)
                System.Delegate.CreateDelegate(
                    typeof(UnityEngine.Events.UnityAction),
                    target,
                    target.GetType().GetMethod(methodName)));
        }
    }
}
