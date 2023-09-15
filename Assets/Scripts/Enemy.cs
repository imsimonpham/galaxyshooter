using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class Enemy : MonoBehaviour
{
    // Movement and Speed
    [SerializeField] private float _speed = 2f;
    [SerializeField] private float _bossSpeed = 1f;
    [SerializeField] private float _dodgeDistanceMultiplier = 20f;
    [SerializeField] private float _highSpeed = 5f;
    [SerializeField] private float _chargingSpeed = 3.5f;
    [SerializeField] private float _rotationSpeed = 1000f;
    [SerializeField] private string _movement;
    [SerializeField] private float _frequency = 1f;
    [SerializeField] private float _magnitude = 3f;
    private Vector3 _initialPosition;
    private Vector3 _sinOffset;

    // Health
    private float _bossHealth;
    [SerializeField] private float _bossMaxHealth = 100f;

    // Firing and Cooldown
    [SerializeField] private float _fireRate = 3f;
    [SerializeField] private float _fastFireRate = 0.7f;
    private float _canFire = -1f;
    private float _spawnRate = 2f;
    private float _canSpawn = -1f;

    // State and Flags
    [SerializeField] private bool _isDead = false;
    [SerializeField] private bool _startFiring = false;
    [SerializeField] private bool _isEnemyShieldActive = false;
    [SerializeField] private bool _isPowerUpDetected = false;
    [SerializeField] private bool _hasFiredBonusLaser = false;
    private bool _immuneToDamage = true;
    private bool _hasTakenABreak = false;
    private bool _startSpawningMines = false;

    // Detection and Ranges
    [SerializeField] private float _detectionRange = 10f;
    [SerializeField] private float _radius = 3f;
    [SerializeField] [Range(1, 360)]private float _angle = 45f;
    [SerializeField] private bool _playerLaserDetected = false;

    // GameObjects, Prefabs, and Components
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private GameObject _enemyShield;
    [SerializeField] private GameObject _powerUpDetector;
    [SerializeField] private Player _player;
    [SerializeField] private BoxCollider2D _playerCollider;
    [SerializeField] private Animator _enemyAnim;
    [SerializeField] private BoxCollider2D _enemyCollider;
    [SerializeField] private CapsuleCollider2D _bossCollider;
    [SerializeField] private LayerMask _powerUpLayerMask;
    [SerializeField] private LayerMask _playerLaserLayerMask;
    private GameObject _enemyContainer;
    private GameObject _bossPrefab;
    private SpawnManager  _spawnManager;
    private UIManager _uiManager;
    private GameManager _gameManager;


    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("Player is null");
        }

        _playerCollider = _player.GetComponent<BoxCollider2D>();
        if (_playerCollider == null)
        {
            Debug.LogError("Player Collider == null");
        }

        _enemyAnim = GetComponent<Animator>();
        if (_enemyAnim == null)
        {
            Debug.LogError("Enemy explosion animator is null");
        }

        if(transform.name != "Boss(Clone)")
        {
            _enemyCollider = GetComponent<BoxCollider2D>();
            if (_enemyCollider == null)
            {
                Debug.LogError("Enemy Box Collider is null");
            }
        }

        _spawnManager = GameObject.FindGameObjectWithTag("SpawnManager").GetComponent<SpawnManager>();

        if (_spawnManager == null)
        {
            Debug.Log("Spawn Manager is null");
        }

        _bossPrefab = _spawnManager.GetBossGameObject();
        if(_bossPrefab != null)
        {
            _bossCollider = _bossPrefab.GetComponent<CapsuleCollider2D>();
            if (_bossCollider == null)
            {
                Debug.LogError("Boss Capsule Collider is null");
            }
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError("Audio source on the enemy is null");
        }

        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();
        if (_uiManager == null)
        {
            Debug.LogError("UI Manger is NULL");
        }

        _gameManager = GameObject.Find("GameManager").GetComponent<GameManager>();
        if (_gameManager == null)
        {
            Debug.LogError("Game Manager is null");
        }

        _enemyContainer = GameObject.Find("EnemyContainer");
        if (_enemyContainer == null)
        {
            Debug.LogError("Game Manager is null");
        }

        _initialPosition = transform.position;
        _bossHealth = _bossMaxHealth;
        StartCoroutine(FOVCheckRoutine());
    }


    // Update is called once per frame
    void Update()
    {
        CalculateMovement(); 
        if (Time.time > _canFire)
        {
            if (transform.name == "SmartEnemy(Clone)")
            {
                _fireRate = _fastFireRate;
            }
            else 
            {
                if (transform.name != "Boss(Clone)") {
                    _fireRate = Random.Range(5f, 7f);
                }
                if(transform.name != "ChargingEnemy(Clone)" && transform.name != "Boss(Clone)" && transform.name != "SmartEnemy(Clone)" && transform.name != "Mine(Clone)")
                {
                    DetectPowerUps();
                }
            }
            _canFire = Time.time + _fireRate;
            _hasFiredBonusLaser = false;
            if (_isDead == false && transform.name != "ChargingEnemy(Clone)")
            {              
                Fire();
            }
        }

        if (_startSpawningMines == true)
        {
            if (Time.time > _canSpawn)
            {
                _canSpawn = Time.time + _spawnRate;
                _spawnManager.SpawnMines();
            }
        }
    }

    private void Fire()
    {
        if(transform.name == "MediumEnemy(Clone)")
        {
            Instantiate(_laserPrefab, new Vector3(transform.position.x, transform.position.y - 0.3f, 0), Quaternion.identity);
        }
        else if(transform.name == "SmartEnemy(Clone)")
        {
            if (_startFiring)
            {
                Instantiate(_laserPrefab, new Vector3(transform.position.x + 0.3f, transform.position.y + 0.2f, 0), transform.rotation * Quaternion.Euler(0,0,180f));
            }
        } 
        else if (transform.name == "Boss(Clone)") {
            if (_startFiring)
            {
                Instantiate(_laserPrefab, new Vector3(transform.position.x - 0.31f, transform.position.y - 0.5f, 0), Quaternion.identity);
            }
        }
        else if (transform.name != "Mine(Clone)")
        {
            Instantiate(_laserPrefab, new Vector3(transform.position.x, transform.position.y - 0.4f, 0), Quaternion.identity);
        }
    }

    private void CalculateMovement()
    {
        if(!_isDead) {
            Vector3 direction = _player.transform.position - transform.position;
            if(transform.name == "Boss(Clone)")
            {
                BossBehavior();
            }
            else if (transform.name == "ChargingEnemy(Clone)")
            {
                ChargingTowardsPlayer();
            }
            else if (transform.name == "SmartEnemy(Clone)")
            {
                SmartMovement();
            }
            else if (transform.name == "Mine(Clone)")
            {
                _speed = 0;
            }
            else
            {
                if (_movement == "ZigZag")
                {
                    MoveZigzag();
                }
                else
                {
                    MoveDown();
                }
            }
        }
       
        if (transform.name != "Boss(Clone)" && transform.position.y < -5f )
        {
            float randomX = Random.Range(-9.3f, 9.3f);
            transform.position = new Vector3(randomX, 6f, 0);
            _initialPosition = transform.position;
        }
    }   

    private void BossBehavior()
    {
        if (_bossHealth > _bossMaxHealth * 0.4)
        {
            BossMovingDownward();
            if (transform.position.y <= 3f)
            {
                _uiManager.ShowBossHealthBar();
                _bossSpeed = 0;
                BossRangeAttack();
            }
        }
        else
        {
            if (!_hasTakenABreak)
            {
                _bossSpeed = 1f;
                BossMovingUpward();
                if (transform.position.y >= 10f)
                {
                    _hasTakenABreak = true; 
                }
            } else
            {
                BossMovingDownward();
                if (transform.position.y <= 3f)
                {
                    _bossSpeed = 0;
                    _enemyAnim.SetTrigger("OnBossCriticalHealth");
                    BossRangeAttack();
                    _startSpawningMines = true; 
                }
            }   
        }
    }

    public float GetBossMaxHealth()
    {
        return _bossMaxHealth;
    }

    private void BossMovingUpward()
    {
        _immuneToDamage = true;
        _startFiring = false;
        transform.Translate(Vector3.up * _bossSpeed * Time.deltaTime);
    }
    private void BossMovingDownward()
    {
        _immuneToDamage = true;
        _startFiring = false;
        transform.Translate(Vector3.down * _bossSpeed * Time.deltaTime);
    }

    private void BossRangeAttack()
    {
        _startFiring = true;
        _immuneToDamage = false;  
    }

    private void DetectPowerUps()
    {
        RaycastHit2D hitPowerUp = Physics2D.Raycast(_powerUpDetector.transform.position, Vector3.down, _detectionRange, _powerUpLayerMask);
        if (hitPowerUp.collider != null)
        {
            _isPowerUpDetected = true;
            if (!_hasFiredBonusLaser)
            {
                Fire();
                _hasFiredBonusLaser = true;
            }
        }
        else
        { 
            _isPowerUpDetected = false;
        }
    }

    public void SetMovement(string movementName)
    {
        _movement = movementName;
    }

    public void MoveDown (){
        transform.Translate(Vector3.down * _speed * Time.deltaTime);
    }

    public void MoveZigzag()
    {
        _initialPosition += Vector3.down * Time.deltaTime * _speed;
        _sinOffset = transform.right * Mathf.Sin(Time.time * _frequency) * _magnitude;
        transform.position = _initialPosition  + _sinOffset;
    }

    void SmartMovement()
    {
        if (transform.position.y <= -4.3f)
        {
            Vector3 direction = _player.transform.position - transform.position;
            direction.Normalize();
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, direction);
            toRotation *= Quaternion.Euler(0f, 0f, -180f);
            transform.Translate(Vector3.right * Time.deltaTime * _speed, Space.World);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
            _startFiring = true;
        }
        else
        {
            transform.Translate(Vector3.down * _highSpeed * Time.deltaTime);
        }

        if (transform.position.x > 9.3f)
        {
            Destroy(this.gameObject);
        }
    }

    void ChargingTowardsPlayer()
    {
        Vector3 direction = _player.transform.position - transform.position;
        if (direction.magnitude <= 5.0f)
        {
            direction.Normalize();
            transform.Translate(direction * Time.deltaTime * _chargingSpeed, Space.World);
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, direction);
            toRotation *= Quaternion.Euler(0f, 0f, 180f);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
        }
        else
        {
            FaceDown();
            MoveDown();
        }
    }

    void FaceDown()
    {
        Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, Vector3.down);
        toRotation *= Quaternion.Euler(0f, 0f, 180f);
        transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _rotationSpeed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_player != null)
            {
                _player.Damage();
            }
            _enemyAnim.SetTrigger("OnEnemyDeath");
            _audioSource.Play();
            _speed = 0;
            _isDead = true;
            Destroy(_enemyCollider);
            Destroy(this.gameObject, 1f);
        }
        if (other.CompareTag("Laser"))
        {
            Destroy(other.gameObject);
            DamageEnemy();
        }
    }
    
    public void ActiveEnemyShield()
    {
        _enemyShield.SetActive(true);
        _isEnemyShieldActive = true;
    }

    public void DeactivateEnemyShield()
    {
        _enemyShield.SetActive(false);
        _isEnemyShieldActive = false;
    }

    public void DamageEnemy()
    {
        if(transform.name == "Boss(Clone)")
        {
            if (!_immuneToDamage)
            {
                _bossHealth--;
                _uiManager.BossHealthBarUIUpdate();
                StartCoroutine(_spawnManager.BossDamageVisualRoutine());
                _player.CalculateScore(10);
            }
            if (_bossHealth == 0)
            {
                _enemyAnim.SetTrigger("OnBossDeath");
                _audioSource.Play();
                _speed = 0;
                _isDead = true;
                _gameManager.PlayerWin();
                _uiManager.DisplayWinningText();
                _uiManager.DisplayRestartText();
                _spawnManager.OnPlayerWinning();
                Destroy(_playerCollider);
                foreach (Transform child in _enemyContainer.transform)
                {
                    Animator childExplosionAnim =  child.GetComponent<Animator>();
                    childExplosionAnim.SetTrigger("OnEnemyDeath");
                    childExplosionAnim.SetTrigger("OnMineExplosion");
                    Destroy(child.gameObject, 1f);
                    _audioSource.Play();
                    _speed = 0;
                }
                Destroy(_enemyCollider);
                Destroy(this.gameObject, 1f);
            }
        } else
        {
            if (_isEnemyShieldActive)
            {
                _enemyShield.SetActive(false);
                _isEnemyShieldActive = false;
                return;
            }
            _player.CalculateScore(10);
            _enemyAnim.SetTrigger("OnEnemyDeath");
            _audioSource.Play();
            _speed = 0;
            _isDead = true;
            Destroy(_enemyCollider);
            Destroy(this.gameObject, 1f);
        }   
    } 

    private void FOV()
    {
        Collider2D[] rangeCheck = Physics2D.OverlapCircleAll(transform.position, _radius, _playerLaserLayerMask);

        if (rangeCheck.Length > 0)
        {
            Transform target = rangeCheck[0].transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            if (Vector3.Angle(-transform.up, directionToTarget) < _angle / 2)
            {
                //Laser Detected: Target within FOV
                _playerLaserDetected = true;
                transform.Translate(transform.right * _speed * Time.deltaTime * _dodgeDistanceMultiplier);
            }
            else
            {
                //Laser Not Detected: Target outside FOV.
                _playerLaserDetected = false;
            }
        }
        else if (_playerLaserDetected)
        {
            //Laser Not Detected: Target left the circular range.
            _playerLaserDetected = false;
        }
        else
        {
           //Nothing in the circle
        }
    }

    private void OnDrawGizmos()
    {
        if (transform.name == "DodgingEnemy(Clone)")
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _radius);
            Vector3 angle01 = DirectionFromAngle(Mathf.Repeat(-transform.eulerAngles.z + 180f, 360f), -_angle / 2);
            Vector3 angle02 = DirectionFromAngle(Mathf.Repeat(-transform.eulerAngles.z + 180f, 360f), _angle / 2);

            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + angle01 * _radius);
            Gizmos.DrawLine(transform.position, transform.position + angle02 * _radius);
        }    
    }

    private Vector3 DirectionFromAngle(float eulerY, float angleInDegrees)
    {
        angleInDegrees += eulerY;
        return new Vector3(Mathf.Sin(angleInDegrees * Mathf.Deg2Rad), Mathf.Cos(angleInDegrees * Mathf.Deg2Rad), 0);
    }

    IEnumerator FOVCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            if(transform.name == "DodgingEnemy(Clone)")
            {
                FOV();
            }
        }
    }
}
