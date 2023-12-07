using UnityEngine;

public class Cactus : MonoBehaviour
{
    public float speed;

    private void Update()
    {
        transform.position += new Vector3(-1 * speed * Time.deltaTime, 0, 0);
        if (transform.position.x < -10)
            transform.position = new Vector3(10, -3.25f, 0);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.gameObject.CompareTag("Player"))
        {
            speed = 0;
            Destroy(collision.gameObject);
        }
    }
}
