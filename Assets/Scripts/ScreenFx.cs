// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Full-screen feedback effects built in code on their own overlay canvas: color flashes,
/// a pulsing red vignette for low health and floating score popups.
/// </summary>
public class ScreenFx : MonoBehaviour
{
    /// <summary>
    /// Font used for popups. Set by the UI so the effects match the rest of the game.
    /// </summary>
    public static Font Font;

    private static ScreenFx _instance;

    private readonly Queue<Text> _popupPool = new Queue<Text>();
    private Image _flash;
    private Image _vignette;
    private Image _bossBack;
    private Image _bossFill;
    private Text _comboText;
    private Image _comboBack;
    private Image _comboFill;
    private int _shownMultiplier;
    private Coroutine _flashRoutine;
    private Coroutine _pressureRoutine;

    private static ScreenFx Instance
    {
        get
        {
            if (_instance == null)
            {
                GameObject go = new GameObject("ScreenFx");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<ScreenFx>();
                _instance.Build();
            }
            return _instance;
        }
    }

    /// <summary>
    /// Flashes the whole screen with a color that fades out.
    /// </summary>
    public static void Flash(Color color, float peakAlpha)
    {
        Instance.StartFlash(color, peakAlpha);
    }

    /// <summary>
    /// Shows a floating text that rises from a world position and fades out.
    /// </summary>
    public static void Popup(Vector3 worldPosition, string text, Color color, float scale)
    {
        Instance.StartPopup(worldPosition, text, color, scale);
    }

    /// <summary>
    /// Turns the pulsing red low-health vignette on or off.
    /// </summary>
    public static void SetPressure(bool on)
    {
        if (!on && _instance == null)
        {
            return;
        }
        Instance.StartPressure(on);
    }

    /// <summary>
    /// Shows the combo multiplier and how much time is left to keep it alive. Multipliers of 1 or less hide it.
    /// </summary>
    public static void SetCombo(int multiplier, float timeFraction)
    {
        if (multiplier <= 1 && _instance == null)
        {
            return;
        }
        Instance.UpdateCombo(multiplier, timeFraction);
    }

    private void BuildComboHud()
    {
        _comboText = CreatePopupLabel();
        _comboText.name = "ComboText";
        _comboText.alignment = TextAnchor.MiddleLeft;
        _comboText.color = new Color(1f, 0.85f, 0.3f);
        _comboText.rectTransform.anchorMin = new Vector2(0.02f, 0.80f);
        _comboText.rectTransform.anchorMax = new Vector2(0.32f, 0.88f);
        _comboText.rectTransform.offsetMin = Vector2.zero;
        _comboText.rectTransform.offsetMax = Vector2.zero;

        _comboBack = new GameObject("ComboBar", typeof(RectTransform)).AddComponent<Image>();
        _comboBack.transform.SetParent(transform, false);
        _comboBack.color = new Color(0f, 0f, 0f, 0.6f);
        _comboBack.raycastTarget = false;
        _comboBack.rectTransform.anchorMin = new Vector2(0.02f, 0.785f);
        _comboBack.rectTransform.anchorMax = new Vector2(0.28f, 0.8f);
        _comboBack.rectTransform.offsetMin = Vector2.zero;
        _comboBack.rectTransform.offsetMax = Vector2.zero;

        _comboFill = new GameObject("Fill", typeof(RectTransform)).AddComponent<Image>();
        _comboFill.transform.SetParent(_comboBack.transform, false);
        _comboFill.color = new Color(1f, 0.6f, 0.15f);
        _comboFill.raycastTarget = false;
        _comboFill.rectTransform.anchorMin = Vector2.zero;
        _comboFill.rectTransform.anchorMax = Vector2.one;
        _comboFill.rectTransform.offsetMin = new Vector2(2f, 2f);
        _comboFill.rectTransform.offsetMax = new Vector2(-2f, -2f);

        _comboText.gameObject.SetActive(false);
        _comboBack.gameObject.SetActive(false);
    }

    private void UpdateCombo(int multiplier, float timeFraction)
    {
        bool show = multiplier > 1;
        _comboText.gameObject.SetActive(show);
        _comboBack.gameObject.SetActive(show);
        if (!show)
        {
            return;
        }

        if (multiplier != _shownMultiplier)
        {
            _shownMultiplier = multiplier;
            _comboText.text = "COMBO x" + multiplier;
        }
        _comboFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(timeFraction), 1f);
    }

    /// <summary>
    /// Shows or hides the boss health bar at the top of the screen. Fraction is 0-1.
    /// </summary>
    public static void SetBossBar(float fraction, bool visible)
    {
        if (!visible && _instance == null)
        {
            return;
        }
        Instance.UpdateBossBar(fraction, visible);
    }

    private void BuildBossBar()
    {
        _bossBack = new GameObject("BossBar", typeof(RectTransform)).AddComponent<Image>();
        _bossBack.transform.SetParent(transform, false);
        _bossBack.color = new Color(0f, 0f, 0f, 0.65f);
        _bossBack.raycastTarget = false;
        _bossBack.rectTransform.anchorMin = new Vector2(0.25f, 0.925f);
        _bossBack.rectTransform.anchorMax = new Vector2(0.75f, 0.955f);
        _bossBack.rectTransform.offsetMin = Vector2.zero;
        _bossBack.rectTransform.offsetMax = Vector2.zero;

        _bossFill = new GameObject("Fill", typeof(RectTransform)).AddComponent<Image>();
        _bossFill.transform.SetParent(_bossBack.transform, false);
        _bossFill.color = new Color(0.9f, 0.2f, 0.3f);
        _bossFill.raycastTarget = false;
        _bossFill.rectTransform.anchorMin = Vector2.zero;
        _bossFill.rectTransform.anchorMax = Vector2.one;
        _bossFill.rectTransform.offsetMin = new Vector2(3f, 3f);
        _bossFill.rectTransform.offsetMax = new Vector2(-3f, -3f);

        _bossBack.gameObject.SetActive(false);
    }

    private void UpdateBossBar(float fraction, bool visible)
    {
        _bossBack.gameObject.SetActive(visible);
        if (visible)
        {
            _bossFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01(fraction), 1f);
        }
    }

    /// <summary>
    /// Shows a large centred message such as a wave announcement.
    /// </summary>
    public static void Banner(string text, Color color)
    {
        Instance.StartBanner(text, color);
    }

    private void StartBanner(string text, Color color)
    {
        Text label = _popupPool.Count > 0 ? _popupPool.Dequeue() : CreatePopupLabel();
        label.gameObject.SetActive(true);
        label.text = text;
        label.color = color;
        label.rectTransform.position = new Vector3(Screen.width * 0.5f, Screen.height * 0.72f, 0f);
        StartCoroutine(PopupRoutine(label, color, 1.8f, 1.6f, 0f));
    }

    private void Build()
    {
        Canvas canvas = gameObject.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        canvas.sortingOrder = 50;

        _vignette = CreateFullScreenImage("Vignette", MakeVignetteSprite());
        _flash = CreateFullScreenImage("Flash", null);
        BuildBossBar();
        BuildComboHud();
    }

    private Image CreateFullScreenImage(string imageName, Sprite sprite)
    {
        GameObject go = new GameObject(imageName, typeof(RectTransform));
        go.transform.SetParent(transform, false);

        Image image = go.AddComponent<Image>();
        image.sprite = sprite;
        image.color = Color.clear;
        image.raycastTarget = false;

        RectTransform rect = image.rectTransform;
        rect.anchorMin = Vector2.zero;
        rect.anchorMax = Vector2.one;
        rect.offsetMin = Vector2.zero;
        rect.offsetMax = Vector2.zero;
        return image;
    }

    /// <summary>
    /// Builds a transparent-centre, opaque-edge radial gradient so no texture asset is needed.
    /// </summary>
    private static Sprite MakeVignetteSprite()
    {
        const int size = 64;
        Texture2D texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
        texture.wrapMode = TextureWrapMode.Clamp;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = (x + 0.5f) / size * 2f - 1f;
                float dy = (y + 0.5f) / size * 2f - 1f;
                float edge = Mathf.Clamp01((Mathf.Sqrt(dx * dx + dy * dy) - 0.55f) / 0.75f);
                texture.SetPixel(x, y, new Color(1f, 1f, 1f, edge * edge));
            }
        }
        texture.Apply();
        return Sprite.Create(texture, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    private void StartFlash(Color color, float peakAlpha)
    {
        if (_flashRoutine != null)
        {
            StopCoroutine(_flashRoutine);
        }
        _flashRoutine = StartCoroutine(FlashRoutine(color, peakAlpha));
    }

    private IEnumerator FlashRoutine(Color color, float peakAlpha)
    {
        for (float t = 0f; t < 0.35f; t += Time.unscaledDeltaTime)
        {
            _flash.color = new Color(color.r, color.g, color.b, peakAlpha * (1f - t / 0.35f));
            yield return null;
        }
        _flash.color = Color.clear;
    }

    private void StartPressure(bool on)
    {
        if (_pressureRoutine != null)
        {
            StopCoroutine(_pressureRoutine);
        }
        _pressureRoutine = StartCoroutine(on ? PressureRoutine() : FadeVignetteRoutine());
    }

    private IEnumerator PressureRoutine()
    {
        for (float t = 0f; ; t += Time.unscaledDeltaTime)
        {
            float alpha = 0.18f + 0.22f * (0.5f + 0.5f * Mathf.Sin(t * Mathf.PI * 2f));
            _vignette.color = new Color(1f, 0.1f, 0.1f, alpha);
            yield return null;
        }
    }

    private IEnumerator FadeVignetteRoutine()
    {
        Color start = _vignette.color;
        for (float t = 0f; t < 0.4f; t += Time.unscaledDeltaTime)
        {
            _vignette.color = new Color(start.r, start.g, start.b, start.a * (1f - t / 0.4f));
            yield return null;
        }
        _vignette.color = Color.clear;
    }

    private void StartPopup(Vector3 worldPosition, string text, Color color, float scale)
    {
        Camera cam = Camera.main;
        if (cam == null)
        {
            return;
        }

        Text label = _popupPool.Count > 0 ? _popupPool.Dequeue() : CreatePopupLabel();
        label.gameObject.SetActive(true);
        label.text = text;
        label.color = color;
        label.rectTransform.position = cam.WorldToScreenPoint(worldPosition);
        StartCoroutine(PopupRoutine(label, color, scale, 0.8f, Screen.height * 0.1f));
    }

    private Text CreatePopupLabel()
    {
        GameObject go = new GameObject("Popup", typeof(RectTransform));
        go.transform.SetParent(transform, false);

        Text label = go.AddComponent<Text>();
        label.font = Font != null ? Font : Font.CreateDynamicFontFromOSFont("Arial", 16);
        label.fontSize = Mathf.RoundToInt(Screen.height / 16f);
        label.alignment = TextAnchor.MiddleCenter;
        label.horizontalOverflow = HorizontalWrapMode.Overflow;
        label.verticalOverflow = VerticalWrapMode.Overflow;
        label.raycastTarget = false;
        go.AddComponent<Outline>().effectColor = new Color(0f, 0f, 0f, 0.8f);
        return label;
    }

    private IEnumerator PopupRoutine(Text label, Color color, float scale, float duration, float rise)
    {
        RectTransform rect = label.rectTransform;
        Vector3 start = rect.position;

        for (float t = 0f; t < duration; t += Time.unscaledDeltaTime)
        {
            float k = t / duration;
            float eased = 1f - Mathf.Pow(1f - k, 3f);
            rect.position = start + Vector3.up * (rise * eased);
            rect.localScale = Vector3.one * (scale * (k < 0.2f ? OutBack(k / 0.2f) : 1f));
            label.color = new Color(color.r, color.g, color.b, k < 0.6f ? 1f : 1f - (k - 0.6f) / 0.4f);
            yield return null;
        }

        label.gameObject.SetActive(false);
        _popupPool.Enqueue(label);
    }

    private static float OutBack(float k)
    {
        const float c1 = 1.70158f;
        const float c3 = c1 + 1f;
        return 1f + c3 * Mathf.Pow(k - 1f, 3f) + c1 * Mathf.Pow(k - 1f, 2f);
    }
}
