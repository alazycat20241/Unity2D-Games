using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("音效")]
    public AudioClip clickSound;      // 按键音

    [Header("环境音")]
    public AudioClip bgmSound;         // 背景音乐
    public AudioSource bgmSource;      // 专门播放背景音乐的AudioSource

    public AudioSource sfxSource;     // 专门播放音效的AudioSource

    void Awake()
    {
        // 单例
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);  // 切换场景不销毁
        }
        else
        {
            Destroy(gameObject);
        }

        // 设置背景音乐播放器
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        // 播放背景音乐
        PlayBGM();
    }

    void PlayBGM()
    {
        if (bgmSound != null)
        {
            bgmSource.clip = bgmSound;
            bgmSource.loop = true;      // 循环播放
            bgmSource.Play();
        }
    }

    // 播放按键音
    public void PlayClickSound()
    {
        if (clickSound != null)
        {
            sfxSource.PlayOneShot(clickSound);
        }
    }
}
