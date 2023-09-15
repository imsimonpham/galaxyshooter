using UnityEngine;
using TMPro;
using UnityEngine.UI;
using System.Collections;
using Unity.VisualScripting;

public class UIManager : MonoBehaviour
{
    [SerializeField] private TMP_Text _scoreText;
    [SerializeField] private TMP_Text _liveText;
    [SerializeField] private GameObject _pauseImage;
    [SerializeField] private Image _fuelBarImg;
    [SerializeField] private Sprite[] _liveSprites;
    [SerializeField] private Sprite[] _fuelBarSprites;
    [SerializeField] private TMP_Text _gameOverText;
    [SerializeField] private TMP_Text _winingText;
    [SerializeField] private TMP_Text _restartText;
    [SerializeField] private TMP_Text _ammoCountText;
    [SerializeField] private TMP_Text _waveText;
    [SerializeField] private Image _bossHealthBar;
    [SerializeField] private Image _bossHealthBarContainer;
    [SerializeField] private GameObject _bossPrefab;
    private Enemy _boss;
    private Player _player;
    private GameManager _gameManager;
    [SerializeField] private float _bossMaxHealth;


    void Start()
    {
       _gameOverText.gameObject.SetActive(false);
       _restartText.gameObject.SetActive(false);
       _winingText.gameObject.SetActive(false);
       _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
       if (_gameManager == null)
       {
           Debug.LogError("Game Manager is null");
       }
       _player = GameObject.Find("Player").GetComponent<Player>();
        if(_player == null)
        {
            Debug.LogError("Player is null");
        }
        _boss = _bossPrefab.GetComponent<Enemy>();
        if (_boss == null)
        {
            Debug.LogError("Boss is null");
        }


        _bossMaxHealth = _boss.GetBossMaxHealth();
    }

    public GameObject GetPauseImage()
    {
        return _pauseImage;
    }



    public void BossHealthBarUIUpdate()
    {
        _bossHealthBar.fillAmount -= 1f / _bossMaxHealth;
    }

    public void ShowBossHealthBar()
    {
        _bossHealthBarContainer.gameObject.SetActive(true);
    }

    public void UpdateScore(int playerScore)
    { 
        _scoreText.text = "" + playerScore;
    }

    public void UpdateAmmo(int playerAmmo)
    {
        _ammoCountText.text = "" + playerAmmo;
    }

   public void UpdateLives(int currentLives)
    {
        _liveText.text = "x " + currentLives;
        
        if (currentLives == 0)
        {
           DisplayGameOverText();
           DisplayRestartText();
           _gameManager.GameOver();
        }
    }

    public void UpdateFuelBar(int fuelLevel)
    {
        _fuelBarImg.sprite = _fuelBarSprites[fuelLevel];
    }

    public void DisplayWinningText()
    {
        _winingText.gameObject.SetActive(true);
    }

    public void DisplayGameOverText()
    {
        _gameOverText.gameObject.SetActive(true);
        StartCoroutine(FlickerRoutine());
    }

    public void DisplayRestartText()
    {
        _restartText.gameObject.SetActive(true);
    }

    public void DisplayWaveText(int currentWave, int totalWaves)
    {
        _waveText.gameObject.SetActive(true);
        _waveText.text = "Wave " + currentWave + "/" + totalWaves;
        StartCoroutine(DisplayWaveTextRoutine());
    }

    IEnumerator FlickerRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.4f);
            _gameOverText.text = "";
            yield return new WaitForSeconds(0.4f);
            _gameOverText.text = "GAME OVER";
        }
    }

    IEnumerator DisplayWaveTextRoutine()
    {
        yield return new WaitForSeconds(2.0f);
        _waveText.CrossFadeAlpha(0.0f, 1.5f, false);
        yield return new WaitForSeconds(1.5f);
        _waveText.gameObject.SetActive(false);
        _waveText.CrossFadeAlpha(1.0f, 0f, false);
    }
}
