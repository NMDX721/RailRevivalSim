#if UNITY_EDITOR
using System;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public static class RailRevivalMvpSetup
{
    private const string ScenePath = "Assets/Scenes/StationSlice_V1.unity";

    [MenuItem("RailRevival/MVP/Setup StationSlice_V1")]
    public static void SetupStationSliceV1()
    {
        Scene scene;
        try
        {
            scene = EditorSceneManager.OpenScene(ScenePath, OpenSceneMode.Single);
        }
        catch (Exception ex)
        {
            Debug.LogError($"Failed to open scene at '{ScenePath}'. {ex.Message}");
            return;
        }

        var canvas = EnsureCanvas(scene);
        EnsureEventSystem(scene);
        EnsureMainCamera(scene);

        // Fix common YAML-corruption artifact: root RectTransform scale accidentally saved as (0,0,0).
        if (canvas != null)
        {
            var canvasRt = canvas.GetComponent<RectTransform>();
            if (canvasRt != null && canvasRt.localScale == Vector3.zero)
            {
                canvasRt.localScale = Vector3.one;
            }
        }

        var topBar = EnsureUiPanel(canvas.transform, "TopBar");
        var rightPanel = EnsureUiPanel(canvas.transform, "RightPanel");
        var bottomActions = EnsureUiPanel(canvas.transform, "BottomActions");

        LayoutRootPanels(canvas.GetComponent<RectTransform>(), topBar.GetComponent<RectTransform>(), rightPanel.GetComponent<RectTransform>(), bottomActions.GetComponent<RectTransform>());

        // Ensure predictable draw order: background panels behind, action surfaces above.
        rightPanel.transform.SetAsFirstSibling();
        var statusHints = FindChild(canvas.transform, "StatusHints");
        if (statusHints != null)
        {
            statusHints.SetActive(false);
            statusHints.transform.SetAsFirstSibling();
        }
        topBar.transform.SetAsLastSibling();
        bottomActions.transform.SetAsLastSibling();

        // Build UI content (idempotent).
        var uiRefs = EnsureUiContent(topBar.transform, rightPanel.transform, bottomActions.transform, canvas.transform);

        // Ensure Managers + scripts.
        var managers = EnsureRoot(scene, "Managers");
        var uiManagerGo = EnsureChild(managers.transform, "UIManager");
        var audioManagerGo = EnsureChild(managers.transform, "AudioManager");
        var gameManagerGo = EnsureChild(managers.transform, "GameManager");

        var uiManager = EnsureComponent<UIManager>(uiManagerGo);
        var audioManager = EnsureComponent<AudioManager>(audioManagerGo);
        _ = EnsureComponent<Transform>(gameManagerGo);

        // Audio clips.
        TryAssignAudioClips(audioManager);

        // Buttons -> UIManager via ButtonController.
        foreach (var btn in uiRefs.BottomButtons)
        {
            var controller = EnsureComponent<ButtonController>(btn.GameObject);
            controller.uiManager = uiManager;
            controller.buttonType = btn.Type;
        }

        // Bind UIManager references.
        AssignUiManagerReferences(uiManager, uiRefs);

        // VisualAssetBinder: bind sprites to buttons + train placeholder sprite.
        var binder = EnsureComponent<VisualAssetBinder>(managers);
        binder.scheduleButton = uiRefs.GetButton("schedule");
        binder.staffButton = uiRefs.GetButton("staff");
        binder.maintenanceButton = uiRefs.GetButton("maintenance");
        binder.stationServiceButton = uiRefs.GetButton("stationService");
        binder.externalAffairsButton = uiRefs.GetButton("externalAffairs");
        binder.endDayButton = uiRefs.GetButton("endDay");
        binder.trainObject = FindFirstInScene(scene, "TrainPlaceholder_Legacy")
            ?? FindFirstInScene(scene, "TrainCandidate_01")
            ?? FindFirstInScene(scene, "Train");
        binder.tilemapObject = FindFirstInScene(scene, "Ground");

        EditorUtility.SetDirty(uiManager);
        EditorUtility.SetDirty(audioManager);
        EditorUtility.SetDirty(binder);

        EditorSceneManager.MarkSceneDirty(scene);
        EditorSceneManager.SaveScene(scene);

        Debug.Log("RailRevival MVP: StationSlice_V1 setup complete. Press Play to smoke-test the loop.");
    }

    private static Canvas EnsureCanvas(Scene scene)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var canvas = root.GetComponentInChildren<Canvas>(true);
            if (canvas != null)
            {
                // Ensure required components exist.
                _ = EnsureComponent<CanvasScaler>(canvas.gameObject);
                _ = EnsureComponent<GraphicRaycaster>(canvas.gameObject);
                return canvas;
            }
        }

        var canvasGo = new GameObject("Canvas", typeof(RectTransform), typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
        SceneManager.MoveGameObjectToScene(canvasGo, scene);
        var newCanvas = canvasGo.GetComponent<Canvas>();
        newCanvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGo.GetComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(800, 600);

        return newCanvas;
    }

    private static void EnsureEventSystem(Scene scene)
    {
        if (FindComponentInScene<EventSystem>(scene) != null)
        {
            return;
        }

        var eventSystemGo = new GameObject("EventSystem", typeof(EventSystem), typeof(StandaloneInputModule));
        SceneManager.MoveGameObjectToScene(eventSystemGo, scene);
    }

    private static void EnsureMainCamera(Scene scene)
    {
        if (FindComponentInScene<Camera>(scene) != null)
        {
            return;
        }

        var camGo = new GameObject("Main Camera", typeof(Camera), typeof(AudioListener));
        camGo.tag = "MainCamera";
        SceneManager.MoveGameObjectToScene(camGo, scene);

        var cam = camGo.GetComponent<Camera>();
        cam.orthographic = true;
        cam.orthographicSize = 5f;
        cam.clearFlags = CameraClearFlags.SolidColor;
        cam.backgroundColor = Color.black;
        camGo.transform.position = new Vector3(0f, 0f, -10f);
    }

    private static GameObject EnsureUiPanel(Transform canvas, string name)
    {
        var go = FindChild(canvas, name);
        if (go == null)
        {
            go = new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            go.transform.SetParent(canvas, false);
        }

        var img = EnsureComponent<Image>(go);
        img.color = new Color(1f, 1f, 1f, 0.15f);

        var rt = go.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        rt.localRotation = Quaternion.identity;

        return go;
    }

    private static void LayoutRootPanels(RectTransform canvas, RectTransform topBar, RectTransform rightPanel, RectTransform bottomActions)
    {
        if (canvas != null && canvas.localScale == Vector3.zero)
        {
            canvas.localScale = Vector3.one;
        }

        // Top bar (height 72)
        topBar.anchorMin = new Vector2(0f, 1f);
        topBar.anchorMax = new Vector2(1f, 1f);
        topBar.pivot = new Vector2(0.5f, 1f);
        topBar.sizeDelta = new Vector2(0f, 72f);
        topBar.anchoredPosition = Vector2.zero;

        // Bottom actions (height 96)
        bottomActions.anchorMin = new Vector2(0f, 0f);
        bottomActions.anchorMax = new Vector2(1f, 0f);
        bottomActions.pivot = new Vector2(0.5f, 0f);
        bottomActions.sizeDelta = new Vector2(0f, 96f);
        bottomActions.anchoredPosition = Vector2.zero;

        // Right panel (width 320), between top and bottom.
        rightPanel.anchorMin = new Vector2(1f, 0f);
        rightPanel.anchorMax = new Vector2(1f, 1f);
        rightPanel.pivot = new Vector2(1f, 0.5f);
        rightPanel.sizeDelta = new Vector2(320f, 0f);
        rightPanel.anchoredPosition = Vector2.zero;
    }

    private static UiRefs EnsureUiContent(Transform topBar, Transform rightPanel, Transform bottomActions, Transform canvas)
    {
        var font = GetRuntimeFont();
        var refs = new UiRefs();

        // TopBar texts.
        refs.DayText = EnsureText(topBar, "DayText", font, 16);
        refs.MoneyText = EnsureText(topBar, "MoneyText", font, 16);
        refs.TrustText = EnsureText(topBar, "TrustText", font, 16);
        refs.TrainConditionText = EnsureText(topBar, "TrainConditionText", font, 16);
        refs.ExpectedPassengersText = EnsureText(topBar, "ExpectedPassengersText", font, 16);
        refs.StatusText = EnsureText(topBar, "StatusText", font, 16);

        PositionTopBarTexts(refs);

        // RightPanel texts.
        refs.DescriptionText = EnsureText(rightPanel, "DescriptionText", font, 14, supportRichText: true);
        refs.BriefingText = EnsureText(rightPanel, "BriefingText", font, 13, supportRichText: true);
        refs.NoticesText = EnsureText(rightPanel, "NoticesText", font, 13, supportRichText: true);
        refs.TodosText = EnsureText(rightPanel, "TodosText", font, 13, supportRichText: true);
        PositionRightPanelTexts(refs);

        // Bottom buttons.
        refs.BottomButtons = new[]
        {
            EnsureButton(bottomActions, "Btn_Schedule", "发车", "schedule"),
            EnsureButton(bottomActions, "Btn_Staff", "人员", "staff"),
            EnsureButton(bottomActions, "Btn_Maintenance", "检修", "maintenance"),
            EnsureButton(bottomActions, "Btn_Service", "服务", "stationService"),
            EnsureButton(bottomActions, "Btn_External", "对外", "externalAffairs"),
            EnsureButton(bottomActions, "Btn_EndDay", "End Day", "endDay"),
        };
        PositionBottomButtons(refs.BottomButtons);

        // Overlay panels (created under Canvas root so RightPanel stays visible).
        var panelsRoot = EnsureChild(canvas, "MvpPanels");
        var panelsRootRt = EnsureComponent<RectTransform>(panelsRoot);
        panelsRootRt.anchorMin = new Vector2(0f, 0f);
        panelsRootRt.anchorMax = new Vector2(1f, 1f);
        panelsRootRt.pivot = new Vector2(0.5f, 0.5f);
        panelsRootRt.offsetMin = Vector2.zero;
        panelsRootRt.offsetMax = Vector2.zero;

        refs.SchedulePanel = EnsureOverlayPanel(panelsRoot.transform, "SchedulePanel", font, out refs.ScheduleTitle, out refs.ScheduleContent);
        refs.StaffPanel = EnsureOverlayPanel(panelsRoot.transform, "StaffPanel", font, out refs.StaffTitle, out refs.StaffContent);
        refs.MaintenancePanel = EnsureOverlayPanel(panelsRoot.transform, "MaintenancePanel", font, out refs.MaintenanceTitle, out refs.MaintenanceContent);
        refs.StationServicePanel = EnsureOverlayPanel(panelsRoot.transform, "StationServicePanel", font, out refs.StationServiceTitle, out refs.StationServiceContent);
        refs.ExternalAffairsPanel = EnsureOverlayPanel(panelsRoot.transform, "ExternalAffairsPanel", font, out refs.ExternalAffairsTitle, out refs.ExternalAffairsContent);
        refs.EndDayPanel = EnsureOverlayPanel(panelsRoot.transform, "EndDayPanel", font, out refs.EndDayTitle, out refs.EndDayContent);
        refs.SummaryPanel = EnsureOverlayPanel(panelsRoot.transform, "SummaryPanel", font, out refs.SummaryTitle, out refs.SummaryContent);

        refs.SchedulePanel.SetActive(false);
        refs.StaffPanel.SetActive(false);
        refs.MaintenancePanel.SetActive(false);
        refs.StationServicePanel.SetActive(false);
        refs.ExternalAffairsPanel.SetActive(false);
        refs.EndDayPanel.SetActive(false);
        refs.SummaryPanel.SetActive(false);

        return refs;
    }

    private static void PositionTopBarTexts(UiRefs refs)
    {
        SetRect(refs.DayText.rectTransform, 10, -6, 160, 20);
        SetRect(refs.MoneyText.rectTransform, 180, -6, 180, 20);
        SetRect(refs.TrustText.rectTransform, 370, -6, 140, 20);
        SetRect(refs.TrainConditionText.rectTransform, 10, -32, 160, 20);
        SetRect(refs.ExpectedPassengersText.rectTransform, 180, -32, 180, 20);
        SetRect(refs.StatusText.rectTransform, 370, -32, 420, 20);
    }

    private static void PositionRightPanelTexts(UiRefs refs)
    {
        SetRect(refs.DescriptionText.rectTransform, 10, -10, 300, 220);
        SetRect(refs.BriefingText.rectTransform, 10, -240, 300, 90);
        SetRect(refs.NoticesText.rectTransform, 10, -340, 300, 110);
        SetRect(refs.TodosText.rectTransform, 10, -460, 300, 130);
    }

    private static void PositionBottomButtons(UiRefs.ButtonSpec[] buttons)
    {
        const float startX = 10f;
        const float y = 0f;
        const float w = 98f;
        const float h = 44f;
        const float gap = 6f;

        for (var i = 0; i < buttons.Length; i++)
        {
            var rt = buttons[i].GameObject.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0f, 0.5f);
            rt.anchorMax = new Vector2(0f, 0.5f);
            rt.pivot = new Vector2(0f, 0.5f);
            rt.anchoredPosition = new Vector2(startX + i * (w + gap), y);
            rt.sizeDelta = new Vector2(w, h);
        }
    }

    private static GameObject EnsureOverlayPanel(Transform parent, string name, Font font, out Text title, out Text content)
    {
        var panel = FindChild(parent, name) ?? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        panel.transform.SetParent(parent, false);

        var rt = panel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        rt.sizeDelta = new Vector2(520f, 420f);

        var img = EnsureComponent<Image>(panel);
        img.color = new Color(0f, 0f, 0f, 0.82f);

        title = EnsureText(panel.transform, $"{name}_Title", font, 18, supportRichText: true);
        content = EnsureText(panel.transform, $"{name}_Content", font, 14, supportRichText: true);

        title.alignment = TextAnchor.UpperCenter;
        content.alignment = TextAnchor.UpperLeft;
        content.horizontalOverflow = HorizontalWrapMode.Wrap;
        content.verticalOverflow = VerticalWrapMode.Overflow;

        SetRect(title.rectTransform, 20, -20, 480, 34);
        SetRect(content.rectTransform, 20, -64, 480, 330);

        return panel;
    }

    private static Text EnsureText(Transform parent, string name, Font font, int fontSize, bool supportRichText = false)
    {
        var go = FindChild(parent, name) ?? new GameObject(name, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
        go.transform.SetParent(parent, false);

        var text = go.GetComponent<Text>();
        text.font = font;
        text.fontSize = fontSize;
        text.color = Color.white;
        text.alignment = TextAnchor.UpperLeft;
        text.supportRichText = supportRichText;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;

        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.localScale = Vector3.one;

        return text;
    }

    private static UiRefs.ButtonSpec EnsureButton(Transform parent, string name, string label, string type)
    {
        var go = FindChild(parent, name);
        if (go == null)
        {
            go = DefaultControls.CreateButton(new DefaultControls.Resources());
            go.name = name;
            go.transform.SetParent(parent, false);
        }

        var button = EnsureComponent<Button>(go);
        var image = EnsureComponent<Image>(go);
        image.color = new Color(1f, 1f, 1f, 0.9f);

        var text = go.GetComponentInChildren<Text>(true);
        if (text != null)
        {
            text.font = GetRuntimeFont();
            text.text = label;
            text.color = Color.black;
            text.alignment = TextAnchor.MiddleCenter;
        }

        return new UiRefs.ButtonSpec(go, button, type);
    }

    private static Font GetRuntimeFont()
    {
        Font font = null;

        try
        {
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
        }
        catch
        {
        }

        if (font == null)
        {
            try
            {
                font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            }
            catch
            {
            }
        }

        return font;
    }

    private static void AssignUiManagerReferences(UIManager uiManager, UiRefs refs)
    {
        // Top bar.
        uiManager.dayText = refs.DayText;
        uiManager.moneyText = refs.MoneyText;
        uiManager.trustText = refs.TrustText;
        uiManager.trainConditionText = refs.TrainConditionText;
        uiManager.expectedPassengersText = refs.ExpectedPassengersText;
        uiManager.statusText = refs.StatusText;

        // Right panel.
        uiManager.descriptionText = refs.DescriptionText;
        uiManager.briefingText = refs.BriefingText;
        uiManager.noticesText = refs.NoticesText;
        uiManager.todosText = refs.TodosText;

        // Panels + titles + content.
        uiManager.schedulePanel = refs.SchedulePanel;
        uiManager.schedulePanelTitle = refs.ScheduleTitle;
        uiManager.schedulePanelContent = refs.ScheduleContent;

        uiManager.staffPanel = refs.StaffPanel;
        uiManager.staffPanelTitle = refs.StaffTitle;
        uiManager.staffPanelContent = refs.StaffContent;

        uiManager.maintenancePanel = refs.MaintenancePanel;
        uiManager.maintenancePanelTitle = refs.MaintenanceTitle;
        uiManager.maintenancePanelContent = refs.MaintenanceContent;

        uiManager.stationServicePanel = refs.StationServicePanel;
        uiManager.stationServicePanelTitle = refs.StationServiceTitle;
        uiManager.stationServicePanelContent = refs.StationServiceContent;

        uiManager.externalAffairsPanel = refs.ExternalAffairsPanel;
        uiManager.externalAffairsPanelTitle = refs.ExternalAffairsTitle;
        uiManager.externalAffairsPanelContent = refs.ExternalAffairsContent;

        uiManager.endDayPanel = refs.EndDayPanel;
        uiManager.endDayPanelTitle = refs.EndDayTitle;
        uiManager.endDayPanelContent = refs.EndDayContent;

        uiManager.summaryPanel = refs.SummaryPanel;
        uiManager.summaryPanelTitle = refs.SummaryTitle;
        uiManager.summaryPanelContent = refs.SummaryContent;
    }

    private static void TryAssignAudioClips(AudioManager audioManager)
    {
        audioManager.uiClick = LoadAsset<AudioClip>("Assets/Audio/UI/ui_click.ogg");
        audioManager.uiSwitch = LoadAsset<AudioClip>("Assets/Audio/UI/ui_switch.ogg");
        audioManager.uiConfirm = LoadAsset<AudioClip>("Assets/Audio/UI/ui_confirm.ogg");
        audioManager.uiError = LoadAsset<AudioClip>("Assets/Audio/UI/ui_error.ogg");
    }

    private static TAsset LoadAsset<TAsset>(string path) where TAsset : UnityEngine.Object
    {
        var asset = AssetDatabase.LoadAssetAtPath<TAsset>(path);
        if (asset == null)
        {
            Debug.LogWarning($"RailRevival MVP: missing asset at '{path}'");
        }
        return asset;
    }

    private static GameObject EnsureRoot(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            if (root.name == name)
            {
                return root;
            }
        }

        var go = new GameObject(name);
        SceneManager.MoveGameObjectToScene(go, scene);
        return go;
    }

    private static GameObject EnsureChild(Transform parent, string name)
    {
        var existing = FindChild(parent, name);
        if (existing != null)
        {
            return existing;
        }

        var go = new GameObject(name);
        go.transform.SetParent(parent, false);
        return go;
    }

    private static GameObject FindChild(Transform parent, string name)
    {
        for (var i = 0; i < parent.childCount; i++)
        {
            var child = parent.GetChild(i);
            if (child.name == name)
            {
                return child.gameObject;
            }
        }
        return null;
    }

    private static GameObject FindFirstInScene(Scene scene, string name)
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var found = FindFirstInHierarchy(root.transform, name);
            if (found != null)
            {
                return found.gameObject;
            }
        }
        return null;
    }

    private static Transform FindFirstInHierarchy(Transform root, string name)
    {
        if (root.name == name)
        {
            return root;
        }

        for (var i = 0; i < root.childCount; i++)
        {
            var found = FindFirstInHierarchy(root.GetChild(i), name);
            if (found != null)
            {
                return found;
            }
        }
        return null;
    }

    private static T FindComponentInScene<T>(Scene scene) where T : Component
    {
        foreach (var root in scene.GetRootGameObjects())
        {
            var comp = root.GetComponentInChildren<T>(true);
            if (comp != null)
            {
                return comp;
            }
        }
        return null;
    }

    private static T EnsureComponent<T>(GameObject go) where T : Component
    {
        var comp = go.GetComponent<T>();
        if (comp == null)
        {
            comp = go.AddComponent<T>();
        }
        return comp;
    }

    private static void SetRect(RectTransform rt, float x, float y, float w, float h)
    {
        rt.anchorMin = new Vector2(0f, 1f);
        rt.anchorMax = new Vector2(0f, 1f);
        rt.pivot = new Vector2(0f, 1f);
        rt.anchoredPosition = new Vector2(x, y);
        rt.sizeDelta = new Vector2(w, h);
    }

    private sealed class UiRefs
    {
        public Text DayText;
        public Text MoneyText;
        public Text TrustText;
        public Text TrainConditionText;
        public Text ExpectedPassengersText;
        public Text StatusText;

        public Text DescriptionText;
        public Text BriefingText;
        public Text NoticesText;
        public Text TodosText;

        public GameObject SchedulePanel;
        public Text ScheduleTitle;
        public Text ScheduleContent;

        public GameObject StaffPanel;
        public Text StaffTitle;
        public Text StaffContent;

        public GameObject MaintenancePanel;
        public Text MaintenanceTitle;
        public Text MaintenanceContent;

        public GameObject StationServicePanel;
        public Text StationServiceTitle;
        public Text StationServiceContent;

        public GameObject ExternalAffairsPanel;
        public Text ExternalAffairsTitle;
        public Text ExternalAffairsContent;

        public GameObject EndDayPanel;
        public Text EndDayTitle;
        public Text EndDayContent;

        public GameObject SummaryPanel;
        public Text SummaryTitle;
        public Text SummaryContent;

        public ButtonSpec[] BottomButtons;

        public Button GetButton(string type)
        {
            foreach (var b in BottomButtons)
            {
                if (b.Type == type)
                {
                    return b.Button;
                }
            }
            return null;
        }

        public readonly struct ButtonSpec
        {
            public readonly GameObject GameObject;
            public readonly Button Button;
            public readonly string Type;

            public ButtonSpec(GameObject gameObject, Button button, string type)
            {
                GameObject = gameObject;
                Button = button;
                Type = type;
            }
        }
    }
}
#endif
