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
    public GameObject player;

    [HideInInspector] public bool isGameActive = false;  // 改为false，等待空格开始
    [HideInInspector] public bool isGameOver = false;    // 标记游戏是否已结束

    private bool isWaitingForStart = true;  // 是否等待开始

    void Awake() => Instance = this;

    void Start()
    {
        // 开始时禁用玩家
        if (player != null)
            player.SetActive(false);

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);
        
        // 游戏初始不开始，等待空格
        isGameActive = false;
        isWaitingForStart = true;
        
        // 不播放音乐，等待开始
        MusicManager.Instance.StopAllMusic();

    }

    void Update()
    {
        // 空格键开始游戏
        if (isWaitingForStart && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }

        // 游戏结束后的处理
        if (!isGameActive && !isWaitingForStart)
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
            if (player != null && Vector2.Distance(player.transform.position, finishPoint.position) < 0.5f)
            {
                GameWin();
            }
        }
    }

    void StartGame()
    {
        if (player != null)
            player.SetActive(true);
        isWaitingForStart = false;
        isGameActive = true;
        MusicManager.Instance.PlayBGM();  // 开始播放背景音乐
        Debug.Log("游戏开始");
    }

    public void GameOver()
    {
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = true;

        if (losePanel) losePanel.SetActive(true);
        MusicManager.Instance.StopBGM();  // 停止背景音乐
        MusicManager.Instance.PlayLoseSound();  // 播放失败音效
    }

    public void GameWin()
    {
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = false;

        if (winPanel) winPanel.SetActive(true);
        MusicManager.Instance.StopBGM();  // 停止背景音乐
        MusicManager.Instance.PlayWinSound();  // 播放胜利音效
    }

    public void Restart()
    {
        MusicManager.Instance.StopAllMusic();  // 停止所有音乐
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }
    
    public void Quit() => Application.Quit();
}