using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public class MainMenuManager : MonoBehaviour
{
    [SerializeField] private Animator playerAnimator;

    private List<string> playerAnimations = new()
    {
        "Burpee",
        "Jumping Jacks",
        "Situps",
    };

    private void Start()
    {
        playerAnimator.Play(playerAnimations[Random.Range(0, playerAnimations.Count)]);
        AudioManager.instance.StopPlayingAll();
        AudioManager.instance.PlaySongsRandomly();
    }

    public void StartGame(int sceneNumber)
    {
        SceneManager.LoadScene(sceneNumber);
    }

    public void ExitGame()
    {
        Application.Quit();
    }
}
