using UnityEngine;
using System.Collections;
using UnityEngine.UI;
using Cinemachine;

public class Player : MonoBehaviour
{
    // Movement, Position and Speed
    [SerializeField] private float _speed = 5f;
    [SerializeField] private float _powerUpSpeed = 7f;
    private float _inputHorizontal, _inputVertical;
    Vector3 playerCurrentDirection;

    // Firing, Fuel and Cooldown
    [SerializeField] private float _fireRate = 0.15f;
    private float _canFire = -1f;
    private int _maxAmmoCount = 100;
    private float _maxFuelAmount = 100f;
    private float _fuelAmountBurntPerSec = 15f;
    private float _fuelAmountRefilledPerSec = 20f;

    // GameObjects, Prefabs, and Components
    [SerializeField] private GameObject _laserPrefab;
    [SerializeField] private GameObject _homingMissilePrefab;
    [SerializeField] private GameObject _tripleShotPrefab;
    [SerializeField] private GameObject _megaBlastPrefab;
    [SerializeField] private GameObject _shield;
    [SerializeField] private GameObject _thruster;
    [SerializeField] private GameObject _leftEngine, _rightEngine;
    [SerializeField] private AudioClip _laserSound;
    [SerializeField] private AudioClip _outOfAmmoSound;
    [SerializeField] private AudioClip _playerExplosionSound;
    [SerializeField] private AudioClip _gunDisabledSound;
    [SerializeField] private AudioClip _playerGettingHitSound;
    [SerializeField] private AudioSource _audioSource;
    [SerializeField] private Image _fuelBar;

    // State and Flags
    [SerializeField] private int _lives = 3;
    [SerializeField] private int _fuelLevel = 1;
    [SerializeField] private int _shieldLives = 0;
    [SerializeField] private int _score;
    [SerializeField] private int _ammoCount;
    [SerializeField] private GameObject _powerUp;
    [SerializeField] private bool _isTripleShotActive = false;
    [SerializeField] private bool _isShieldActive = false;
    [SerializeField] private bool _isOutOfAmmo = false;
    [SerializeField] private bool _isHomingProjectitleActive = false;
    [SerializeField] private bool _isMegaBlastActive = false;
    [SerializeField] private bool _isSpeedBoostActive = false;
    [SerializeField] private bool _isGunDisabled = false;
    [SerializeField] private bool _hasPressedKeyC = false;

    // Managers and Components
    private SpawnManager _spawnManager;
    private UIManager _uiManager;
    private BoxCollider2D _playerCollider;
    private ParticleSystem _shieldParticleSystem;
    private CameraShakeManager _cameraShakeManager;
    private CinemachineImpulseSource _impulseSource;
    private Animator _playerExplosion;



    // Start is called before the first frame update
    void Start()
    {
        transform.position = new Vector3(0, 0, 0);
        _ammoCount = _maxAmmoCount;
        _spawnManager = GameObject.Find("SpawnManager").GetComponent<SpawnManager>();
        _uiManager = GameObject.Find("Canvas").GetComponent<UIManager>();

        if (_spawnManager == null)
        {
            Debug.LogError("Spawn Manager is NULL");
        }

        if (_uiManager == null)
        {
            Debug.LogError("UI Manger is NULL");
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError("Audio Source on the Player is null");
        }
        
        _playerExplosion = GetComponent<Animator>();
        if (_playerExplosion == null)
        {
            Debug.LogError("Player Explosion animator is null");
        }

        _playerCollider = GetComponent<BoxCollider2D>();
        if (_playerCollider == null)
        {
            Debug.LogError("Player Collider == null");
        }

        _shieldParticleSystem = _shield.GetComponent<ParticleSystem>();
        if (_shieldParticleSystem == null)
        {
            Debug.LogError("Shield Particle System is null");   
            Debug.LogError("Shield Particle System is null");   
        }

        _cameraShakeManager = GameObject.Find("CameraShakeManager").GetComponent<CameraShakeManager>();
        if (_cameraShakeManager == null)
        {
            Debug.LogError("Camera Shake Manager is null");
        }
        
        _impulseSource = GetComponent<CinemachineImpulseSource>();
        if (_impulseSource == null)
        {
            Debug.LogError("Impulse Source is null");
        }
    } 

    // Update is called once per frame
    void Update()
    {
        CalculateMovement();
        if (Input.GetKeyDown(KeyCode.Space) && Time.time > _canFire)
        {
            _canFire = Time.time + _fireRate;
            Fire();
        }
        _powerUp = GameObject.FindWithTag("PowerUp");

        if (Input.GetKeyDown(KeyCode.C))
        {
            _hasPressedKeyC = true;
        }

        if (_hasPressedKeyC)
        {
            CollectPowerUps();
            if (_powerUp == null)
            {
                _hasPressedKeyC = false;
            }   
        }
       
        if (_fuelBar.fillAmount >= 0.4)
        {
            _fuelLevel = 1;
        }
        else
        {
            _fuelLevel = 0;
        }        
        _uiManager.UpdateFuelBar(_fuelLevel); 
        ActivateSpeedBoostOnFuel();
    }

    void CalculateMovement()
    {
        _inputHorizontal = Input.GetAxis("Horizontal");
        _inputVertical = Input.GetAxis("Vertical");
        Vector3 direction = new Vector3(_inputHorizontal, _inputVertical, 0);
        transform.Translate(direction * _speed * Time.deltaTime);
       
        transform.position = new Vector3(Mathf.Clamp(transform.position.x, -9.6f, 9.6f), Mathf.Clamp(transform.position.y,-5f, 5f) ,0);
       
    }

    public void Fire()
    {
        if (_ammoCount > 0 && !_isGunDisabled)
        {
            if (_isMegaBlastActive)
            {
                Instantiate(_megaBlastPrefab, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            }
            else if (_isTripleShotActive)
            {
                Instantiate(_tripleShotPrefab, new Vector3(transform.position.x, transform.position.y - 0.3f, 0), Quaternion.identity);
            }
            else if (_isHomingProjectitleActive) 
            {
                Instantiate(_homingMissilePrefab, new Vector3(transform.position.x, transform.position.y + 0.7f, 0), Quaternion.identity);
            } 
            else
            {
                Instantiate(_laserPrefab, new Vector3(transform.position.x, transform.position.y + 0.7f, 0), Quaternion.identity);
            }
            _ammoCount--;
            _uiManager.UpdateAmmo(_ammoCount);
            _audioSource.PlayOneShot(_laserSound);
        }
        else
        {
            if (_ammoCount == 0)
            {
                _isOutOfAmmo = true;
                _audioSource.PlayOneShot(_outOfAmmoSound);
            } else
            {
                _audioSource.PlayOneShot(_gunDisabledSound);
            }
        }
    }

    public void Damage()
    {
        _audioSource.PlayOneShot(_playerGettingHitSound);
        if (_isShieldActive)
        {
            ShieldDamage();
            _cameraShakeManager.CameraShake(_impulseSource);
            return;
        }
        
        _lives --;
        _uiManager.UpdateLives(_lives);
        _cameraShakeManager.CameraShake(_impulseSource);

        if (_lives == 2)
        {
            _leftEngine.SetActive(true);
        } else if (_lives == 1)
        {
            _rightEngine.SetActive(true);
        } else if (_lives == 0)
        {
            _spawnManager.OnPlayerDeath();
            _playerExplosion.SetTrigger("OnPlayerDeath");
            _audioSource.PlayOneShot(_playerExplosionSound);
            Destroy(_playerCollider);
            foreach(Transform child in transform)
            {
                Destroy(child.gameObject);
            }
            Destroy(this.gameObject, 1f);
        }
    }

    private void CollectPowerUps()
    {
        if (_powerUp != null)
        {
            playerCurrentDirection = transform.position - _powerUp.transform.position;
            playerCurrentDirection.Normalize();
            _powerUp.transform.Translate(playerCurrentDirection * Time.deltaTime * _powerUpSpeed);
        } 
    }

    public void DisableFiring()
    {
        _isGunDisabled = true;
        StartCoroutine(GunDisabledRoutine());
    }

    public void ShieldDamage()
    {
        if (_shieldLives == 3)
        {
            _shieldLives--;
            ChangeShieldVisual(0.50f);
            return;
        }
        else if (_shieldLives == 2)
        {
            _shieldLives--;
            ChangeShieldVisual(0.20f);
            return;
        }
        else if (_shieldLives == 1)
        {
            _shieldLives--;
            _isShieldActive = false;
            _shield.SetActive(false);
            return;
        }
    }


    public void RefillAmmo()
    {
        _ammoCount += 50;
        //_ammoCount = (_ammoCount > _maxAmmoCount) ? _maxAmmoCount : _ammoCount; 
        _uiManager.UpdateAmmo(_ammoCount);
    }

    public float GetMaxAmmoCount()
    {
        return _maxAmmoCount;
    }

    public void ActivateHomingProjectile()
    {
        _isTripleShotActive = false;
        _isMegaBlastActive = false;
        _isHomingProjectitleActive = true;
        StartCoroutine(HomingProjectileDownRoutine());
    }

    public void ActivateMegaBlast()
    {
        _isHomingProjectitleActive = false;
        _isTripleShotActive = false;
        _isMegaBlastActive = true;
        StartCoroutine(MegaBlastDownRoutine());
    }

    public void ActivateTripleShot()
    {
        _isHomingProjectitleActive = false;
        _isMegaBlastActive = false;
        _isTripleShotActive = true;
        StartCoroutine(TripleShotPowerDownRoutine());
    }

    public void AddExtraLife()
    {  
        _lives++;
        _uiManager.UpdateLives(_lives); 
        if (_lives == 3)
        {
            _leftEngine.SetActive(false);
        }
        else if (_lives == 2)
        {
            _rightEngine.SetActive(false);
        } 
    }

    void ActivateSpeedBoostOnFuel()
    {
        if (_isSpeedBoostActive == false && Input.GetKey(KeyCode.LeftShift))
        {
            if (_fuelBar.fillAmount > 0)
            {
                _speed = 8.0f;
                _fuelBar.fillAmount -= _fuelAmountBurntPerSec/_maxFuelAmount * Time.deltaTime;
                _thruster.transform.localScale = new Vector3 (3f, 3f, 3f);
            }
            else
            {
                if (_isSpeedBoostActive == false)
                {
                    _speed = 5.0f;
                    _thruster.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
                }
            }
        }
        else
        {
            if (_isSpeedBoostActive == false)
            {
                _speed = 5.0f;
                _thruster.transform.localScale = new Vector3 (1.5f, 1.5f, 1.5f);
            }
            _fuelBar.fillAmount += _fuelAmountRefilledPerSec/_maxFuelAmount * Time.deltaTime;
        }
    }
    
    public void ActivateSpeedBoost()
    {
        _speed = 8.0f;
        _isSpeedBoostActive = true;
        _thruster.transform.localScale = new Vector3 (1f, 1f, 1f);
        StartCoroutine(SpeedBoostDownRoutine());
    }

    public void CalculateScore(int points)
    {
        _score += points;
        _uiManager.UpdateScore(_score);
    }

    public void ActivateShield()
    {
        _shield.SetActive(true);
        ChangeShieldVisual(1f);
        _shieldLives = 3;
        _isShieldActive = true;
    }

    void ChangeShieldVisual(float opacity)
    {
        var main = _shieldParticleSystem.main;
        main.startColor = new Color(1f,1f,1f,opacity);
    }

    IEnumerator TripleShotPowerDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isTripleShotActive = false;
    }

    IEnumerator SpeedBoostDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isSpeedBoostActive = false;
        _thruster.transform.localScale = new Vector3 (0.5f, 0.5f, 0.5f);
        _speed = 5.0f;
    }

    IEnumerator MegaBlastDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isMegaBlastActive = false;
    }

    IEnumerator HomingProjectileDownRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isHomingProjectitleActive = false;
    }

    IEnumerator GunDisabledRoutine()
    {
        yield return new WaitForSeconds(5.0f);
        _isGunDisabled = false;
    }
    
}
