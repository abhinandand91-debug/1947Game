// Author:  Abhinandan Dharmadhikari
// Created: 2020-04-03

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Manages the User Interface game objects
/// </summary>
public class UserInterfaceManager : MonoBehaviour
{
    public Text ScoreText;
    public GameObject MainScreen;
    public GameObject GameScreen;
    public GameObject GameManager;

    public GameObject[] HealthIcons;


    private Text _bestText;
    private GameOverScreen _gameOver;
    private PauseMenu _pause;
    private readonly List<GameObject> _extraHearts = new List<GameObject>();

    private void Awake()
    {
        BuildBestText();
        ScreenFx.Font = ScoreText.font;
        _gameOver = GameOverScreen.Create(ScoreText.font);
        DifficultySelector.Create(MainScreen.transform, ScoreText.font);
        _pause = PauseMenu.Create(ScoreText.font, QuitToMenu);
        MobileControls.Create(ScoreText.font, _pause.Toggle);
        _bestText.text = "Best: " + HighScore.Best;
    }

    /// <summary>
    /// Creates the best score label on the main screen so no scene changes are needed.
    /// </summary>
    private void BuildBestText()
    {
        GameObject label = new GameObject("BestScoreText", typeof(RectTransform));
        label.transform.SetParent(MainScreen.transform, false);

        _bestText = label.AddComponent<Text>();
        _bestText.font = ScoreText.font;
        _bestText.fontSize = ScoreText.fontSize;
        _bestText.alignment = TextAnchor.MiddleCenter;
        _bestText.color = Color.white;

        RectTransform rect = _bestText.rectTransform;
        rect.anchorMin = new Vector2(0.5f, 0f);
        rect.anchorMax = new Vector2(0.5f, 0f);
        rect.pivot = new Vector2(0.5f, 0f);
        rect.anchoredPosition = new Vector2(0f, 40f);
        rect.sizeDelta = new Vector2(700f, 120f);
    }

    /// <summary>
    /// Ends the game and shows the final score along with the best score.
    /// </summary>
    /// <param name="finalScore">Score of the run that just ended</param>
    public void EndGame(int finalScore)
    {
        bool newRecord = HighScore.Submit(finalScore);
        _bestText.text = newRecord
            ? "New best: " + finalScore + "!"
            : "Score: " + finalScore + "   Best: " + HighScore.Best;
        GameManager.SetActive(false);
        ScreenFx.SetPressure(false);
        MobileControls.SetVisible(false);
        StartCoroutine(ShowGameOver(finalScore, newRecord));
    }

    /// <summary>
    /// Waits a moment so the player's explosion can play, then shows the game over screen.
    /// </summary>
    private IEnumerator ShowGameOver(int score, bool newRecord)
    {
        yield return new WaitForSecondsRealtime(0.9f);

        GameScreen.SetActive(false);
        Cursor.visible = true;
        Sfx.Play(newRecord ? "new_best" : "game_over");
        _gameOver.Show(score, HighScore.Best, newRecord, Start_OnClick, EndGame);
    }

    /// <summary>
    /// Brings back the rightmost lost heart (hearts are lost from the left).
    /// </summary>
    public void RestoreHeart()
    {
        Transform health = GameScreen.transform.Find("Health");
        for (int i = health.childCount - 1; i >= 0; i--)
        {
            GameObject heart = health.GetChild(i).gameObject;
            if (!heart.activeSelf)
            {
                heart.SetActive(true);
                return;
            }
        }
    }

    /// <summary>
    /// Refills every heart.
    /// </summary>
    public void RestoreAllHearts()
    {
        foreach (Transform heart in GameScreen.transform.Find("Health"))
        {
            heart.gameObject.SetActive(true);
        }
    }

    /// <summary>
    /// Adds a bonus heart to the row. It is removed again when the next run starts.
    /// </summary>
    public void AddHeart()
    {
        Transform health = GameScreen.transform.Find("Health");
        if (health.childCount == 0)
        {
            return;
        }

        GameObject copy = Instantiate(health.GetChild(health.childCount - 1).gameObject, health);
        copy.SetActive(true);
        _extraHearts.Add(copy);
    }

    private void ClearExtraHearts()
    {
        foreach (GameObject heart in _extraHearts)
        {
            if (heart != null)
            {
                Destroy(heart);
            }
        }
        _extraHearts.Clear();
    }

    /// <summary>
    /// Leaves a run from the pause menu: clears the field and returns to the main screen.
    /// </summary>
    private void QuitToMenu()
    {
        foreach (GameObject enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            Destroy(enemy);
        }
        foreach (Laser laser in FindObjectsOfType<Laser>())
        {
            Destroy(laser.gameObject);
        }
        foreach (Pickup pickup in FindObjectsOfType<Pickup>())
        {
            Destroy(pickup.gameObject);
        }

        GameManager.GetComponent<GameManager>().TogglePlayer(false);
        ScreenFx.SetPressure(false);
        ScreenFx.SetCombo(1, 0f);
        MobileControls.SetVisible(false);
        EndGame();
    }

    public void AdjustScore(int amount)
    {
        ScoreText.text = "Score: " + amount;
    }


    /// <summary>
    /// Removes a heart from the UI.
    /// </summary>
    public void DecreasePlayerHealth()
    {
        foreach (Transform heart in GameScreen.transform.Find("Health").transform)
        {
            if (heart.gameObject.activeSelf)
            {
                heart.gameObject.SetActive(false);
                break;
            }
        }
    }

    /// <summary>
    /// Starts the game by enabling the Game Manager and 
    /// </summary>
    public void Start_OnClick()
    {
        ClearExtraHearts();
        GameManager.SetActive(true);
        ScoreText.text = "Score: 0";

        foreach (Transform health in GameScreen.transform.Find("Health").transform)
        {
            health.gameObject.SetActive(true);
        }

        MainScreen.SetActive(false);
        GameScreen.SetActive(true);

        Cursor.visible = false;
        MobileControls.SetVisible(true);
    }

    /// <summary>
    /// Ends the gam by removing enemy AI and toggling the appropriate UI
    /// </summary>
    public void EndGame()
    {
        GameManager.SetActive(false);
        MainScreen.SetActive(true);
        GameScreen.SetActive(false);

        Cursor.visible = true;
    }
}
