using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LifeController : MonoBehaviour{
    public GameObject[] lifeIcon;
    public int life;

    // Update is called once per frame
    void Update()
    {
        if(life < 1)
        {
            Destroy(lifeIcon[0].gameObject);
            ScoreController.score=0;
            Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
        }
        else if(life < 2)
        {
            Destroy(lifeIcon[1].gameObject);
        }else if (life < 3)
        {
            Destroy(lifeIcon[2].gameObject);
        }
        else if (life < 4)
        {
            Destroy(lifeIcon[3].gameObject);
        }
        else if (life < 5)
        {
            Destroy(lifeIcon[4].gameObject);
        }
    }

    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Enemy"))
        {
            life--;
        }
        //Reduce health
        if (collision.gameObject.CompareTag("EnemyBullet"))
        {
            life -= collision.gameObject.GetComponent<BulletController>().damage;
            Destroy(collision.gameObject, 0.0f);
        }
    }
}
