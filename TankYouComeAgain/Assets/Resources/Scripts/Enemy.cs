using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Enemy : NetworkBehaviour
{

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
    public bool invulnerable = false;
    [SyncVar]
    public bool canMove = true;
    [SyncVar]
    bool alive = true;

    /* SPEED VARIABLES */
    public float projectileSpeed = 10f;
    public float stunTime = 5f;
    public float timer = 0;

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

    AudioSource clip;
        
    // Added for movement smoothing
    void OnSerializeNetworkView(BitStream stream, NetworkMessageInfo info)
    {
        Vector3 syncPos = rb.position;
        Vector3 velocity = rb.velocity;
        float rotation = rb.rotation;
        float angVel = rb.angularVelocity;

        stream.Serialize(ref syncPos);
        stream.Serialize(ref velocity);
        stream.Serialize(ref rotation);
        stream.Serialize(ref angVel);

        if (stream.isReading)
        {
            rb.position = syncPos;
            rb.velocity = velocity;
            rb.rotation = rotation;
            rb.angularVelocity = angVel;
        }
    }

    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        model = transform.Find("Model").gameObject;

        /* INFO CANVAS */
        healthBarMiniRect = healthBarMini.GetComponent<RectTransform>();
        healthBarMini.GetComponent<Image>().color = playerColor;
        barrel.GetComponent<SpriteRenderer>().color = playerColor;
        enemyNameText.text = enemyName;
        infoRot = infoCanvas.transform.rotation;
        infoPos = infoCanvas.transform.localPosition;
        clip = GetComponent<AudioSource>();

        /* Gather Players */
        players = GameObject.FindObjectsOfType<Player>();
           
    }

    void Update()
    {
        if (Game.instance.GameOver())
        {
            rb.velocity = Vector3.zero;
            return;
        }

        GetMovement();
        UpdateSprites();
    }

    private void LateUpdate()
    {
        UpdateSprites();
    }

    [Command]
    void CmdFire()
    {
        GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, spawnPoint.transform.position, Quaternion.identity);
        Rigidbody2D prb = instantiatedProjectile.GetComponent<Rigidbody2D>();
        prb.velocity = spawnPoint.transform.up * projectileSpeed;
        prb.velocity += rb.velocity;

        // left the owner as null for now
        instantiatedProjectile.GetComponent<Projectile>().owner = null;
        NetworkServer.Spawn(instantiatedProjectile);
    }

    [Command]
    void CmdDeathFlag()
    {
        GameObject instantiatedDeathFlag = (GameObject)Instantiate(deathFlag, transform.position, Quaternion.identity);
        NetworkServer.Spawn(instantiatedDeathFlag);
    }

    private void FollowTarget(Transform target, float stopDistance, float moveSpeed)
    {
        Vector3 vectorToTarget = target.position - transform.position;
        float angle = (Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg) - 90;
        Quaternion q = Quaternion.AngleAxis(angle, Vector3.forward);
        transform.rotation = Quaternion.Slerp(transform.rotation, q, Time.deltaTime * 200.0f);

        if (Vector2.Distance(transform.position, target.position) > stopDistance)
        {
            Vector2 dir = (target.position - transform.position).normalized;
            rb.velocity = dir * 1.0f;
        }
        else
        {
            rb.velocity = Vector2.zero;
            timer += Time.deltaTime;
            if (timer >= fireRate)
            {
                CmdFire();
                timer = 0;
            }

            body.GetComponent<Animator>().SetFloat("Velocity", rb.velocity.magnitude);

        }
            
    }

    private void GetMovement()
    {
        if (canMove)
        {
            float min_dist = Mathf.Infinity;
            Transform target = null;

            foreach (Player player in players)
            {
                float dist = Vector2.Distance(transform.position, player.transform.position);
                if (dist < min_dist)
                {
                    min_dist = dist;
                    target = player.transform;
                }
            }

            if (target != null)
                FollowTarget(target, 5.0f, 200.0f);
        }
        else
        {
            rb.velocity = Vector2.zero;
        }

    }

    public void Stun()
    {
        StartCoroutine(Stunned());
    }

    void UpdateSprites()
    {
        model.SetActive(alive);
        infoCanvas.transform.position = infoPos + transform.position;
        infoCanvas.transform.rotation = infoRot;
        healthBarMiniRect.anchorMax = new Vector2(healthBarMiniRect.anchorMin.x + 0.5f * (health) / MAX_HEALTH, healthBarMiniRect.anchorMax.y);
    }


    void OnCollisionEnter2D(Collision2D collision)
    {
        if (!invulnerable && collision.gameObject.CompareTag("Projectile") && collision.gameObject.GetComponent<Projectile>().owner != null)
        {
            health -= 10.0f;

            if (health < 0)
                StartCoroutine(Death());
            else
                StartCoroutine(Flash());        
        }
    }
    IEnumerator Flash()
    {
        body.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.5f);
        body.GetComponent<SpriteRenderer>().color = playerColor;
    }

    IEnumerator Stunned()
    {
        canMove = false;
        rb.velocity = Vector3.zero;
        CancelInvoke();
        // play some particle effect here
        yield return new WaitForSeconds(stunTime);
        canMove = true;
    }

    IEnumerator Death()
    {
        alive = false;
        canMove = false;
        CmdDeathFlag();
        infoCanvas.SetActive(false);
        yield return new WaitForSeconds(5.0f);
        NetworkServer.Destroy(gameObject);
    }
}

