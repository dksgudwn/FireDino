using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Dino : MonoBehaviour
{
    public float jumpForce = 5f; // �����ϴ� ��
    private Rigidbody2D rb; // ĳ������ ������ٵ�
    private bool isGrounded; // ĳ���Ͱ� �ٴڿ� �ִ��� ����

    void Start()
    {
        rb = GetComponent<Rigidbody2D>(); // ������ٵ� ������Ʈ�� ������
    }

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space) && isGrounded) // �����̽��ٸ� ������ �ٴڿ� ������
        {
            rb.AddForce(Vector2.up * jumpForce, ForceMode2D.Impulse); // �� �������� ���� ��
        }
    }

    void OnCollisionEnter2D(Collision2D collision) // �ݶ��̴��� �浹�� ��
    {
        if (collision.gameObject.CompareTag("Ground")) // �浹�� ���ӿ�����Ʈ�� �±װ� Ground�̸�
        {
            isGrounded = true; // �ٴڿ� �ִٰ� ǥ��
        }
    }

    void OnCollisionExit2D(Collision2D collision) // �ݶ��̴��� ������ ��
    {
        if (collision.gameObject.CompareTag("Ground")) // ������ ���ӿ�����Ʈ�� �±װ� Ground�̸�
        {
            isGrounded = false; // �ٴڿ� ���ٰ� ǥ��
        }
    }
}
