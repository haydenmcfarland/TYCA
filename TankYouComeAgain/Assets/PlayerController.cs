using System.Collections;
using UnityEngine;

public class PlayerController : MonoBehaviour {

    public string playerNum;
    public string bulletName;
    public float fireRate = 1.0f;
    int health = 3;
    public GameObject Barrel;
    public GameObject Bullet;
    float moveStep = 0;
    float rotStep = 0;
    float rotBarrelStep = 0;
    bool hasFired = false;
    bool isHit = false;

    // Use this for initialization
    void Start () {

        /* Need Update for GamePad Settings */
        if (playerNum == "2")
            bulletName = "Bullet1";
        else
            bulletName = "Bullet2";

	}

    // Update is called once per frame
    void Update()
    {
        if (health <= 0)
            Destroy(gameObject);

        if (Input.GetAxis("Fire" + playerNum) != 0 && !hasFired)
        {
            hasFired = !hasFired;
            Vector3 start = Barrel.transform.position + Barrel.transform.up;
            GameObject bullet = Instantiate(Bullet, start, Barrel.transform.rotation);
            bullet.GetComponent<Rigidbody2D>().velocity = Barrel.transform.up * Time.deltaTime * 300;
            StartCoroutine(BulletWaitTime());
        }

        if (Input.GetAxis("RotBarrel" + playerNum) != 0)
            rotBarrelStep = Time.deltaTime * 200;
        else
            rotBarrelStep = 0;


        if (Input.GetAxis("Vertical" + playerNum) != 0)
            moveStep = Time.deltaTime * Input.GetAxis("Vertical" + playerNum) * 200;

        if (Input.GetAxis("Horizontal" + playerNum) != 0)
            rotStep = Time.deltaTime * Input.GetAxis("Horizontal" + playerNum) * 200;
        
        transform.GetComponent<Rigidbody2D>().velocity = transform.up * moveStep;
        transform.GetComponent<Rigidbody2D>().MoveRotation(transform.GetComponent<Rigidbody2D>().rotation - rotStep);
        Barrel.transform.Rotate(0,0,rotBarrelStep);
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.name.Contains(bulletName) && !isHit)
        {
            Destroy(collision.gameObject);
            StartCoroutine(Flash());
        }
    }

    IEnumerator BulletWaitTime()
    {
        yield return new WaitForSeconds(fireRate);
        hasFired = false;
    }

    IEnumerator Flash()
    {
        isHit = true;
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.4f);
        GetComponent<SpriteRenderer>().color = Color.white;
        health -= 1;
        isHit = false;
    }
}

