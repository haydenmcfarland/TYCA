﻿using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour {
    public const int NUM_ABILITIES = 4;
    public const float GLOBAL_COOLDOWN = 0.5f;
    public const float MAX_HEALTH = 100f;
    /* public for initialization or access*/
    public int id;
    public float fireRate = 1.0f;
    public int deaths = 0;
    public float shieldTime = 3f;
    public float ultiMoveMultiplier = 3f;
    public float ultimateDuration = 15f;
    public float ultimateFireRate = 0.1f;
    public float[] abilityCooldowns = new float[NUM_ABILITIES];
    [SyncVar]
    public float health = MAX_HEALTH;
    public KeyCode left = KeyCode.A;
    public KeyCode right = KeyCode.D;
    public KeyCode up = KeyCode.W;
    public KeyCode down = KeyCode.S;
    public KeyCode[] ability;
    public float rotationSpeed = 25f;
    public float velocity = 0f;
    public float moveSpeed = 5f;
    public float projectileSpeed = 1f;
    public bool canMove = true;
    public float stunTime = 5f;
    public bool invulnerable = false;

    /* prefabs */
    public GameObject spawnPoint;
    public GameObject body;
    public GameObject projectile;
    public GameObject grenade;
    public GameObject shield;

    /* private */
    float[] currAbilityCooldowns = new float[NUM_ABILITIES];
    float[] abilityTimers = new float[NUM_ABILITIES];
    Image[] abilityCD = new Image[NUM_ABILITIES];
    Text[] abilityCDText = new Text[NUM_ABILITIES];
    Rigidbody2D rb;
    GameObject healthBar;
    RectTransform healthBarRect;

    // Use this for initialization
    void Start() {
        for (int i = 0; i < NUM_ABILITIES; ++i) {
            abilityTimers[i] = 0;
            currAbilityCooldowns[i] = abilityCooldowns[i];
            abilityCD[i] = GameObject.Find("Canvas/HUD/Ability " + (i + 1) + "/Cooldown").GetComponent<Image>();
            abilityCDText[i] = GameObject.Find("Canvas/HUD/Ability " + (i + 1) + "/Cooldown Text").GetComponent<Text>();
        }
        id = Game.instance.RegisterPlayer(this);
        shield.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
        healthBar = GameObject.Find("Canvas/HUD/Health Bar");
        healthBarRect = healthBar.GetComponent<RectTransform>();
    }

    // Update is called once per frame
    void Update() {
        if (health <= 0) {
            // death code goes here
        }
        if (canMove) {
            GetMovement();
        }
        HandleAbilities();
    }
    void UpdateHealthBarUI() {
        healthBarRect.anchorMax = new Vector2(healthBarRect.anchorMin.x + 0.35f * (health) / MAX_HEALTH, healthBarRect.anchorMax.y);
    }
    [Command]
    void CmdFire() {
        GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, spawnPoint.transform.position, Quaternion.identity);
        instantiatedProjectile.GetComponent<Rigidbody2D>().velocity = spawnPoint.transform.up * projectileSpeed;
        instantiatedProjectile.GetComponent<Projectile>().assignedID = id;
        NetworkServer.Spawn(instantiatedProjectile);
    }

    [Command]
    void CmdFireGrenade() {
        GameObject instantiatedGrenade = (GameObject)Instantiate(grenade, spawnPoint.transform.position, Quaternion.identity);
        instantiatedGrenade.GetComponent<Rigidbody2D>().velocity = spawnPoint.transform.up * projectileSpeed;
        NetworkServer.Spawn(instantiatedGrenade);
    }

    [Command]
    void CmdActivateShield() {
        StartCoroutine(Shield());
    }
    [Command]
    void CmdActivateUltimate() {
        StartCoroutine(Ultimate());
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

    private void HandleAbilities() {
        for (int i = 0; i < NUM_ABILITIES; ++i) {
            if (Input.GetKeyDown(ability[i]) && abilityTimers[i] <= 0 && canMove) {
                for (int j = 0; j < NUM_ABILITIES; ++j) {
                    if (i != j && abilityTimers[j] <= 0) {
                        abilityTimers[j] = GLOBAL_COOLDOWN;
                        currAbilityCooldowns[j] = GLOBAL_COOLDOWN;
                    }
                }
                abilityTimers[i] = abilityCooldowns[i];
                // ability code goes here
                ActivateAbility(i);
            }
            if (abilityTimers[i] >= 0) {
                abilityTimers[i] -= Time.deltaTime;
                abilityCD[i].fillAmount = abilityTimers[i] / currAbilityCooldowns[i];
                abilityCDText[i].text = "" + (int)abilityTimers[i];
                if (abilityCDText[i].text == "0") {
                    abilityCDText[i].text = "";
                }
            } else {
                currAbilityCooldowns[i] = abilityCooldowns[i];
                abilityCDText[i].text = "";
            }
        }
    }

    private void ActivateAbility(int index) {
        switch (index) {
            case 0:
                CmdFire();
                break;
            case 1:
                CmdActivateShield();
                break;
            case 2:
                CmdFireGrenade();
                break;
            case 3:
                CmdActivateUltimate();
                break;
            default:
                break;
        }
    }

    public void Damage(float damage) {
        health -= damage;
        Debug.Log(damage);
        UpdateHealthBarUI();
    }

    private void OnCollisionEnter2D(Collision2D collision) {

        if (!invulnerable && collision.gameObject.CompareTag("Projectile") && collision.gameObject.GetComponent<Projectile>().assignedID != id) {
            StartCoroutine(Flash());
        }
    }

    public void Stun() {
        StartCoroutine(Stunned());
    }

    IEnumerator Flash() {
        invulnerable = true;
        body.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.5f);
        body.GetComponent<SpriteRenderer>().color = Color.white;
        health -= 1;
        invulnerable = false;
    }

    IEnumerator Shield() {
        shield.SetActive(true);
        invulnerable = true;
        yield return new WaitForSeconds(shieldTime);
        invulnerable = false;
        shield.SetActive(false);
    }

    IEnumerator Ultimate() {
        moveSpeed *= ultiMoveMultiplier;
        rotationSpeed *= ultiMoveMultiplier;
        InvokeRepeating("CmdFire", 0, ultimateFireRate);
        yield return new WaitForSeconds(ultimateDuration);
        CancelInvoke();
        moveSpeed /= ultiMoveMultiplier;
        rotationSpeed /= ultiMoveMultiplier;
    }

    IEnumerator Stunned() {
        canMove = false;
        rb.velocity = Vector3.zero;
        CancelInvoke();
        yield return new WaitForSeconds(stunTime);
        canMove = true;
    }
}

