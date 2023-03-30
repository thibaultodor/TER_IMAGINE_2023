using UnityEngine;
using UnityEngine.UI;
using UnityEngine.UIElements;

// Tank = 2 rigidbody Mesh ( body + turret ( turret being a child of the body ) )
//      + Box collider 
public class PlayerController : MonoBehaviour
{
    Rigidbody PlayerBody;
    public GameObject Bullet;
    private Transform Turret;
    private Transform bulletSpawnPoint;

    private float curSpeed, targetSpeed, rotSpeed;

    private float turretRotSpeed = 10.0f;
    private float maxForwardSpeed = 5.0f;
    private float maxBackwardSpeed = -5.0f;

    public static int score = 0;

    //Bullet shooting rate
    protected float shootRate = 0.5f;
    protected float elapsedTime;

    // Turret is the child of Tank. bulletSpawnPoint is the child of Turret.
    void Start()
    {
        PlayerBody = GetComponent<Rigidbody>();
        //Tank Settings
        rotSpeed = 150.0f;
        //Get the turret of the tank
        // Turret = gameObject.transform.GetChild(0).transform;
        // bulletSpawnPoint = Turret.GetChild(0).transform;
    }

    void Update()
    {
        UpdateWeapon();
        UpdateControl();
    }

    void UpdateWeapon()
    {
        if (Input.GetMouseButtonDown(0))
        {
            elapsedTime += Time.deltaTime;
            if (elapsedTime >= shootRate) // Cooldown check
            {
                //Reset the time
                elapsedTime = 0.0f;
                //Instantiate the bullet
                Instantiate(Bullet, bulletSpawnPoint.position, bulletSpawnPoint.rotation);
            }
        }
    }

    void UpdateControl()
    {
        //////// WARNING : here, we aim with the mouse
        //////// In the final game, we have two different aiming systems : 
        ////////    - the turret follows the direction in which the tank goes in default mode 
        ////////    - the aiming can be controlled with the joystick when pressing a specific button

        //AIMING WITH THE MOUSE
        //Generate a plane that intersects the transform's
        //position with an upwards normal.
        Plane playerPlane = new Plane(Vector3.up, transform.position + new Vector3(0, 0, 0));
        // Generate a ray from the cursor position
        Ray RayCast = Camera.main.ScreenPointToRay(Input.mousePosition);
        //Determine the point where the cursor ray intersects
        //the plane.
        float HitDist = 0;
        // If the ray is parallel to the plane, Raycast will
        //return false.
        if (playerPlane.Raycast(RayCast, out HitDist))
        {
            //Get the point along the ray that hits the
            //calculated distance.
            Vector3 RayHitPoint = RayCast.GetPoint(HitDist);
            Quaternion targetRotation = Quaternion.LookRotation(RayHitPoint - transform.position);
            Turret.transform.rotation = Quaternion.Slerp(Turret.transform.rotation, targetRotation, Time.deltaTime * turretRotSpeed);
        }

        //////// END WARNING 

        if (Input.GetKey(KeyCode.W))
        {
            targetSpeed = maxForwardSpeed;
        }
        else if (Input.GetKey(KeyCode.S))
        {
            targetSpeed = maxBackwardSpeed;
        }
        else
        {
            targetSpeed = 0;
        }

        curSpeed = Mathf.Lerp(curSpeed, targetSpeed, 7.0f * Time.deltaTime);
        transform.Translate(new Vector3(1, 0, 0) * Time.deltaTime * curSpeed);
        }
}
