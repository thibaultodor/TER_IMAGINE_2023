using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    //Explosion Effect
    public GameObject Explosion;

    public float lifeTime;
    public float bulletSpeed;
    public int damage = 50;
    public Vector2 direction;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        StartCoroutine(EndOfBulletDelay());
        direction = rb.velocity;
    }

    IEnumerator EndOfBulletDelay()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Wall")){
            Vector2 normal = collision.contacts[0].normal;
            direction = Vector2.Reflect(direction, normal);
            rb.velocity = direction.normalized * bulletSpeed;
        }
    }
}