using UnityEngine;

public class Mine : MonoBehaviour
{
    private float _speed = 2f;

    [SerializeField] private Player _player;
    [SerializeField] private Animator _mineExplosionAnim;
    [SerializeField] private BoxCollider2D _enemyCollider;
    [SerializeField] private AudioSource _audioSource;

    void Start()
    {
        _player = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
        if (_player == null)
        {
            Debug.LogError("Player is null");
        }
        _mineExplosionAnim = GetComponent<Animator>();
        if (_mineExplosionAnim == null)
        {
            Debug.LogError("Enemy explosion animator is null");
        }

        _enemyCollider = GetComponent<BoxCollider2D>();
        if (_enemyCollider == null)
        {
            Debug.LogError("Enemy Box Collider is null");
        }

        _audioSource = GetComponent<AudioSource>();
        if (_audioSource == null)
        {
            Debug.LogError("Audio source on the enemy is null");
        }
    }

    
    void Update()
    {
        MinesBehavior();
    }

    private void MinesBehavior()
    {
        transform.Translate(Vector3.down * _speed * Time.deltaTime);
        if(transform.position.y <= -6.5f)
        {
            Destroy(this.gameObject);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            if (_player != null)
            {
                _player.Damage();
            }
            _mineExplosionAnim.SetTrigger("OnMineExplosion");
            _audioSource.Play();
            _speed = 0;
            Destroy(_enemyCollider);
            Destroy(this.gameObject, 1f);
        }

        if (other.CompareTag("Laser"))
        {
            _mineExplosionAnim.SetTrigger("OnMineExplosion");
            _audioSource.Play();
            _speed = 0;
            Destroy(other.gameObject);
            Destroy(_enemyCollider);
            Destroy(this.gameObject, 1f);
        }
    }
}
