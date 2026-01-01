using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    private PlayerControler PlayerControler;

    void Start()
    {

        PlayerControler = GetComponent<PlayerControler>();

    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0))
        {
            PlayerControler.Attack();
        }
    }


}
