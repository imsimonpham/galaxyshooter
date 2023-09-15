using System;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class GameManager : MonoBehaviour
{
    [SerializeField] private bool _isGameOver = false;
    [SerializeField] private bool _playerWin = false;
    [SerializeField] private GameObject _instructionsUI;
    [SerializeField] private GameObject _menuUI;
    [SerializeField] private bool _isGamePaused;
    private GameObject _pauseImage;
    private UIManager _uiManager;


    private void Start()
    {
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        _pauseImage = _uiManager.GetPauseImage();
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.R) && _isGameOver == true || Input.GetKeyDown(KeyCode.R) && _playerWin == true)
        {
            SceneManager.LoadScene("Game");
        } 
        
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            Application.Quit();
        }

        if (Input.GetKeyDown(KeyCode.P))
        {
            _isGamePaused = !_isGamePaused;
        }

        if (_isGamePaused)
        {
            _pauseImage.SetActive(true);
            Time.timeScale = 0;
        }
        else
        {
            _pauseImage.SetActive(false);
            Time.timeScale = 1;
        }
    }

    public void GameOver()
    {
        _isGameOver = true;
    }
    
    public void PlayerWin()
    {
        _playerWin = true;
    }

    public void ShowInstructions()
    {
        _instructionsUI.SetActive(true);
        _menuUI.SetActive(false);
        
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
