using System.Collections;
using UnityEngine;
using Vector3 = UnityEngine.Vector3;


public class Laser : MonoBehaviour
{
    [SerializeField] private float _playerLaserSpeed = 6f;
    [SerializeField] private float _playerLaserSpeedMultiplier = 2f;
    private float _enemyLaserSpeed = 3f;
    private float _laserRotationSpeed = 1000f;
    [SerializeField] private float _radius = 10f;
    [SerializeField] private bool _enemyDetected = false;
    private Collider2D[] _enemyCheck;

    private Player _player;
    [SerializeField] private LayerMask _enemyLayerMask;

    void Start()
    {
        _player = GameObject.Find("Player").GetComponent<Player>();
        if(_player == null)
        {
            Debug.LogError("Player is null"); 
        }
        StartCoroutine(FOVCheckRoutine());
    }

    void Update()
    {
        if (transform.CompareTag("EnemyLaser"))
        {
            Vector3 playerCurrentDirection = _player.transform.position - transform.position;
            if (transform.name == "EnemyHeatSeekingLaser(Clone)" && playerCurrentDirection.magnitude <= 5.0f)
            {
              HeatSeeking();
            } else if(transform.name == "EnemySingleLaser(Clone)")
            {
              TowardPlayer();
            } else
            {
              MoveDown();
            }
        }
        else
        {
            if(transform.name == "HomingMissile(Clone)" && _enemyDetected)
            {
                Homing();
            } else
            {
                MoveUp();
            }
        }
    }

    void MoveUp()
    {   
        transform.Translate(Vector3.up * _playerLaserSpeed * Time.deltaTime);

        if (transform.position.y > 6f || transform.position.x < -9f || transform.position.x > 9f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject, 2f);
            }
            else
            {
                Destroy(this.gameObject, 2f);
            }
        }
    }

    void MoveDown()
    {
        transform.Translate(Vector3.down * _enemyLaserSpeed * Time.deltaTime);
        if (transform.position.y < -9f)
        {
            if (transform.parent != null)
            {
                Destroy(transform.parent.gameObject);
            }  else
            {
                Destroy(this.gameObject);
            }
        }
    }

    void HeatSeeking()
    {
        Vector3 playerCurrentDirection = _player.transform.position - transform.position;
        playerCurrentDirection.Normalize();
        transform.Translate(playerCurrentDirection * Time.deltaTime * _enemyLaserSpeed);
    }

    void TowardPlayer()
    {
        transform.Translate(Vector3.up * Time.deltaTime * _enemyLaserSpeed);
        if (transform.position.y < -9f || transform.position.y > 9f || transform.position.x < -9f || transform.position.x > 9f)
        {
            Destroy(this.gameObject, 2f);
        }
    }

    void Homing()
    {
        if (_enemyCheck.Length > 0)
        {
            // Find the closest target from the _enemyCheck array
            Collider2D closestCollider = _enemyCheck[0];
            float closestDistance = Vector3.Distance(transform.position, closestCollider.transform.position);

            for (int i = 1; i < _enemyCheck.Length; i++)
            {
                float distance = Vector3.Distance(transform.position, _enemyCheck[i].transform.position);
                if (distance < closestDistance)
                {
                    closestCollider = _enemyCheck[i];
                    closestDistance = distance;
                }
            }

            // Move the laser towards the closest target
            Transform target = closestCollider.transform;
            Vector3 directionToTarget = (target.position - transform.position).normalized;
            transform.Translate(directionToTarget * _playerLaserSpeed * _playerLaserSpeedMultiplier * Time.deltaTime, Space.World);
            Quaternion toRotation = Quaternion.LookRotation(Vector3.forward, directionToTarget);
            transform.rotation = Quaternion.RotateTowards(transform.rotation, toRotation, _laserRotationSpeed * Time.deltaTime);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (transform.CompareTag("EnemyLaser") && other.CompareTag("Player"))
        {
            _player.Damage();
            Destroy(this.gameObject);
        }

        if (transform.CompareTag("EnemyLaser") && (other.CompareTag("PowerUp") && other.gameObject.layer == LayerMask.NameToLayer("PowerUp")))
        {
            Destroy(other.gameObject);
            Destroy(this.gameObject);
        }

        if (transform.CompareTag("Laser")  && other.CompareTag("EnemyLaser"))
        {
            Destroy(other.gameObject);
            Destroy(this.gameObject);
        }
    }

    private void FOV()
    {
         _enemyCheck = Physics2D.OverlapCircleAll(transform.position, _radius, _enemyLayerMask);

        if (_enemyCheck.Length > 0)
        {
            //There are enemies in the circle
            _enemyDetected = true; 
        }
        else
        {
            //Nothing in the circle
            _enemyDetected = false;
        }
    }

    private void OnDrawGizmos()
    {
        if(transform.name == "HomingMissile(Clone)")
        {
            Gizmos.color = Color.white;
            Gizmos.DrawWireSphere(transform.position, _radius);
        }
    }

    IEnumerator FOVCheckRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(0.1f);
            FOV();
        }
    }
}
