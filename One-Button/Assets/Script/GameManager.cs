using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public GameObject winPanel;
    public GameObject player;

    [Header("重新开始界面")]
    public GameObject restartPanel;  // 面板
    public Button restartButton;      // 开始按钮
    public Button quitButton;       // 退出按钮

    [HideInInspector] public bool isGameActive = false;
    [HideInInspector] public bool isGameOver = false;
    [HideInInspector] private bool isGameWin= false;

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

        if (winPanel) winPanel.SetActive(false);
        if (restartPanel) restartPanel.SetActive(false);
        // 绑定按钮事件
        if (restartButton)restartButton.onClick.AddListener(Restart);

        if (quitButton)quitButton.onClick.AddListener(Quit);

        // 游戏初始不开始
        isGameActive = false;
    }

    void Update()
    {
        if (!isGameActive && !winPanel.activeSelf && !restartPanel.activeSelf && Input.GetKeyDown(KeyCode.Space))
        {
            StartGame();
        }


        if (isGameWin)
        {
            if (Input.GetKeyDown(KeyCode.Space))
            {
                LoadNextLevel();
            }
        }
    }


    void StartGame()
    {
        // 通知MusicManager游戏开始了
        MusicManager.Instance.OnGameStart();
        // 延迟激活玩家
        StartCoroutine(DelayedPlayerActivation());
    }
    IEnumerator DelayedPlayerActivation()
    {
        // 等待设定的延迟时间
        yield return new WaitForSeconds(startDelay);

        isGameActive = true;
    }
    public void GameOver()
    {
        // 通知MusicManager游戏失败
        MusicManager.Instance.OnGameOver();
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = true;

        if (restartPanel) restartPanel.SetActive(true);
    }

    public void GameWin()
    {
        // 通知MusicManager游戏胜利
        MusicManager.Instance.OnGameWin();
        if (!isGameActive) return;
        isGameActive = false;
        isGameOver = false;
        isGameWin = true;
        if (winPanel) winPanel.SetActive(true);
    }

    public void Restart()
    {
        // 通知MusicManager游戏重启
        MusicManager.Instance.OnGameRestart();
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

    public void LoadNextLevel()
    {
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        int nextSceneIndex = currentSceneIndex + 1;

        // 检查是否有下一关
        if (nextSceneIndex < SceneManager.sceneCountInBuildSettings)
        {
            SceneManager.LoadScene(nextSceneIndex);
        }
        else
        {
            //返回主菜单
            SceneManager.LoadScene(0);
        }
    }
}