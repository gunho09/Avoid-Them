using System.Diagnostics;
using UnityEngine;

public class PlayerControler : MonoBehaviour
{
    // 기본 스탯
    public float playerSpeed = 5f;
    public float playerLevel = 1f;
    public float plusHp = 1f;
    public float plusPW = 1f;
    public float numHp = 1f;
    public float numPW = 1f;
    public float exp = 0;
    public float playerStartHp = 100;
    public float playerStartPw = 30;
    public float attackRange = 1.5f;
    public float attackDamage = 10f;
    public LayerMask enemy;

    // 계산된 스탯 (클래스 레벨에서 선언)
    public float PlayerMaxHp;
    public float PlayerDamage;
    public float PlayerCurrentHp;

    void Start()
    {
        // 게임 시작할 때 스탯 계산
        PlayerMaxHp = plusHp + numHp + playerStartHp;
        PlayerDamage = plusPW + numPW + playerStartPw;
        PlayerCurrentHp = PlayerMaxHp;
    }

    void Update()
    {
        float moveX = Input.GetAxis("Horizontal");
        float moveY = Input.GetAxis("Vertical");
        Vector3 movement = new Vector3(moveX, moveY, 0f);
        transform.Translate(movement * playerSpeed * Time.deltaTime);
    }

    public void Attack()
    {
        Collider[] hits = Physics.OverlapSphere(transform.position, attackRange, enemy);
        foreach (Collider hit in hits)
        {
            hit.GetComponent<IDamageable>()?.TakeDamage(attackDamage);
        }
    }

    public void TakeDamage(float damage)
    {
        PlayerCurrentHp -= damage;

        if (PlayerCurrentHp <= 0)
        {
            PlayerCurrentHp = 0;
            Die();
        }
    }

    void Die()
    {
        UnityEngine.Debug.Log("플레이어 사망!");
        // 사망 처리 (나중에 추가)
    }
}