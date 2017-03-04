using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour {

    /* public for initialization or access*/
    public int id;
    public float fireRate = 1.0f;
    public int deaths = 0;
    [SyncVar]
    public int health = 3;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode fire = KeyCode.Space;
    public KeyCode rotateBarrel = KeyCode.X;
    public float rotationSpeed = 25f;
    public float velocity = 0f;
    public float moveSpeed = 5f;
    public float projectileSpeed = 1f;

    /* prefabs */
    public GameObject barrel;
    public GameObject projectile;
    public GameObject ScoreBoard;

    /* private */
    float rotBarrelStep = 0;
    bool hasFired = false;
    bool isHit = false;

    // Use this for initialization
    void Start() {

    }

    // Update is called once per frame
    void Update() {
        if (health <= 0) {
            // death code goes here
        }
        GetMovement();
        HandleFire();

    }

    [Command]
    void CmdFire() {
        GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, barrel.transform.position, Quaternion.identity);
        instantiatedProjectile.GetComponent<Rigidbody2D>().velocity = barrel.transform.up * projectileSpeed;
        instantiatedProjectile.GetComponent<Projectile>().assignedID = id;
        NetworkServer.Spawn(instantiatedProjectile);
    }
    private void GetMovement() {
        if (Input.GetKey(left)) { // click left
            float turnVelocity = Mathf.Max(rotationSpeed, rotationSpeed * velocity * 0.1f);
            transform.Rotate(new Vector3(0.0f, 0.0f, turnVelocity * Time.deltaTime));
        }

        if (Input.GetKey(right)) {
            float turnVelocity = Mathf.Max(rotationSpeed, rotationSpeed * velocity * 0.1f);
            transform.Rotate(new Vector3(0.0f, 0.0f, -turnVelocity * Time.deltaTime));
        }
        if (Input.GetKey(up)) {
            velocity = Mathf.Min(moveSpeed, velocity + moveSpeed * Time.deltaTime);
        } else if (Input.GetKey(down)) {
            velocity = Mathf.Max(-moveSpeed * (1 + (rotationSpeed)) / 2f, velocity - moveSpeed * .85f * Time.deltaTime);
        } else {
            if (velocity > 0) {
                velocity = Mathf.Max(0f, velocity - moveSpeed / 2f * Time.deltaTime);
            } else if (velocity < 0) {
                velocity = Mathf.Min(0f, velocity + moveSpeed / 2f * Time.deltaTime);
            }
        }
        transform.Translate(0.0f, velocity * Time.deltaTime, 0.0f);
    }

    private void HandleFire() {
        if (Input.GetKeyDown(fire) && !hasFired) {
            hasFired = !hasFired;
            CmdFire();
            StartCoroutine(BulletWaitTime());
        }

        if (Input.GetKeyDown(rotateBarrel))
            rotBarrelStep = Time.deltaTime * 200;
        else
            rotBarrelStep = 0;
        barrel.transform.Rotate(0, 0, rotBarrelStep);
    }
    private void OnCollisionEnter2D(Collision2D collision) {
        if (!isHit) {
            Destroy(collision.gameObject);
            StartCoroutine(Flash());
        }
    }

    IEnumerator BulletWaitTime() {
        yield return new WaitForSeconds(fireRate);
        hasFired = false;
    }

    IEnumerator Flash() {
        isHit = true;
        GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.5f);
        GetComponent<SpriteRenderer>().color = Color.white;
        health -= 1;
        isHit = false;
    }
}

