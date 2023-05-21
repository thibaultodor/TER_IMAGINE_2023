using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LifeController : MonoBehaviour{
    public GameObject[] lifeIcon;
    public int life;
    private int old_life;

    // Update is called once per frame
    void Update()
    {
        for( int l = life; l < old_life; l++ )
            Destroy(lifeIcon[l].gameObject);

        if (life <= 0 )
        {
            ScoreController.score=0;
            Scene scene = SceneManager.GetActiveScene(); SceneManager.LoadScene(scene.name);
        }

        old_life = life;
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
