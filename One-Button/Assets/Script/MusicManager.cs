using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MusicManager : MonoBehaviour
{
    public static MusicManager Instance { get; private set; }

    [Header("音效")]
    public AudioClip clickSound;      // 按键音
    public AudioClip loseSound;       // 失败音效
    public AudioClip winSound;        // 胜利音效

    [Header("音乐")]
    public AudioClip bgmSound;         // 背景音乐
    public AudioSource bgmSource;      // 专门播放背景音乐的AudioSource
    public AudioSource sfxSource;      // 专门播放音效的AudioSource

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
            return;
        }

        // 设置背景音乐播放器
        if (bgmSource == null)
            bgmSource = gameObject.AddComponent<AudioSource>();

        // 设置音效播放器
        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();
    }

    // 播放背景音乐（从头开始）
    public void PlayBGM()
    {
        if (bgmSound != null)
        {
            bgmSource.clip = bgmSound;
            bgmSource.loop = true;
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

    // 停止背景音乐
    public void StopBGM()
    {
        if (bgmSource.isPlaying)
        {
            bgmSource.Stop();
        }
    }

    // 停止所有音乐/音效
    public void StopAllMusic()
    {
        if (bgmSource.isPlaying)
            bgmSource.Stop();
        if (sfxSource.isPlaying)
            sfxSource.Stop();
    }

    // 播放失败音效
    /*public void PlayLoseSound()
    {
        if (loseSound != null)
        {
            sfxSource.PlayOneShot(loseSound);
        }
    }

    // 播放胜利音效
    public void PlayWinSound()
    {
        if (winSound != null)
        {
            sfxSource.PlayOneShot(winSound);
            Debug.Log("播放胜利音效");
        }
    }*/

}
