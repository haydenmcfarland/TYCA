using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour {

    /* CONST VARIABLES */
    public const int NUM_ABILITIES = 4;
    public const float GLOBAL_COOLDOWN = 0.5f;
    public const float MAX_HEALTH = 100f;

    /* NETWORK SYNC VARIABLES */
    [SyncVar]
    public float fireRate = 3.0f;
    [SyncVar]
    public float health = MAX_HEALTH;
    [SyncVar]
    public bool canMove = true;
    [SyncVar]
    bool alive = true;

    /* SPEED VARIABLES */
    public float projectileSpeed = 10f;
    public float stunTime = 5f;
    public float timer = 0;
    public float rotMult = 200f;
    public float deathTime = 5f;

    /* DROP IN GAMEOBJECTS */
    public GameObject spawnPoint;
    public GameObject body;
    public GameObject barrel;
    public GameObject projectile;
    public GameObject deathFlag;

    /* PLAYER INFO VARIABLES */
    [SyncVar]
    public Color playerColor = Color.white;
    [SyncVar]
    public string enemyName = "";

    /* ENEMY INFO DISPLAY */
    public GameObject infoCanvas;
    public Text enemyNameText;
    public GameObject healthBarMini;
    RectTransform healthBarMiniRect;
    // used to force world canvas position under parent
    Quaternion infoRot;
    Vector3 infoPos;

    /* ENEMY OBJECT */
    Rigidbody2D rb;
    GameObject model;
    Player[] players;

    // Added for movement smoothing
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info) {
        Vector3 syncPos = rb.position;
        Vector3 velocity = rb.velocity;
        float rotation = rb.rotation;
        float angVel = rb.angularVelocity;

        stream.Serialize(ref syncPos);
        stream.Serialize(ref velocity);
        stream.Serialize(ref rotation);
        stream.Serialize(ref angVel);

        if (stream.isReading) {
            rb.position = syncPos;
            rb.velocity = velocity;
            rb.rotation = rotation;
            rb.angularVelocity = angVel;
        }
    }

    void Start() {
        body.GetComponent<NetworkAnimator>().SetParameterAutoSend(0, true);
        rb = GetComponent<Rigidbody2D>();
        model = transform.Find("Model").gameObject;

        /* INFO CANVAS */
        healthBarMiniRect = healthBarMini.GetComponent<RectTransform>();
        healthBarMini.GetComponent<Image>().color = playerColor;
        barrel.GetComponent<SpriteRenderer>().color = playerColor;
        enemyNameText.text = enemyName;
        infoRot = infoCanvas.transform.rotation;
        infoPos = infoCanvas.transform.localPosition;

        /* GATHER PLAYERS FOR MOVEMENT */
        players = GameObject.FindObjectsOfType<Player>();

    }

    void Update() {

        if (Game.instance.GameOver()) {
            rb.velocity = Vector3.zero;
            return;
        }
        if (isServer) {
            GetMovement();
        }
    }

    private void LateUpdate() {
        // need late update to correct issues with physics
        UpdateSprites();
    }

    [Command]
    void CmdFire() {
        GameObject instantiatedProjectile = Instantiate(projectile, spawnPoint.transform.position, Quaternion.identity);
        Rigidbody2D prb = instantiatedProjectile.GetComponent<Rigidbody2D>();
        prb.velocity = transform.up * projectileSpeed;
        prb.velocity += rb.velocity;

        float angle = Mathf.Atan2(transform.up.y, transform.up.x) * Mathf.Rad2Deg;
        prb.transform.rotation = Quaternion.AngleAxis(angle, Vector3.forward);

        // enemy is considered a null owner as it is not a player (no broadcast messages and no scoring)
        instantiatedProjectile.GetComponent<Projectile>().owner = null;
        NetworkServer.Spawn(instantiatedProjectile);
    }

    [Command]
    void CmdDeathFlag() {
        GameObject instantiatedDeathFlag = Instantiate(deathFlag, transform.position, Quaternion.identity);
        NetworkServer.Spawn(instantiatedDeathFlag);
    }

    private void FollowTarget(Transform target, float stopDistance, float moveSpeed) {
        Vector3 rotVecDir = target.position - transform.position;
        float angle = (Mathf.Atan2(rotVecDir.y, rotVecDir.x) * Mathf.Rad2Deg) - 90;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * rotMult);

        if (Vector2.Distance(transform.position, target.position) > stopDistance) {
            Vector2 dir = (target.position - transform.position).normalized;
            rb.velocity = dir;
        } else {
            rb.velocity = Vector2.zero;
            timer += Time.deltaTime;
            if (timer >= fireRate) {
                CmdFire();
                timer = 0;
            }
        }

    }

    private void GetMovement() {
        if (canMove) {
            float min_dist = Mathf.Infinity;
            Transform target = null;

            foreach (Player player in players) {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < min_dist) {
                    min_dist = dist;
                    target = player.transform;
                }
            }

            if (target)
                FollowTarget(target, 5.0f, 200.0f);
        } else
            rb.velocity = Vector2.zero;

    }

    public void Stun() {
        StartCoroutine(Stunned());
    }

    void UpdateSprites() {
        model.SetActive(alive);
        infoCanvas.transform.position = infoPos + transform.position;
        infoCanvas.transform.rotation = infoRot;
        healthBarMiniRect.anchorMax = new Vector2(healthBarMiniRect.anchorMin.x + 0.5f * (health) / MAX_HEALTH, healthBarMiniRect.anchorMax.y);
        body.GetComponent<Animator>().SetFloat("Velocity", rb.velocity.magnitude);
    }
    public void Damage(float damage, Player killer) {
        if (!isServer) {
            return;
        }
        health -= damage;
        if (health <= 0 && alive) {
            StartCoroutine(Death(killer));
        }
    }
    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Projectile")) {

            Projectile p = collision.gameObject.GetComponent<Projectile>();
            Damage(p.damage, p.owner);
            StartCoroutine(Flash());
        }
        if (collision.gameObject.CompareTag("Grenade")) {

            Grenade g = collision.gameObject.GetComponent<Grenade>();
            Damage(g.damage, g.owner);
            Stun();
            StartCoroutine(Flash());
        }
    }
    [ClientRpc]
    void RpcRespawn() {
        NetworkStartPosition[] spawnPoints = FindObjectsOfType<NetworkStartPosition>();
        transform.position = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
    }

    IEnumerator Flash() {
        body.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.1f);
        body.GetComponent<SpriteRenderer>().color = playerColor;
    }

    IEnumerator Stunned() {
        canMove = false;
        rb.velocity = Vector3.zero;
        CancelInvoke();
        // play some particle effect here
        yield return new WaitForSeconds(stunTime);
        canMove = true;
    }

    IEnumerator Death(Player killer) {
        alive = false;
        canMove = false;
        CmdDeathFlag();
        if (killer) {
            killer.kills++;
        }
        infoCanvas.SetActive(false);
        yield return new WaitForSeconds(deathTime);
        health = MAX_HEALTH;
        alive = true;
        canMove = true;
        RpcRespawn();
        infoCanvas.SetActive(true);
    }
}

