using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dino : MonoBehaviour
{
    public float jumpForce = 5f; // 점프하는 힘
    private Rigidbody2D rb; // 캐릭터의 리지드바디
    private bool isGrounded; // 캐릭터가 바닥에 있는지 여부

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // 리지드바디 컴포넌트를 가져옴
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // 스페이스바를 누르고 바닥에 있으면
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // 윗 방향으로 힘을 줌
        }
    }

    void OnCollisionEnter2D(Collision2D collision) // 콜라이더와 충돌할 때
    {
        if (collision.gameObject.CompareTag("Ground")) // 충돌한 게임오브젝트의 태그가 Ground이면
        {
            isGrounded = true; // 바닥에 있다고 표시
        }
    }

    void OnCollisionExit2D(Collision2D collision) // 콜라이더와 떨어질 때
    {
        if (collision.gameObject.CompareTag("Ground")) // 떨어진 게임오브젝트의 태그가 Ground이면
        {
            isGrounded = false; // 바닥에 없다고 표시
        }
    }
}
