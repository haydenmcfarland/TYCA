using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour {
    public const int NUM_ABILITIES = 4;
    public const float GLOBAL_COOLDOWN = 0.5f;
    /* public for initialization or access*/
    public int id;
    public float fireRate = 1.0f;
    public int deaths = 0;
    public float[] abilityCooldowns = new float[NUM_ABILITIES];
    public float[] abilityTimers = new float[NUM_ABILITIES];
    
    [SyncVar]
    public int health = 3;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode fire = KeyCode.Space;
    public KeyCode[] ability;
    public float rotationSpeed = 25f;
    public float velocity = 0f;
    public float moveSpeed = 5f;
    public float projectileSpeed = 1f;

    /* prefabs */
    public GameObject barrel;
    public GameObject projectile;

    /* private */
    bool hasFired = false;
    bool isHit = false;
    float[] currAbilityCooldowns = new float[NUM_ABILITIES];
    Image[] abilityCD = new Image[NUM_ABILITIES];
    Text[] abilityCDText = new Text[NUM_ABILITIES];

    // Use this for initialization
    void Start() {
        for(int i = 0; i < NUM_ABILITIES; ++i) {
            abilityTimers[i] = 0;
            currAbilityCooldowns[i] = abilityCooldowns[i];
            abilityCD[i] = GameObject.Find("Canvas/HUD/Ability " + (i + 1) + "/Cooldown").GetComponent<Image>();
            abilityCDText[i] = GameObject.Find("Canvas/HUD/Ability " + (i + 1) + "/Cooldown Text").GetComponent<Text>();
        }
        id = Game.instance.RegisterPlayer(this);
    }

    // Update is called once per frame
    void Update() {
        if (health <= 0) {
            // death code goes here
        }
        GetMovement();
        HandleFire();
        HandleAbilities();
    }

    [Command]
    void CmdFire() {
        GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, barrel.transform.position, Quaternion.identity);
        instantiatedProjectile.GetComponent<Rigidbody2D>().velocity = barrel.transform.up * projectileSpeed;
        instantiatedProjectile.GetComponent<Projectile>().assignedID = id;
        NetworkServer.Spawn(instantiatedProjectile);
    }
    private void GetMovement() {
        if (Input.GetKey(left)) {
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
            velocity = 0;
        }
        transform.Translate(0.0f, velocity * Time.deltaTime, 0.0f);
    }

    private void HandleFire() {
        if (Input.GetKeyDown(fire) && !hasFired) {
            hasFired = !hasFired;
            CmdFire();
            StartCoroutine(BulletWaitTime());
        }
    }

    private void HandleAbilities() {
        for(int i = 0; i < NUM_ABILITIES; ++i) {
            if (Input.GetKeyDown(ability[i]) && abilityTimers[i] <= 0) {
                for (int j = 0; j < NUM_ABILITIES; ++j) {
                    if(i != j && abilityTimers[j] <= 0) {
                        abilityTimers[j] = GLOBAL_COOLDOWN;
                        currAbilityCooldowns[j] = GLOBAL_COOLDOWN;
                    }
                }
                abilityTimers[i] = abilityCooldowns[i];
                // ability code goes here
            }
            if (abilityTimers[i] >= 0) {
                abilityTimers[i] -= Time.deltaTime;
                abilityCD[i].fillAmount = abilityTimers[i] / currAbilityCooldowns[i];
                abilityCDText[i].text = "" + (int)abilityTimers[i];
                if(abilityCDText[i].text == "0") {
                    abilityCDText[i].text = "";
                }
            } else {
                currAbilityCooldowns[i] = abilityCooldowns[i];
                abilityCDText[i].text = "";
            }
        }
    }
    private void OnCollisionEnter2D(Collision2D collision) {

        if (!isHit && collision.transform.name.Contains("Projectile")) {
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

