using System.Collections.Specialized;
using System.Threading;
using UnityEngine;

public class PlayerControler : MonoBehaviour
{

    public float playerSpeed = 5f;

    
    void Update()
    {
        
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");

        Vector3 movement = new Vector3(moveX, moveY, 0f);
        
        transform.Translate(movement * playerSpeed * Time.deltaTime);

    }
}
