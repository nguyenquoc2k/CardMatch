using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    [Header("Setup")] [SerializeField] private GameObject homeUI;
    [SerializeField] private GameObject cardHarmonyPrefab;
    [SerializeField] private GameObject continueButton;
    private GameObject currentGame;

    private void Awake()
    {
        Instance = this;
        CheckStateButton();
    }

    public void CheckStateButton()
    {
        string savePath = Application.persistentDataPath + "/card_harmony_save.json";
        bool hasSave = System.IO.File.Exists(savePath);
        continueButton.SetActive(hasSave);
    }
    // Gọi khi nhấn nút Start
    public void OnClickNewGame()
    {
        homeUI.SetActive(false);
        currentGame = Instantiate(cardHarmonyPrefab);
        currentGame.transform.SetParent(transform);

        var logic = currentGame.GetComponentInChildren<MultiplayerGames.CardHarmony.CardHarmonyLM>();
        logic.SpawnCardGrid();

        AudioManager.Instance?.MuteMusicTemporarily(true);
    }

    public void OnClickContinue()
    {
        homeUI.SetActive(false);
        currentGame = Instantiate(cardHarmonyPrefab);
        currentGame.transform.SetParent(transform);

        var logic = currentGame.GetComponentInChildren<MultiplayerGames.CardHarmony.CardHarmonyLM>();
        logic.LoadFromSavedGame();

        AudioManager.Instance?.MuteMusicTemporarily(true);
    }

    
    public void ReturnToMenu()
    {
        if (currentGame != null)
        {
            Destroy(currentGame);
        }

        homeUI.SetActive(true);
        AudioManager.Instance?.MuteMusicTemporarily(false);
    }
}