using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

/// <summary>
/// Drop this on any empty GameObject in your scene (or let it live as a prefab).
/// Builds its own world-space canvas at runtime, follows the camera, and mirrors
/// every Debug.Log / Warning / Error into it. No manual UI setup required.
/// Always auto-scrolls to the newest line, so no scrolling input is needed in VR.
///
/// Wire a world-space button's OnClick to WorldSpaceConsole.Instance.ToggleVisible()
/// if you want to show/hide it on-device.
/// </summary>
public class WorldSpaceConsole : MonoBehaviour
{
    [Header("Placement")]
    [Tooltip("What the console follows. Leave null to use Camera.main.")]
    [SerializeField] private Transform followTarget;
    [SerializeField] private Vector3 localOffset = new Vector3(0f, -0.15f, 1f);
    [SerializeField] private bool followEveryFrame = true;

    [Header("Appearance")]
    [SerializeField] private int maxLines = 150;
    [SerializeField] private float panelWidthMeters = 0.6f;
    [SerializeField] private float panelHeightMeters = 0.35f;
    [SerializeField] private int fontSize = 28;

    [Header("Visibility")]
    [SerializeField] private bool startVisible = true;

    public static WorldSpaceConsole Instance { get; private set; }

    private Canvas canvas;
    private TMP_Text text;
    private ScrollRect scrollRect;
    private RectTransform content;
    private readonly Queue<string> lines = new Queue<string>();
    private readonly StringBuilder builder = new StringBuilder();
    private bool visible;
    private bool dirty;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildUI();
        SetVisible(startVisible);
    }

    private void OnEnable() => Application.logMessageReceived += HandleLog;
    private void OnDisable() => Application.logMessageReceived -= HandleLog;

    private void Update()
    {
        if (!followEveryFrame) return;

        Transform t = followTarget != null ? followTarget : (Camera.main != null ? Camera.main.transform : null);
        if (t == null) return;

        transform.position = t.TransformPoint(localOffset);
        transform.rotation = t.rotation;
    }

    // Text/scroll update happens at most once per frame, and only while visible.
    private void LateUpdate()
    {
        if (!dirty || !visible || text == null) return;
        dirty = false;

        builder.Clear();
        foreach (string line in lines)
            builder.AppendLine(line);
        text.text = builder.ToString();

        LayoutRebuilder.ForceRebuildLayoutImmediate(content);
        scrollRect.verticalNormalizedPosition = 0f; // snap to newest line
    }

    public void ToggleVisible() => SetVisible(!visible);

    public void SetVisible(bool value)
    {
        visible = value;
        canvas.gameObject.SetActive(visible);
        if (visible) dirty = true; // refresh with anything logged while hidden
    }

    public void Clear()
    {
        lines.Clear();
        dirty = true;
    }

    private void HandleLog(string message, string stackTrace, LogType type)
    {
        string color = type switch
        {
            LogType.Error or LogType.Exception => "#FF5C5C",
            LogType.Warning => "#FFD166",
            _ => "#E0E0E0"
        };

        lines.Enqueue($"<color={color}>[{DateTime.Now:HH:mm:ss}] {message}</color>");
        while (lines.Count > maxLines)
            lines.Dequeue();

        dirty = true;
    }

    private void BuildUI()
    {
        const float refWidth = 1000f;
        const float refHeight = 600f;

        GameObject canvasGO = new GameObject("WorldSpaceConsoleCanvas");
        canvasGO.transform.SetParent(transform, false);
        canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        canvasGO.AddComponent<GraphicRaycaster>();

        RectTransform canvasRect = canvasGO.GetComponent<RectTransform>();
        canvasRect.sizeDelta = new Vector2(refWidth, refHeight);
        canvasRect.localScale = new Vector3(panelWidthMeters / refWidth, panelHeightMeters / refHeight, 1f);

        GameObject bg = new GameObject("Background");
        bg.transform.SetParent(canvasRect, false);
        Image bgImage = bg.AddComponent<Image>();
        bgImage.color = new Color(0f, 0f, 0f, 0.75f);
        StretchFull(bg.GetComponent<RectTransform>());

        GameObject scrollGO = new GameObject("Scroll View");
        scrollGO.transform.SetParent(bg.transform, false);
        scrollRect = scrollGO.AddComponent<ScrollRect>();
        RectTransform scrollRectTransform = scrollGO.GetComponent<RectTransform>();
        StretchFull(scrollRectTransform, 10f);

        GameObject viewportGO = new GameObject("Viewport");
        viewportGO.transform.SetParent(scrollRectTransform, false);
        viewportGO.AddComponent<RectMask2D>();
        Image viewportImage = viewportGO.AddComponent<Image>();
        viewportImage.color = new Color(1f, 1f, 1f, 0.001f);
        RectTransform viewportRect = viewportGO.GetComponent<RectTransform>();
        StretchFull(viewportRect);

        GameObject contentGO = new GameObject("Content");
        contentGO.transform.SetParent(viewportRect, false);
        content = contentGO.AddComponent<RectTransform>();
        content.anchorMin = new Vector2(0f, 1f);
        content.anchorMax = new Vector2(1f, 1f);
        content.pivot = new Vector2(0.5f, 1f);
        content.sizeDelta = new Vector2(0f, refHeight);

        // Layout group gives Content a real height from the text, so the ScrollRect has something to scroll.
        VerticalLayoutGroup layout = contentGO.AddComponent<VerticalLayoutGroup>();
        layout.childControlWidth = true;
        layout.childControlHeight = true;
        layout.childForceExpandWidth = true;
        layout.childForceExpandHeight = false;

        ContentSizeFitter fitter = contentGO.AddComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;

        GameObject textGO = new GameObject("LogText");
        textGO.transform.SetParent(content, false);
        text = textGO.AddComponent<TextMeshProUGUI>();
        text.fontSize = fontSize;
        text.enableWordWrapping = true;
        text.richText = true;
        text.text = string.Empty;

        scrollRect.content = content;
        scrollRect.viewport = viewportRect;
        scrollRect.horizontal = false;
        scrollRect.vertical = true;
    }

    private static void StretchFull(RectTransform rect, float margin = 0f)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = new Vector2(margin, margin);
        rect.offsetMax = new Vector2(-margin, -margin);
    }
}