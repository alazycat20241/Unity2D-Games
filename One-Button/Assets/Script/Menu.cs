using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Menu : MonoBehaviour
{
    public Button startButton;
    public Button quitButton;

    void Start()
    {
        if (startButton)
            startButton.onClick.AddListener(StartGame);

        if (quitButton)
            quitButton.onClick.AddListener(Quit);
    }

    void StartGame()
    {
        SceneManager.LoadScene(1); // º”‘ÿ”Œœ∑≥°æ∞
    }

    void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
        Application.Quit();
#endif
    }
}
