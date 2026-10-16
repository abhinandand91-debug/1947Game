// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

/// <summary>
/// On-screen buttons for touch devices: a pause button, and a fire button when auto-fire is off.
/// Also handles the Escape / P keys for pausing on desktop (Escape is the Android back button too).
/// </summary>
public class MobileControls : MonoBehaviour
{
    private static MobileControls _instance;

    private Action _onPause;
    private GameObject _pauseButton;
    private GameObject _fireButton;
    private bool _visible;

    /// <summary>True while the on-screen fire button is held down.</summary>
    public static bool FireHeld { get; private set; }

    public static void Create(Font font, Action onPause)
    {
        if (_instance != null)
        {
            return;
        }

        GameObject go = new GameObject("MobileControls");
        _instance = go.AddComponent<MobileControls>();
        _instance._onPause = onPause;
        _instance.Build(font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16));
    }

    /// <summary>
    /// Shows the controls while a run is in progress and hides them otherwise.
    /// </summary>
    public static void SetVisible(bool visible)
    {
        if (_instance == null)
        {
            return;
        }

        _instance._visible = visible;
        if (!visible)
        {
            FireHeld = false;
        }
        _instance.Refresh();
    }

    private void Build(Font font)
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 40;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        _pauseButton = MakeButton("Pause", "II", font, new Vector2(0f, 0f), new Vector2(60f, 60f), new Vector2(90f, 90f), new Color(0f, 0f, 0f, 0.45f));
        _pauseButton.GetComponent<Button>().onClick.AddListener(delegate { RequestPause(); });

        _fireButton = MakeButton("Fire", "FIRE", font, new Vector2(1f, 0f), new Vector2(-150f, 150f), new Vector2(200f, 200f), new Color(0.9f, 0.25f, 0.25f, 0.5f));
        EventTrigger trigger = _fireButton.AddComponent<EventTrigger>();
        AddTrigger(trigger, EventTriggerType.PointerDown, true);
        AddTrigger(trigger, EventTriggerType.PointerUp, false);
        AddTrigger(trigger, EventTriggerType.PointerExit, false);

        Refresh();
    }

    private GameObject MakeButton(string objectName, string label, Font font, Vector2 anchor, Vector2 position, Vector2 size, Color color)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        Image image = go.AddComponent<Image>();
        image.color = color;
        image.rectTransform.anchorMin = anchor;
        image.rectTransform.anchorMax = anchor;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;

        go.AddComponent<Button>().targetGraphic = image;

        GameObject labelObject = new GameObject("Label", typeof(RectTransform));
        labelObject.transform.SetParent(go.transform, false);
        Text text = labelObject.AddComponent<Text>();
        text.font = font;
        text.fontSize = Mathf.RoundToInt(size.y * 0.32f);
        text.alignment = TextAnchor.MiddleCenter;
        text.text = label;
        text.color = Color.white;
        text.raycastTarget = false;
        text.rectTransform.sizeDelta = size;
        return go;
    }

    private static void AddTrigger(EventTrigger trigger, EventTriggerType type, bool held)
    {
        EventTrigger.Entry entry = new EventTrigger.Entry();
        entry.eventID = type;
        entry.callback.AddListener(delegate { FireHeld = held; });
        trigger.triggers.Add(entry);
    }

    private void Update()
    {
        if (!_visible)
        {
            return;
        }

        if (Input.GetKeyDown(KeyCode.Escape) || Input.GetKeyDown(KeyCode.P))
        {
            RequestPause();
        }

        // Auto-fire can be switched in the pause menu, so keep the fire button in sync
        Refresh();
    }

    private void RequestPause()
    {
        if (_onPause != null)
        {
            _onPause();
        }
    }

    private void Refresh()
    {
        _pauseButton.SetActive(_visible && Input.touchSupported);
        _fireButton.SetActive(_visible && Input.touchSupported && !GameSettings.AutoFire);
    }
}
