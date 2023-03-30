using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class PlayerController : MonoBehaviour
{

    public float speed;
    Rigidbody playerRb;
    public static int score;
    public GameObject bulletPrefab;
    public float bulletSpeed;
    private float lastFire;
    public float fireDelay;


    // Start is called before the first frame update
    void Start()
    {
        lastFire = Time.time;
        playerRb = GetComponent<Rigidbody>();
    }

    // Update is called once per frame
    void Update()
    {
        float sides = Input.GetAxis("Horizontal");
        float forwards = Input.GetAxis("Vertical");

        // float shootHorizontal = Input.GetAxis("shootHorizontal");
        // float shootVertical = Input.GetAxis("shootVertical");
        bool shoot = Input.GetButton("shoot");

        // if((shootHorizontal != 0 || shootVertical != 0) && Time.time > (lastFire + fireDelay))
        // {
        //     Shoot(shootHorizontal, shootVertical);
        //     lastFire = Time.time;
        // }

        if ((shoot && Time.time > (lastFire + fireDelay)) && Time.timeScale > 0f)
        {
            ShootFront();
            lastFire = Time.time;
        }

        playerRb.velocity = transform.forward.normalized * (forwards * speed);
        playerRb.angularVelocity = new Vector3(0, sides * speed, 0);
    }

    void ShootFront()
    {
        GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation) as GameObject;
        BulletController bulletController = bullet.GetComponent<BulletController>();
        Rigidbody bulletRb = bulletController.rb;
        bulletController.transform.Rotate(Vector3.left, 90);
        bulletRb.useGravity = false;
        bulletRb.freezeRotation = true;
        
        bulletController.velocity = playerRb.velocity + transform.forward*bulletSpeed;
    }

    // void Shoot(float x, float y)
    // {
    //     GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation) as GameObject;
    //     bullet.GetComponent<BulletController>().bulletSpeed = bulletSpeed;
    //     bullet.transform.position -= new Vector3(0,0,1); 
    //     Rigidbody rb = bullet.AddComponent<Rigidbody>(); // Add the rigidbody.
    //     rb.useGravity = false;
    //     rb.freezeRotation = true;
    //     rb.velocity = new Vector3(
    //         (x<0)?Mathf.Floor(x) * bulletSpeed : Mathf.Ceil(x) * bulletSpeed,
    //         (y<0)?Mathf.Floor(y) * bulletSpeed : Mathf.Ceil(y) * bulletSpeed,
    //         0
    //     );
    // }
}
