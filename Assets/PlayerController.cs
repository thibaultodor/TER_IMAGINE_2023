using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    public float speed;
    Rigidbody PlayerBody;
    public static int score;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    private float lastFire;
    public float fireDelay;


    // Start is called before the first frame update
    void Start()
    {
        PlayerBody = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float horizontal = Input.GetAxis("Horizontal");
        float vertical = Input.GetAxis("Vertical");

        float shootHorizontal = Input.GetAxis("shootHorizontal");
        float shootVertical = Input.GetAxis("shootVertical");

        if((shootHorizontal != 0 || shootVertical != 0) && Time.time > (lastFire + fireDelay))
        {
            Shoot(shootHorizontal, shootVertical);
            lastFire = Time.time;
        }

        PlayerBody.velocity = new Vector3(horizontal*speed,vertical*speed,0);
    }

    void Shoot(float x, float y)
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation) as GameObject;
        bullet.transform.position -= new Vector3(0,0,1); 
        Rigidbody rb = bullet.AddComponent<Rigidbody>(); // Add the rigidbody.
        rb.useGravity = false;
        rb.freezeRotation = true;
        rb.velocity = new Vector3(
            (x<0)?Mathf.Floor(x) * bulletSpeed : Mathf.Ceil(x) * bulletSpeed,
            (y<0)?Mathf.Floor(y) * bulletSpeed : Mathf.Ceil(y) * bulletSpeed,
            0
        );
    }
}
