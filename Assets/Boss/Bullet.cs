using UnityEngine;

public class Bullet : MonoBehaviour
{
    public float speed = 10f;
    private Vector2 direction;
    private PlayerControler playerControler;


    void Start()
    {
        GameObject playerParent = GameObject.FindGameObjectWithTag("Player");
        playerControler =
    GameObject.FindGameObjectWithTag("Player")
    .GetComponentInChildren<PlayerControler>();
    }

    public void Init(Vector2 dir)
    {
        direction = dir.normalized;
        Destroy(gameObject, 3f);
    }

    void Update()
    {
        transform.Translate(direction * speed * Time.deltaTime);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            playerControler.TakeDamage(10);

        }
    }
}
