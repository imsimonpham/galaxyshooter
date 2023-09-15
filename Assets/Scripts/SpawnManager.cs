using System.Collections;
using UnityEngine;

public class SpawnManager : MonoBehaviour
{
    // Movement and Speed
    private int _movementProbabilityInt;
    private string _movement;

    //Health
    private float _bossHealth;
    private float _bossMaxHealth;

    // GameObjects, Prefabs, and Components
    [SerializeField] private GameObject _regularEnemyPrefab;
    [SerializeField] private GameObject _mediumEnemyPrefab;
    [SerializeField] private GameObject _chargingEnemyPrefab;
    [SerializeField] private GameObject _smartEnemyPrefab;
    [SerializeField] private GameObject _dodgingEnemyPrefab;
    [SerializeField] private GameObject _bossPrefab;
    [SerializeField] private GameObject _enemyContainer;
    [SerializeField] private GameObject _minePrefab;
    [SerializeField] private UIManager _uiManager;
    [SerializeField] private GameObject[] _powerUps;

    // Firing and Cooldown
    private Enemy _enemy;
    private Enemy _regularEnemy;
    private Enemy _mediumEnemy;
    private Enemy _chargingEnemy;
    private Enemy _smartEnemy;
    private Enemy _boss;
    private SpriteRenderer _bossSpriteRenderer;
    private Mine _mine;

    // State and Flags
    private int[] _enemiesPerWaveArray = new int[] { 1, 1, 1};
    private int _totalWaves = 3;
    private bool _stopSpawning = false;


    void Start()
    {
        _regularEnemy = _regularEnemyPrefab.GetComponent<Enemy>();
        _mediumEnemy = _mediumEnemyPrefab.GetComponent<Enemy>();
        _chargingEnemy = _chargingEnemyPrefab.GetComponent<Enemy>();
        _smartEnemy = _smartEnemyPrefab.GetComponent<Enemy>();
        _boss = _bossPrefab.GetComponent<Enemy>();
        
        _mine = _minePrefab.GetComponent<Mine>();

        if (_regularEnemy == null )
        {
            Debug.LogError("Regular Enemy is null");
        } 
        if (_mediumEnemy == null)
        {
            Debug.LogError("Medium Enemy is null");
        }
        if (_chargingEnemy == null)
        {
            Debug.LogError("Charging Enemy is null");
        }
        if (_smartEnemy == null)
        {
            Debug.LogError("Smart Enemy is null");
        }
        if ( _boss == null)
        {
            Debug.LogError("Boss is null");
        }
        if (_mine == null)
        {
            Debug.LogError("Mine is null");
        }        
    }
    
    public void StartSpawning()
    {
        StartCoroutine(SpawnWaves());
        StartCoroutine(SpawnPowerUpRoutine());
    }

    IEnumerator SpawnWaves()
    {
        for (int wave = 0; wave < _totalWaves; wave++)
        {
            _uiManager.DisplayWaveText(wave + 1, _totalWaves);
            int enemiesInWave = _enemiesPerWaveArray[wave];
            if(wave < (_totalWaves - 1))
            {
                yield return StartCoroutine(SpawnEnemyRoutine(enemiesInWave));
            } else if (wave == (_totalWaves - 1))
            {
                SpawnBoss();
                yield return new WaitForSeconds(5f);
                yield return StartCoroutine(SpawnEnemyRoutine(enemiesInWave));
            }

            // Wait until all enemies in the wave are cleared
            while (_enemyContainer.transform.childCount > 0)
            {
                yield return null;
            }
        }
    }

    IEnumerator SpawnEnemyRoutine(int enemiesInWave)
    {
        for (int i = 1; i <= enemiesInWave; i++)
        {
            if (_stopSpawning) yield break;
            yield return new WaitForSeconds(2.0f);
            SpawnEnemies();
            yield return new WaitForSeconds(1.5f);
        }
    }

    IEnumerator SpawnPowerUpRoutine()
    {
        while (!_stopSpawning)
        {
            yield return new WaitForSeconds(10f);
            Vector3 posToSpawn = new Vector3(Random.Range(-9.3f, 9.3f), 6f, 0);
            int _powerUpProbabilityInt = Random.Range(1, 101);
            switch (_powerUpProbabilityInt)
            {
                case <= 15:
                    Instantiate(_powerUps[0], posToSpawn, Quaternion.identity);
                    break;
                case <= 30:
                    Instantiate(_powerUps[1], posToSpawn, Quaternion.identity);
                    break;
                case <= 40:
                    Instantiate(_powerUps[2], posToSpawn, Quaternion.identity);
                    break;
                case <= 80:
                    Instantiate(_powerUps[3], posToSpawn, Quaternion.identity);
                    break;
                case <= 85:
                    Instantiate(_powerUps[4], posToSpawn, Quaternion.identity);
                    break;
                case <= 90:
                    Instantiate(_powerUps[5], posToSpawn, Quaternion.identity);
                    break;
                case <= 95:
                    Instantiate(_powerUps[6], posToSpawn, Quaternion.identity);
                    break;
                case <= 100:
                Instantiate(_powerUps[7], posToSpawn, Quaternion.identity);
                    break;
                default:
                    Debug.Log("Default");
                    break;
            }
        }
    }

    private void CalculateEnemyMovementProbability(Enemy enemy)
    {
        if (enemy != null)
        {
            _movementProbabilityInt = Random.Range(1, 101);
            if (_movementProbabilityInt <= 70)
            {
                _movement = "Down";
                enemy.SetMovement(_movement);
            }
            else
            {
                _movement = "Zigzag";
                enemy.SetMovement(_movement);
            }
        }
        else
        {
            Debug.LogError("Enemy is null");
        }
    }

    private void SpawnEnemies()
    {
        Vector3 posToSpawn = new Vector3(Random.Range(-9.3f, 9.3f), 6f, 0);
        Vector3 smartEnemyPosToSpawn = new Vector3(-9.7f, 6f, 0);
        Vector3 bossPosToSpawn = new Vector3(0, 10f, 0);
        int _enemyTypeProbability = Random.Range(1, 101);
        int _enemyShieldProbability = Random.Range(1, 101); 
        if(_enemyTypeProbability <= 50)
        {
            GameObject newEnemy = Instantiate(_regularEnemyPrefab, posToSpawn, Quaternion.identity);
            if(_enemyShieldProbability <= 30)
            {
                _regularEnemy.ActiveEnemyShield();
            } else
            {
                _regularEnemy.DeactivateEnemyShield();
            }
            newEnemy.transform.parent = _enemyContainer.transform;
            _enemy = newEnemy.GetComponent<Enemy>();
            CalculateEnemyMovementProbability(_enemy);
        } else if (_enemyTypeProbability <= 65)
        {
            GameObject newEnemy = Instantiate(_mediumEnemyPrefab, posToSpawn, Quaternion.identity);
            if (_enemyShieldProbability <= 30)
            {
                _mediumEnemy.ActiveEnemyShield();
            } else
            {
                _mediumEnemy.DeactivateEnemyShield();
            }
            newEnemy.transform.parent = _enemyContainer.transform;
        }
        else if (_enemyTypeProbability <= 75)
        {
            GameObject newEnemy = Instantiate(_chargingEnemyPrefab, posToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
        }
        else if (_enemyTypeProbability <= 85)
        {
            GameObject newEnemy = Instantiate(_smartEnemyPrefab, smartEnemyPosToSpawn, Quaternion.identity) ;
            newEnemy.transform.parent = _enemyContainer.transform;
        }
        else
        {
            GameObject newEnemy = Instantiate(_dodgingEnemyPrefab, smartEnemyPosToSpawn, Quaternion.identity);
            newEnemy.transform.parent = _enemyContainer.transform;
        }
    }

    public GameObject GetBossGameObject()
    {
        return _bossPrefab;
    }

    private void SpawnBoss()
    {
        Vector3 posToSpawn = new Vector3(0, 10f, 0);
        GameObject newEnemy = Instantiate(_bossPrefab, posToSpawn, Quaternion.identity);
        _bossSpriteRenderer = newEnemy.GetComponent<SpriteRenderer>();
        if(_bossSpriteRenderer == null)
        {
            Debug.LogError("Boss Sprite Renderer is null");
        }
        newEnemy.transform.parent = _enemyContainer.transform;
    }

    public IEnumerator BossDamageVisualRoutine()
    {
        _bossSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
        yield return new WaitForSeconds(0.1f);
        _bossSpriteRenderer.color = new Color(1f, 1f, 1f, 0.85f);
        yield return new WaitForSeconds(0.1f);
        _bossSpriteRenderer.color = new Color(1f, 1f, 1f, 1f);
    }



    public void SpawnMines()
    {
        Vector3 posToSpawn = new Vector3(Random.Range(-9.3f, 9.3f), 6f, 0);
        GameObject newEnemy = Instantiate(_minePrefab, posToSpawn, Quaternion.identity);
        newEnemy.transform.parent = _enemyContainer.transform;
    }

    public void OnPlayerDeath()
    {
        _stopSpawning = true;
    }

    public void OnPlayerWinning()
    {
        _stopSpawning = true;
    }

    private void StartEnemySpawn()
    {
        _stopSpawning = false;
    }
}
