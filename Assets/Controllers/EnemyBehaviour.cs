using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyBehaviour : FMS
{
    // Different states of an ennemy tank
    public enum FSMState
    {
        None,
        Patrol,
        Chase,
        Attack,
        Dead,
    }

    //Current state that the NPC is reaching
    public FSMState curState;

    //Speed of the tank
    private float curSpeed;

    //Tank Rotation Speed
    private float curRotSpeed;

    //Bullet
    public GameObject bulletPrefab;
    private float bulletSpeed;

    //Whether the NPC is destroyed or not
    private bool bDead;
    private int health;
    private int max_health;

    public float FindNextWandarPointDistance;
    public float BeginToChasePlayerDistance;
    public float BeginToAttackPlayerDistance;
    public float LostSightOfPlayerChaseDistance;
    public float MinimalDistanceBetweenTanks;
    public float LostSightOfPlayerAttackDistance;

    private float const_y_pos;

    //Initialize the Finite state machine for the NPC tank with default values 
    protected override void Initialize()
    {
        curState = FSMState.Patrol;
        curSpeed = 1.0f;
        curRotSpeed = 1.0f;

        bDead = false;
        max_health = health = 1;


        const_y_pos = transform.position[1];


        elapsedTime = 0.0f;
        bulletSpeed = 2.0f;
        shootRate = 3.0f;
        health = 1;

        //Get the list of points
        // WandarPoints = Id of the list of points to generate
        pointList = GameObject.FindGameObjectsWithTag("WandarPoint");

        //Set Random destination point first
        FindNextPoint();

        //Get the target enemy(Player)
        GameObject objPlayer = GameObject.FindGameObjectWithTag("Player");
        playerTransform = objPlayer.transform;
        if (!playerTransform)
            print("Player doesn't exist.. Please add one with Tag named 'Player'");

        //Get the turret of the tank
        // turret = gameObject.transform.GetChild(0).transform;
        // bulletSpawnPoint = turret.GetChild(0).transform;
    }

    // Update each frame

    private void EnforceFrozenAxis( GameObject g )
    {
        g.transform.position = new Vector3(transform.position[0], const_y_pos, transform.position[2]);
        g.transform.eulerAngles = new Vector3(0.0f, transform.eulerAngles.y, 0.0f);
    }
    protected override void FSMUpdate()
    {
        switch (curState)
        {
            case FSMState.Patrol: UpdatePatrolState(); break;
            case FSMState.Chase: UpdateChaseState(); break;
            case FSMState.Attack: UpdateAttackState(); break;
            case FSMState.Dead: UpdateDeadState(); break;
        }

        //Update the time
        elapsedTime += Time.deltaTime;

        //Go to dead state is no health left
        if (health < 1 && health > -1 )
        {
            ScoreController.score++;
            curState = FSMState.Dead;
            Transform Tank = this.gameObject.transform.GetChild(0);
            float health_percant = (float)health / (float)max_health;
            Color new_color = new Color(1.0f, health_percant, health_percant, 0.0f );
            
            for( int i = 0; i < Tank.transform.childCount; i++ )
                Tank.transform.GetChild(i).GetComponent<MeshRenderer>().material.SetColor("_Color", Color.red);

            health = -1;
        }

        if( curState != FSMState.Dead)
            EnforceFrozenAxis(this.gameObject);
    }

    protected void UpdatePatrolState()
    {
        //Find another random patrol point if the current point is reached
        if (Vector3.Distance(transform.position, destPos) <= FindNextWandarPointDistance )
        {
            print("Reached to the destination point calculating the next point");
            FindNextPoint();
        }
        //Check the distance with player tank
        //When the distance is near, transition to chase state
        else if (Vector3.Distance(transform.position, playerTransform.position) <= BeginToChasePlayerDistance )
        {
            print("Switch to Chase Position");
            curState = FSMState.Chase;
        }

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    // Chooses at random a new destination point amongst the waypoints
    protected void FindNextPoint()
    {
        print("Finding next point");

        int rndIndex = Random.Range(0, pointList.Length);
        float rndRadius = 1.0f;
        Vector3 rndPosition = Vector3.zero;

        destPos = pointList[rndIndex].transform.position + rndPosition;

        //Check Range to decide the random point
        //as the same as before
        if (IsInCurrentRange(destPos))
        {
            rndPosition = new Vector3(Random.Range(-rndRadius, rndRadius), Random.Range(-rndRadius, rndRadius), 0.0f);
            destPos = pointList[rndIndex].transform.position + rndPosition;
        }
    }

    protected bool IsInCurrentRange(Vector3 pos)
    {
        float xPos = Mathf.Abs(pos.x - transform.position.x);
        float zPos = Mathf.Abs(pos.z - transform.position.z);

        if (xPos <= 1.5f && zPos <= 1.5f)
            return true;

        return false;
    }

    /////// WARNING : here there are no obstacles taken into account 
    protected void UpdateChaseState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with player tank When
        //the distance is near, transition to attack state
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist <= BeginToAttackPlayerDistance ) 
            curState = FSMState.Attack;
        //Go back to patrol is it become too far
        else if (dist >= LostSightOfPlayerChaseDistance ) 
            curState = FSMState.Patrol;

        //Go Forward
        transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
    }

    protected void UpdateAttackState()
    {
        //Set the target position as the player position
        destPos = playerTransform.position;

        //Check the distance with the player tank
        float dist = Vector3.Distance(transform.position, playerTransform.position);

        if (dist >= MinimalDistanceBetweenTanks && dist < BeginToAttackPlayerDistance )
        {
            //Go Forward
            transform.Translate(Vector3.forward * Time.deltaTime * curSpeed);
            curState = FSMState.Attack;
        }
        //Transition to patrol is the tank become too far
        else if (dist >= LostSightOfPlayerAttackDistance )
            curState = FSMState.Patrol;

        //Rotate to the target point
        Quaternion targetRotation = Quaternion.LookRotation(destPos - transform.position);
        transform.rotation = Quaternion.Slerp(transform.rotation, targetRotation, Time.deltaTime * curRotSpeed);

        //Always Turn the turret towards the player
        //Quaternion turretRotation = Quaternion.LookRotation(destPos - turret.position, new Vector3(0, 0, 1));
        //turret.rotation = Quaternion.Slerp(turret.rotation, turretRotation, Time.deltaTime * curRotSpeed);

        //Shoot the bullets
        ShootBullet();
    }

    private void ShootBullet()
    {
        if (elapsedTime >= shootRate)
        {
            GameObject bullet = Instantiate(bulletPrefab, transform.position, transform.rotation) as GameObject;
            BulletController bulletController = bullet.GetComponent<BulletController>();
            bullet.gameObject.tag = "EnemyBullet";
            bullet.layer = LayerMask.NameToLayer("Enemy");
            Rigidbody bulletRb = bulletController.rb;
            bulletController.transform.Rotate(Vector3.left, 90);
            bulletRb.useGravity = false;
            bulletRb.freezeRotation = true;

            bulletController.velocity = transform.forward * bulletSpeed;// transform.position + transform.forward * bulletSpeed;

            print(transform.forward);

            elapsedTime = 0;
        }
    }

    protected void UpdateDeadState()
    {
        
        //Show the dead animation with some physics effects
        if (!bDead)
        {
            bDead = true;
            Explode();
        }
        Destroy(gameObject, 1.5f);
    }

    // Applies an ExplosionForce to the the rigidbody component with some random direction
    protected void Explode()
    {
        this.gameObject.GetComponent<Rigidbody>().isKinematic = false;

        SpriteRenderer[] sprites = gameObject.GetComponentsInChildren<SpriteRenderer>();
        sprites[0].enabled = false;

        float rndX = Random.Range(2.0f, 6.0f);
        float rndY = Random.Range(2.0f, 6.0f);
        float rndZ = Random.Range(2.0f, 6.0f);

        for (int i = 0; i < 3; i++)
        {
            GetComponent<Rigidbody>().AddExplosionForce(100.0f, transform.position - new Vector3(rndX, rndY, rndZ), 8.0f, 2.0f);
            GetComponent<Rigidbody>().velocity = transform.TransformDirection(new Vector3(rndX, rndY, rndZ));
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        //Reduce health
        if (collision.gameObject.CompareTag("AllyBullet"))
        {
            health -= collision.gameObject.GetComponent<BulletController>().damage;
            Destroy(collision.gameObject, 0.0f);
        }
    }
}