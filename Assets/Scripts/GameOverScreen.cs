// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using System;
using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Game over panel with the final score, the best score and Retry / Menu buttons.
/// Built entirely in code on its own canvas, so it needs no scene setup.
/// </summary>
public class GameOverScreen : MonoBehaviour
{
    private static readonly Color Gold = new Color(1f, 0.85f, 0.3f);

    private Font _font;
    private CanvasGroup _group;
    private RectTransform _panel;
    private Text _title;
    private Text _scoreText;
    private Text _bestText;
    private Action _onRetry;
    private Action _onMenu;

    /// <summary>
    /// Creates the (hidden) screen. Call Show to display it.
    /// </summary>
    public static GameOverScreen Create(Font font)
    {
        GameObject go = new GameObject("GameOverScreen");
        GameOverScreen screen = go.AddComponent<GameOverScreen>();
        screen._font = font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
        screen.Build();
        go.SetActive(false);
        return screen;
    }

    /// <summary>
    /// Shows the screen with the result of the run that just ended.
    /// </summary>
    public void Show(int score, int best, bool newBest, Action onRetry, Action onMenu)
    {
        _onRetry = onRetry;
        _onMenu = onMenu;
        _title.text = newBest ? "NEW BEST!" : "GAME OVER";
        _title.color = newBest ? Gold : new Color(1f, 0.35f, 0.35f);
        _scoreText.text = "Score: " + score;
        _bestText.text = "Best: " + best;

        gameObject.SetActive(true);
        StartCoroutine(FadeIn());
    }

    private void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 60;

        CanvasScaler scaler = gameObject.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);
        scaler.matchWidthOrHeight = 0.5f;

        gameObject.AddComponent<GraphicRaycaster>();
        _group = gameObject.AddComponent<CanvasGroup>();

        // Dim the game behind the panel and swallow stray taps
        Image dim = new GameObject("Dim", typeof(RectTransform)).AddComponent<Image>();
        dim.transform.SetParent(transform, false);
        dim.color = new Color(0f, 0f, 0f, 0.75f);
        Stretch(dim.rectTransform);

        _panel = new GameObject("Panel", typeof(RectTransform)).GetComponent<RectTransform>();
        _panel.SetParent(transform, false);
        _panel.sizeDelta = new Vector2(700f, 560f);

        _title = MakeText("Title", 96, new Vector2(0f, 190f));
        _scoreText = MakeText("Score", 64, new Vector2(0f, 80f));
        _scoreText.color = Color.white;
        _bestText = MakeText("Best", 44, new Vector2(0f, 10f));
        _bestText.color = Gold;

        MakeButton("Retry", "RETRY", new Vector2(0f, -110f), new Vector2(360f, 96f), new Color(0.2f, 0.7f, 0.35f), OnRetryClicked);
        MakeButton("Menu", "MENU", new Vector2(0f, -225f), new Vector2(360f, 76f), new Color(0.3f, 0.34f, 0.42f), OnMenuClicked);
    }

    private Text MakeText(string objectName, int size, Vector2 position)
    {
        GameObject go = new GameObject(objectName, typeof(RectTransform));
        go.transform.SetParent(_panel, false);

        Text text = go.AddComponent<Text>();
        text.font = _font;
        text.fontSize = size;
        text.alignment = TextAnchor.MiddleCenter;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        text.raycastTarget = false;
        go.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);

        text.rectTransform.anchoredPosition = position;
        text.rectTransform.sizeDelta = new Vector2(700f, 120f);
        return text;
    }

    private void MakeButton(string objectName, string label, Vector2 position, Vector2 size, Color color, UnityEngine.Events.UnityAction onClick)
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

        Text text = MakeText(objectName + "Label", Mathf.RoundToInt(size.y * 0.5f), Vector2.zero);
        text.transform.SetParent(go.transform, false);
        text.text = label;
        text.color = Color.white;
        text.rectTransform.sizeDelta = size;
    }

    private void OnRetryClicked()
    {
        Sfx.Play("ui_click");
        gameObject.SetActive(false);
        if (_onRetry != null)
        {
            _onRetry();
        }
    }

    private void OnMenuClicked()
    {
        Sfx.Play("ui_click");
        gameObject.SetActive(false);
        if (_onMenu != null)
        {
            _onMenu();
        }
    }

    private IEnumerator FadeIn()
    {
        const float duration = 0.35f;
        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            _group.alpha = k;
            _panel.localScale = Vector3.one * Mathf.Lerp(0.85f, 1f, 1f - (1f - k) * (1f - k));
            yield return null;
        }
        _group.alpha = 1f;
        _panel.localScale = Vector3.one;
    }

    private static void Stretch(RectTransform rect)
    {
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
    }
}
