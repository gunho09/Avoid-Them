using UnityEngine;

public class PlayerAttack : MonoBehaviour
{

    private PlayerControler PlayerControler;

    void Start()
    {

        PlayerControler = GetComponent<PlayerControler>();

    }
}
