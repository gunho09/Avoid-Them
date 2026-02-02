using UnityEngine;
using System.Collections;


public class apocalipsManager : MonoBehaviour
{

    public GameObject Zombie;
    public Transform spawnPoint;
    public float spawnInterval = 1.5f;

    void Start()
    {
        StartCoroutine(SpawnRoutine());
    }

    IEnumerator SpawnRoutine()
    {
        while (true)
        {

            Instantiate(Zombie, spawnPoint.position, Quaternion.identity);
            yield return new WaitForSeconds(spawnInterval);

        }

    }
}
