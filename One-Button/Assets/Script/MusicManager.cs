using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("音效")]
    public AudioClip clickSound;      // 按键音

    [Header("背景音乐")]
    public AudioClip menuBGM;         // 菜单背景音乐（也是失败时的音乐）
    public AudioClip[] levelBGM;      // 关卡音乐数组 [0]=关卡1, [1]=关卡2, [2]=关卡3

    [Header("音频源")]
    public AudioSource bgmSource;      // 专门播放背景音乐的AudioSource
    public AudioSource sfxSource;      // 专门播放音效的AudioSource

    private int currentLevelIndex = -1; // 当前播放的关卡索引
    private bool isWaitingForVictory = false; // 是否在等待胜利后的音乐切换
    private bool isGameStarted = false; // 游戏是否已经开始

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            InitializeAudioSources();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 初始播放菜单音乐
        PlayMenuBGM();
    }

    void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void Update()
    {
        // 检查是否在等待胜利后切换音乐
        if (isWaitingForVictory && !bgmSource.isPlaying)
        {
            PlayMenuBGM();
            isWaitingForVictory = false;
        }
    }

    // 场景加载时的回调
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        string sceneName = scene.name;

        // 每次加载新场景时，重置游戏开始状态
        isGameStarted = false;

        // 根据场景类型决定音乐
        if (sceneName == "Level" || sceneName == "Level 2" || sceneName == "Level 3")
        {
            // 进入游戏场景，但游戏还没开始，继续播放菜单音乐
        }
        else
        {
            // 非游戏场景（如菜单），确保播放菜单音乐
            PlayMenuBGM();
        }
    }

    private void InitializeAudioSources()
    {
        if (bgmSource == null)
        {
            bgmSource = gameObject.AddComponent<AudioSource>();
            bgmSource.loop = true;
            bgmSource.playOnAwake = false;
        }

        if (sfxSource == null)
        {
            sfxSource = gameObject.AddComponent<AudioSource>();
            sfxSource.playOnAwake = false;
        }
    }

    // === GameManager 调用的方法 ===

    // 游戏开始时调用
    public void OnGameStart()
    {
        if (isGameStarted) return; // 防止重复调用

        string sceneName = SceneManager.GetActiveScene().name;

        if (sceneName == "Level")
        {
            PlayLevelBGM(1);
            isGameStarted = true;
        }
        else if (sceneName == "Level 2")
        {
            PlayLevelBGM(2);
            isGameStarted = true;
        }
        else if (sceneName == "Level 3")
        {
            PlayLevelBGM(3);
            isGameStarted = true;
        }
    }

    public void OnGameWin()
    {
        if (bgmSource.isPlaying && currentLevelIndex >= 0)
        {
            bgmSource.loop = false;
            isWaitingForVictory = true;
            isGameStarted = false;
        }
        else
        {
            PlayMenuBGM();
            isGameStarted = false;
        }
    }

    public void OnGameOver()
    {
        PlayMenuBGM();
        isGameStarted = false;
    }

    public void OnGameRestart()
    {
        // 不停止音乐，让音乐继续播放
        // 场景重新加载后，OnSceneLoaded会处理
        isGameStarted = false;
    }

    // === 私有方法 ===

    private void PlayMenuBGM()
    {
        if (menuBGM != null)
        {
            if (bgmSource.clip == menuBGM)
                return;

            PlayBGM(menuBGM);
            currentLevelIndex = -1;
            isWaitingForVictory = false;
        }
    }

    private void PlayLevelBGM(int levelNumber)
    {
        if (levelNumber < 1 || levelNumber > levelBGM.Length)
        {
            return;
        }

        int index = levelNumber - 1;
        if (levelBGM[index] != null)
        {
            // 如果已经在播放这个关卡音乐，就不重复播放
            if (bgmSource.clip == levelBGM[index] && bgmSource.isPlaying)
                return;

            PlayBGM(levelBGM[index]);
            currentLevelIndex = index;
            bgmSource.loop = true;
            isWaitingForVictory = false;
        }
    }

    private void PlayBGM(AudioClip clip)
    {
        if (clip == null) return;

        // 如果正在播放相同的音乐，则不切换
        if (bgmSource.clip == clip && bgmSource.isPlaying)
            return;

        bgmSource.clip = clip;
        bgmSource.Play();
    }

    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
        isWaitingForVictory = false;
    }

 

    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }

    public void SetBGMVolume(float volume)
    {
        bgmSource.volume = Mathf.Clamp01(volume);
    }

    public void SetSFXVolume(float volume)
    {
        sfxSource.volume = Mathf.Clamp01(volume);
    }

    // === 辅助方法 ===


    // 获取当前是否在播放关卡音乐
    public bool IsPlayingLevelBGM()
    {
        return currentLevelIndex >= 0;
    }

    // 获取当前是否游戏已开始
    public bool IsGameStarted()
    {
        return isGameStarted;
    }
}
