// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Pause panel with Resume, auto-fire and vibration toggles and Quit. Built in code on its own canvas.
/// </summary>
public class PauseMenu : MonoBehaviour
{
    private Font _font;
    private RectTransform _panel;
    private Text _autoFireLabel;
    private Text _vibrationLabel;
    private Action _onQuit;

    public static PauseMenu Create(Font font, Action onQuit)
    {
        GameObject go = new GameObject("PauseMenu");
        PauseMenu menu = go.AddComponent<PauseMenu>();
        menu._font = font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
        menu._onQuit = onQuit;
        menu.Build();
        go.SetActive(false);
        return menu;
    }

    /// <summary>
    /// Pauses the game, or resumes it if it is already paused.
    /// </summary>
    public void Toggle()
    {
        if (gameObject.activeSelf)
        {
            Resume();
            return;
        }

        RefreshLabels();
        gameObject.SetActive(true);
        Time.timeScale = 0f;
    }

    private void Resume()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1f;
    }

    private void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 70;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;
        gameObject.AddComponent<GraphicRaycaster>();

        Image dim = new GameObject("Dim", typeof(RectTransform)).AddComponent<Image>();
        dim.transform.SetParent(transform, false);
        dim.color = new Color(0f, 0f, 0f, 0.8f);
        dim.rectTransform.anchorMin = Vector2.zero;
        dim.rectTransform.anchorMax = Vector2.one;
        dim.rectTransform.offsetMin = Vector2.zero;
        dim.rectTransform.offsetMax = Vector2.zero;

        _panel = new GameObject("Panel", typeof(RectTransform)).GetComponent<RectTransform>();
        _panel.SetParent(transform, false);

        Text title = MakeLabel(_panel, "Title", 96, new Vector2(0f, 210f), new Vector2(700f, 120f));
        title.text = "PAUSED";

        MakeButton("Resume", "RESUME", new Vector2(0f, 80f), new Vector2(460f, 90f), new Color(0.2f, 0.7f, 0.35f), Resume);
        _autoFireLabel = MakeButton("AutoFire", "", new Vector2(0f, -30f), new Vector2(460f, 76f), new Color(0.3f, 0.34f, 0.42f), ToggleAutoFire);
        _vibrationLabel = MakeButton("Vibration", "", new Vector2(0f, -125f), new Vector2(460f, 76f), new Color(0.3f, 0.34f, 0.42f), ToggleVibration);
        MakeButton("Quit", "QUIT TO MENU", new Vector2(0f, -230f), new Vector2(460f, 76f), new Color(0.65f, 0.25f, 0.25f), Quit);
    }

    private Text MakeLabel(Transform parent, string objectName, int size, Vector2 position, Vector2 boxSize)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(parent, false);

        Text text = go.AddComponent<Text>();
        text.font = _font;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.color = Color.white;
        text.raycastTarget = false;
        go.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = boxSize;
        return text;
    }

    private Text MakeButton(string objectName, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(_panel, false);

        Image image = go.AddComponent<Image>();
        image.color = color;
        image.rectTransform.anchoredPosition = position;
        image.rectTransform.sizeDelta = size;

        Button button = go.AddComponent<Button>();
        button.targetGraphic = image;
        button.onClick.AddListener(onClick);

        Text text = MakeLabel(go.transform, objectName + "Label", Mathf.RoundToInt(size.y * 0.45f), Vector2.zero, size);
        text.text = label;
        return text;
    }

    private void RefreshLabels()
    {
        _autoFireLabel.text = "AUTO-FIRE: " + (GameSettings.AutoFire ? "ON" : "OFF");
        _vibrationLabel.text = "VIBRATION: " + (GameSettings.Vibration ? "ON" : "OFF");
    }

    private void ToggleAutoFire()
    {
        GameSettings.AutoFire = !GameSettings.AutoFire;
        Sfx.Play("ui_click");
        RefreshLabels();
    }

    private void ToggleVibration()
    {
        GameSettings.Vibration = !GameSettings.Vibration;
        Sfx.Play("ui_click");
        RefreshLabels();
    }

    private void Quit()
    {
        Sfx.Play("ui_click");
        Resume();
        if (_onQuit != null)
        {
            _onQuit();
        }
    }
}
