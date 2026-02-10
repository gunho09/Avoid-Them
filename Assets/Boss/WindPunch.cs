using UnityEngine;

public class WindPunch : MonoBehaviour
{
    public float speed = 10f;
    public float damage = 40f;
    public float lifeTime = 3f;

    private Vector2 moveDirection;

    public void Setup(Vector2 dir, float dmg)
    {
        moveDirection = dir.normalized;
        damage = dmg;

        float angle = Mathf.Atan2(moveDirection.y, moveDirection.x) * Mathf.Rad2Deg;
        
        transform.rotation = Quaternion.Euler(0, 0, angle - 90);

        Destroy(gameObject, lifeTime);
    }

    void Update()
    {
        transform.position += (Vector3)moveDirection * speed * Time.deltaTime;
    }

    void OnTriggerEnter2D(Collider2D collision)
    {
        if (collision.CompareTag("Player"))
        {
            PlayerControler player = collision.GetComponent<PlayerControler>();
            if (player != null)
            {
                player.TakeDamage(damage);
            }
            Destroy(gameObject);
        }
        else if (collision.gameObject.layer == LayerMask.NameToLayer("Wall"))
        {
            Destroy(gameObject);
        }
    }
}