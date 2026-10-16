// Author:  Abhinandan Dharmadhikari
// Created: 2020-04-03

using System;
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

    private void Awake()
    {
        BuildBestText();
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
        GameManager.SetActive(true);
        ScoreText.text = "Score: 0";

        foreach (Transform health in GameScreen.transform.Find("Health").transform)
        {
            health.gameObject.SetActive(true);
        }

        MainScreen.SetActive(false);
        GameScreen.SetActive(true);

        Cursor.visible = false;
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
