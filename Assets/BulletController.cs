using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BulletController : MonoBehaviour
{
    public float lifeTime;
    public bool isWall = false;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(EndOfBulletDelay());
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    IEnumerator EndOfBulletDelay()
    {
        yield return new WaitForSeconds(lifeTime);
        Destroy(gameObject);
    }

    private void OnTriggerEnter(Collider collision)
    {
        if (collision.tag == "Wall") { 
            gameObject.GetComponent<Rigidbody>().velocity = -gameObject.GetComponent<Rigidbody>().velocity; 
        }
    }
}
