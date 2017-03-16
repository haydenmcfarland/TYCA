using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

public class Player : NetworkBehaviour {

    /* CONST VARIABLES */
    public const int NUM_ABILITIES = 4;
    public const float GLOBAL_COOLDOWN = 0.5f;
    public const float MAX_HEALTH = 100f;
    public const int MAX_LIVES = 5;
    public const string DEATH_MESSAGE = "You died! Respawn in ";
    public const string VOTE_STR = "Votes to Restart: ";

    /* NETWORK SYNC VARIABLES */
    [SyncVar]
    public int id;
    [SyncVar]
    public int deaths = 0;
    [SyncVar]
    public int kills = 0;
    [SyncVar]
    public float fireRate = 1.0f;
    [SyncVar]
    public float health = MAX_HEALTH;
    [SyncVar]
    public bool invulnerable = false;
    [SyncVar]
    public bool canMove = true;
    [SyncVar]
    bool shielded;
    [SyncVar]
    bool alive = true;
    [SyncVar]
    public string broadcast;
    [SyncVar]
    public bool voteToRestart = false;
    [SyncVar]
    public int votes;

    /* TIMING/ABILITY VARIABLES */
    float timer;
    public float deathTime = 5f;
    public float shieldTime = 3f;
    public float ultiMoveMultiplier = 3f;
    public float ultimateDuration = 15f;
    public float ultimateFireRate = 0.1f;
    public float[] abilityCooldowns = new float[NUM_ABILITIES];
    public KeyCode[] ability;
    public Sprite wesley;

    /* SPEED VARIABLES */
    float rotationSpeed;
    float moveSpeed;
    public float moveMult = 100f;
    public float rotMult = 100f;
    public float projectileSpeed = 10f;
    public float stunTime = 5f;

    /* DROP IN GAMEOBJECTS */
    public GameObject spawnPoint;
    public GameObject body;
    public GameObject barrel;
    public GameObject projectile;
    public GameObject grenade;
    public GameObject deathFlag;
    public GameObject shield;

    /* PLAYER INFO VARIABLES */
    [SyncVar]
    public Color playerColor = Color.white;
    [SyncVar]
    public string playerName = "";
    NetworkStartPosition[] spawnPoints;

    /* PRIVATE ABILITY VARIABLES */
    float[] currAbilityCooldowns = new float[NUM_ABILITIES];
    float[] abilityTimers = new float[NUM_ABILITIES];
    Image[] abilityCD = new Image[NUM_ABILITIES];
    Text[] abilityCDText = new Text[NUM_ABILITIES];

    /* UI ELEMENTS */
    GameObject healthBar;
    RectTransform healthBarRect;
    GameObject deathOverlay;
    Text deathText;
    Text broadcastText;
    Text voteText;

    /* PLAYER INFO DISPLAY */
    public GameObject infoCanvas;
    public Text playerNameText;
    public GameObject healthBarMini;
    RectTransform healthBarMiniRect;
    // used to force world canvas position under parent
    Quaternion infoRot;
    Vector3 infoPos;


    /* PLAYER OBJECT */
    Rigidbody2D rb;
    GameObject model;
    public Sprite firstBodyFrame;
    public Sprite secondBodyFrame;

    AudioSource clip;
    // Use for local player initialization
    public override void OnStartLocalPlayer() {
        for (int i = 0; i < NUM_ABILITIES; ++i) {
            abilityTimers[i] = 0;
            currAbilityCooldowns[i] = abilityCooldowns[i];
            abilityCD[i] = GameObject.Find("Canvas/HUD/Ability " + (i + 1) + "/Cooldown").GetComponent<Image>();
            abilityCDText[i] = GameObject.Find("Canvas/HUD/Ability " + (i + 1) + "/Cooldown Text").GetComponent<Text>();
        }
        healthBar = GameObject.Find("Canvas/HUD/Health Bar");
        voteText = GameObject.Find("Canvas/Vote Text").GetComponent<Text>();
        healthBarRect = healthBar.GetComponent<RectTransform>();
        deathOverlay = GameObject.Find("Canvas/Death Overlay");
        deathText = GameObject.Find("Canvas/Death Overlay/Text").GetComponent<Text>();
        deathOverlay.SetActive(false);
        spawnPoints = FindObjectsOfType<NetworkStartPosition>();

    }

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
        if (isServer) {
            id = Game.instance.AssignId();
        }
        Game.instance.RegisterPlayer(this);
        shield.SetActive(false);
        rb = GetComponent<Rigidbody2D>();
        model = transform.Find("Model").gameObject;
        broadcastText = GameObject.Find("Canvas/Broadcast").GetComponent<Text>();
        /* INFO CANVAS */
        healthBarMiniRect = healthBarMini.GetComponent<RectTransform>();
        healthBarMini.GetComponent<Image>().color = playerColor;
        barrel.GetComponent<SpriteRenderer>().color = playerColor;
        body.GetComponent<SpriteRenderer>().color = playerColor;
        playerNameText.text = playerName;
        infoRot = infoCanvas.transform.rotation;
        infoPos = infoCanvas.transform.localPosition;
        clip = GetComponent<AudioSource>();
    }

    // Update is called once per frame
    void Update() {
        if (Game.instance.GameOver()) {
            rb.velocity = Vector3.zero;
            return;
        }
        if (isServer) {
            votes = Game.instance.GetNumVotes();
        }
        if (Input.GetKey(KeyCode.W)) {
            if (Input.GetKey(KeyCode.E)) {
                if (Input.GetKey(KeyCode.S)) {
                    if (Input.GetKey(KeyCode.Space)) {
                        body.GetComponent<SpriteRenderer>().sprite = wesley;
                    }
                }
            }
        }
        UpdateSprites();
        if (!isLocalPlayer) {
            return;
        }
        if (canMove) {
            GetMovement();
        }
        if (Input.GetKeyDown(KeyCode.R)) {
            CmdVoteToRestart();
        }
        HandleAbilities();
        UpdateUI();
        Camera.main.transform.position = new Vector3(transform.position.x, transform.position.y, Camera.main.transform.position.z);
    }

    [ClientRpc]
    void RpcRespawn() {
        if (isLocalPlayer) {
            // Set the spawn point to origin as a default value
            Vector3 sp = Vector3.zero;

            // If there is a spawn point array and the array is not empty, pick one at random
            if (spawnPoints != null && spawnPoints.Length > 0) {
                sp = spawnPoints[UnityEngine.Random.Range(0, spawnPoints.Length)].transform.position;
            }

            // Set the player’s position to the chosen spawn point
            transform.position = sp;
        }
    }

    public void Restart() {
        //reset variables
        deaths = 0;
        kills = 0;
        health = MAX_HEALTH;
        voteToRestart = false;
        Game.instance.timer.Reset();
        // Set the player’s position to its original spawn point
        transform.position = GameObject.Find("Network Spawn " + (id + 1)).transform.position;
    }

    [Command]
    void CmdFire() {
        GameObject instantiatedProjectile = (GameObject)Instantiate(projectile, spawnPoint.transform.position, Quaternion.identity);
        Rigidbody2D prb = instantiatedProjectile.GetComponent<Rigidbody2D>();
        instantiatedProjectile.transform.rotation = transform.rotation;
        instantiatedProjectile.transform.Rotate(new Vector3(0, 0, 1), 90);
        prb.velocity = spawnPoint.transform.up * projectileSpeed;
        prb.velocity += rb.velocity;
        instantiatedProjectile.GetComponent<Projectile>().owner = this;
        NetworkServer.Spawn(instantiatedProjectile);
    }

    [Command]
    void CmdFireGrenade() {
        GameObject instantiatedGrenade = (GameObject)Instantiate(grenade, spawnPoint.transform.position, Quaternion.identity);
        Rigidbody2D prb = instantiatedGrenade.GetComponent<Rigidbody2D>();
        prb.velocity = spawnPoint.transform.up * projectileSpeed;
        prb.velocity += rb.velocity;
        instantiatedGrenade.GetComponent<Grenade>().owner = this;
        NetworkServer.Spawn(instantiatedGrenade);
    }

    [Command]
    void CmdDeathFlag() {
        GameObject instantiatedDeathFlag = (GameObject)Instantiate(deathFlag, transform.position, Quaternion.identity);
        NetworkServer.Spawn(instantiatedDeathFlag);
    }

    [Command]
    void CmdUltimate() {
        StartCoroutine(Ultimate());
    }

    [Command]
    void CmdVoteToRestart() {
        voteToRestart = true;
    }

    private void GetMovement() {

        if (Input.GetAxis("Vertical") != 0) {
            moveSpeed = Time.deltaTime * Input.GetAxis("Vertical") * moveMult;
        } else {
            moveSpeed = 0;
        }

        if (Input.GetAxis("Horizontal") != 0) {
            rotationSpeed = Time.deltaTime * Input.GetAxis("Horizontal") * rotMult;
        }


        rb.MoveRotation(rb.rotation - rotationSpeed);
        rb.velocity = transform.up * moveSpeed;
        body.GetComponent<Animator>().SetFloat("Velocity", rb.velocity.magnitude);

    }

    public void Damage(float damage, Player killer) {
        if (!isServer || invulnerable) {
            return;
        }
        health -= damage;
        if (health <= 0 && alive) {
            StartCoroutine(Death(killer));
        }
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
                StartCoroutine(Shield());
                break;
            case 2:
                CmdFireGrenade();
                break;
            case 3:
                CmdUltimate();
                break;
            default:
                break;
        }
    }
    public void Stun() {
        StartCoroutine(Stunned());
    }
    void UpdateUI() {
        healthBarRect.anchorMax = new Vector2(healthBarRect.anchorMin.x + 0.35f * (health) / MAX_HEALTH, healthBarRect.anchorMax.y);
        deathOverlay.SetActive(!alive);
        voteText.text = VOTE_STR + votes + "/" + Game.instance.GetNumPlayers();
        if (!alive) {
            timer -= Time.deltaTime;
            deathText.text = DEATH_MESSAGE + (int)timer;
        } else {
            timer = deathTime;
        }
    }
    void UpdateSprites() {

        shield.SetActive(shielded);
        model.SetActive(alive);
        if (!alive) {
            broadcastText.text = broadcast;
        }
        infoCanvas.transform.position = infoPos + transform.position;
        infoCanvas.transform.rotation = infoRot;
        healthBarMiniRect.anchorMax = new Vector2(healthBarMiniRect.anchorMin.x + 0.5f * (health) / MAX_HEALTH, healthBarMiniRect.anchorMax.y);
        Game.instance.UpdateStats(this);
    }


    void OnCollisionEnter2D(Collision2D collision) {
        if (!invulnerable && collision.gameObject.CompareTag("Projectile") && collision.gameObject.GetComponent<Projectile>().owner != this) {
            StartCoroutine(Flash());
        }
    }

    IEnumerator Flash() {
        body.GetComponent<SpriteRenderer>().color = Color.red;
        yield return new WaitForSeconds(0.5f);
        body.GetComponent<SpriteRenderer>().color = playerColor;
    }

    IEnumerator Shield() {
        shielded = true;
        invulnerable = true;
        Game.instance.PlayClip(clip);
        yield return new WaitForSeconds(shieldTime);
        invulnerable = false;
        shielded = false;
    }

    IEnumerator Ultimate() {
        moveMult *= ultiMoveMultiplier;
        rotMult *= ultiMoveMultiplier;
        InvokeRepeating("CmdFire", 0, ultimateFireRate);
        yield return new WaitForSeconds(ultimateDuration);
        CancelInvoke();
        moveMult /= ultiMoveMultiplier;
        rotMult /= ultiMoveMultiplier;
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
        if (killer != null) {
            broadcast = killer.playerName + " has slain " + playerName;
            killer.kills++;
        }
        infoCanvas.SetActive(false);
        CancelInvoke();
        deaths++;
        yield return new WaitForSeconds(deathTime);
        health = MAX_HEALTH;
        RpcRespawn();
        broadcast = "";
        yield return new WaitForEndOfFrame();
        alive = true;
        canMove = true;
        infoCanvas.SetActive(true);
    }
}

