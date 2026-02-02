using UnityEngine;

public class poisonplatform : MonoBehaviour
{
    
    public float damage = 30f;
    public PlayerControler PlayerControler;
    private Transform targetCharacter;


    void Start()
    {
        GameObject playerParent = GameObject.FindGameObjectWithTag("Player");
        if (playerParent != null)
        {
            PlayerControler = playerParent.GetComponentInChildren<PlayerControler>();

            if (PlayerControler != null)
            {
                targetCharacter = PlayerControler.transform;
            }
            else
            {
                targetCharacter = playerParent.transform;
            }
        }
    }


    private void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {       
            PlayerControler.TakeDamage(damage);
            PlayerControler.Slow();

        }
    }
    
    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControler.Slow();

        }
    }


    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerControler.Fast();
        }
    }
}
