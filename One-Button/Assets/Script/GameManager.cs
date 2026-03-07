using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject winPanel;
    public Transform finishPoint;
    public GameObject player;

    [Header("重新开始界面")]
    public GameObject restartPanel;  // 面板
    public Button restartButton;      // 开始按钮
    public Button quitButton;       // 退出按钮

    [HideInInspector] public bool isGameActive = false;
    [HideInInspector] public bool isGameOver = false;

    [Header("游戏设置")]
    public float startDelay = 0.5f;  // 玩家延迟开始的时间
    void Awake()
    {
        // 单例保护
        if (Instance == null)
        {
            Instance = this;
        }
        
    }

    void Start()
    {
        // 开始时禁用玩家
        if (player != null)
            player.SetActive(false);

        if (winPanel) winPanel.SetActive(false);
        if (restartPanel) restartPanel.SetActive(false);
        // 绑定按钮事件
        if (restartButton)restartButton.onClick.AddListener(Restart);

        if (quitButton)quitButton.onClick.AddListener(Quit);

        // 游戏初始不开始
        isGameActive = false;

        // 不播放音乐，等待开始
        MusicManager.Instance.StopAllMusic();
    }

    void Update()
    {
        if (!isGameActive && !winPanel.activeSelf && !restartPanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
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
        // 播放背景音乐
        MusicManager.Instance.PlayBGM();
        // 延迟激活玩家
        StartCoroutine(DelayedPlayerActivation());
    }
    IEnumerator DelayedPlayerActivation()
    {
        // 等待设定的延迟时间
        yield return new WaitForSeconds(startDelay);

        // 激活玩家并开始游戏
        if (player != null)
            player.SetActive(true);
        isGameActive = true;
    }
    public void GameOver()
    {

        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = true;

        if (restartPanel) restartPanel.SetActive(true);
        MusicManager.Instance.StopBGM();
    }

    public void GameWin()
    {
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = false;

        if (winPanel) winPanel.SetActive(true);
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