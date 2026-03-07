using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject winPanel;
    public GameObject losePanel;
    public Transform finishPoint;
    public GameObject player;

    [Header("开始界面")]
    public GameObject startPanel;  // 开始界面面板
    public Button startButton;      // 开始按钮
    public Button quitButton;       // 退出按钮

    [HideInInspector] public bool isGameActive = false;
    [HideInInspector] public bool isGameOver = false;

    private bool isWaitingForStart = true;

    void Awake() => Instance = this;

    void Start()
    {
        // 开始时禁用玩家
        if (player != null)
            player.SetActive(false);

        if (winPanel) winPanel.SetActive(false);
        if (losePanel) losePanel.SetActive(false);

        // 显示开始界面
        if (startPanel)
        {
            startPanel.SetActive(true);

            // 绑定按钮事件
            if (startButton)
                startButton.onClick.AddListener(StartGame);

            if (quitButton)
                quitButton.onClick.AddListener(Quit);
        }

        // 游戏初始不开始
        isGameActive = false;
        isWaitingForStart = true;

        // 不播放音乐，等待开始
        MusicManager.Instance.StopAllMusic();
    }

    void Update()
    {
        //
        if (isWaitingForStart && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }

        // 游戏结束后的处理
        // 只要游戏结束（显示胜利或失败面板），按空格键就重开
        if ((winPanel.activeSelf || losePanel.activeSelf) && Input.GetKeyDown(KeyCode.Space))
        {
            Restart();
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

    //开始按钮点击
    public void OnStartButtonClicked()
    {
        if (startPanel)
            startPanel.SetActive(false);
    }

    void StartGame()
    {
        // 播放背景音乐
        MusicManager.Instance.PlayBGM();
        // 隐藏开始界面
        if (startPanel)
            startPanel.SetActive(false);

        if (player != null)
            player.SetActive(true);

        isWaitingForStart = false;
        isGameActive = true;

    }

    public void GameOver()
    {
        //player.SetActive(false);

        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = true;

        if (losePanel) losePanel.SetActive(true);
        MusicManager.Instance.StopBGM();
        isWaitingForStart = true;
    }

    public void GameWin()
    {
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = false;

        if (winPanel) winPanel.SetActive(true);
        MusicManager.Instance.StopBGM();
        isWaitingForStart = true;
    }

    public void Restart()
    {
        MusicManager.Instance.StopAllMusic();
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}