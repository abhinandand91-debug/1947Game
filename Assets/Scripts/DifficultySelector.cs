// Author:  Abhinandan Dharmadhikari
// Created: 2026-10-16

using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Easy / Normal / Hard buttons for the main menu, built in code so no scene changes are needed.
/// </summary>
public class DifficultySelector : MonoBehaviour
{
    private static readonly string[] Names = { "EASY", "NORMAL", "HARD" };

    private readonly Image[] _buttons = new Image[3];

    public static DifficultySelector Create(Transform parent, Font font)
    {
        GameObject go = new GameObject("DifficultySelector", typeof(RectTransform));
        go.transform.SetParent(parent, false);

        DifficultySelector selector = go.AddComponent<DifficultySelector>();
        selector.Build(font);
        return selector;
    }

    private void Build(Font font)
    {
        RectTransform rect = (RectTransform)transform;
        rect.anchorMin = new Vector2(0.5f, 0.3f);
        rect.anchorMax = new Vector2(0.5f, 0.3f);
        rect.sizeDelta = new Vector2(740f, 70f);

        for (int i = 0; i < _buttons.Length; i++)
        {
            GameObject buttonObject = new GameObject(Names[i], typeof(RectTransform));
            buttonObject.transform.SetParent(transform, false);

            Image image = buttonObject.AddComponent<Image>();
            image.rectTransform.anchoredPosition = new Vector2((i - 1) * 250f, 0f);
            image.rectTransform.sizeDelta = new Vector2(230f, 70f);
            _buttons[i] = image;

            Button button = buttonObject.AddComponent<Button>();
            button.targetGraphic = image;
            int index = i;
            button.onClick.AddListener(() => Select(index));

            GameObject labelObject = new GameObject("Label", typeof(RectTransform));
            labelObject.transform.SetParent(buttonObject.transform, false);
            Text label = labelObject.AddComponent<Text>();
            label.font = font != null ? font : Font.CreateDynamicFontFromOSFont("Arial", 16);
            label.fontSize = 34;
            label.alignment = TextAnchor.MiddleCenter;
            label.text = Names[i];
            label.color = Color.white;
            label.raycastTarget = false;
            label.rectTransform.sizeDelta = new Vector2(230f, 70f);
        }

        Refresh();
    }

    private void Select(int index)
    {
        Difficulty.Level = index;
        Sfx.Play("ui_click");
        Refresh();
    }

    private void Refresh()
    {
        for (int i = 0; i < _buttons.Length; i++)
        {
            _buttons[i].color = i == Difficulty.Level ? new Color(0.2f, 0.7f, 0.35f) : new Color(0.3f, 0.34f, 0.42f);
        }
    }
}
