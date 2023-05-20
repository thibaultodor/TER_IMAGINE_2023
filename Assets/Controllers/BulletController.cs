using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float lifeTime;
    public Rigidbody rb;
    public Vector3 velocity
    {
        get { return _velocity; }
        set
        {
            _velocity = value;
            rb.velocity = _velocity;
        }
    }

    public int damage;
    private Vector3 _velocity; 

    void Start()
    {
        damage = 1;
        StartCoroutine(EndOfBulletDelay());
    }

    IEnumerator EndOfBulletDelay()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }
    

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("LeftCondWall") ||
            collision.gameObject.CompareTag("FrontCondWall") ||
            collision.gameObject.CompareTag("BackCondWall") ||
            collision.gameObject.CompareTag("RightCondWall"))
        {
            Vector3 normal = collision.GetContact(0).normal;
            print(velocity);
            velocity = Vector3.Reflect(velocity, normal);
            print(velocity);
            transform.forward = velocity.normalized;
            transform.Rotate(Vector3.left, 90);
        }
        if (collision.gameObject.CompareTag("Wall") || collision.gameObject.CompareTag("Obstacles"))
        {
            Vector3 normal = collision.GetContact(0).normal;
            velocity = Vector3.Reflect(velocity, normal);
            transform.forward = velocity.normalized;
            transform.Rotate(Vector3.left, 90);
        }
        if (collision.gameObject.tag == "AllyBullet" && gameObject.tag == "EnemyBullet")
        {
            Destroy(gameObject);
        }
        if (collision.gameObject.tag == "EnemyBullet" && gameObject.tag == "AllyBullet")
        {
            Destroy(gameObject);
        }
    }
}