using UnityEngine;
using System.Collections;

public class PoisonCloud : MonoBehaviour
{
    [Header("Life")]
    public float lifeTime = 5f;

    void OnEnable()
    {
        StartCoroutine(AutoDestroy());
    }

    IEnumerator AutoDestroy()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
}
