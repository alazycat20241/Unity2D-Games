using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject winPanel;
    public GameObject losePanel;
    public Transform finishPoint;

    [HideInInspector] public bool isGameActive = true;
    private bool isGameOver = false;  // 标记游戏是否已结束

    void Awake() => Instance = this;

    void Start()
    {
        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
    }

    void Update()
    {
        if (!isGameActive)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                if (isGameOver)
                {
                    Restart();
                }
                else
                {
                    Quit();
                }
            }
        }

        // 检查是否到达终点
        if (isGameActive && finishPoint != null)
        {
            PlayerController player = FindObjectOfType<PlayerController>();
            if (player != null &&Vector2.Distance(player.transform.position, finishPoint.position) < 0.5f)
            {
                GameWin();
            }
        }
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = true;

        if (losePanel) losePanel.SetActive(true);
    }

    public void GameWin()
    {
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = false;

        if (winPanel) winPanel.SetActive(true);
    }

    public void Restart() => SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    public void Quit() => Application.Quit();
}