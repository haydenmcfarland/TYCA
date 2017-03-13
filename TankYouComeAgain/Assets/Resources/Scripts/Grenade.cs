using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

public class Grenade : NetworkBehaviour {
    public float lifetime = 5f;
    public float radius = 2f;
    public float damage = 25f;
    public Player owner;
    ParticleSystem ps;
    Rigidbody2D rb;
    AudioSource clip;
	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
        rb = GetComponent<Rigidbody2D>();
        clip = GetComponent<AudioSource>();
        StartCoroutine(Explode());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player") && !col.gameObject.GetComponentInParent<Player>().invulnerable && col.gameObject.GetComponentInParent<Player>()!= owner) {
            Player p = col.gameObject.GetComponentInParent<Player>();
            p.Stun();
            p.Damage(damage, owner);
            Destroy(gameObject);
        }
    }

    IEnumerator Explode() {
        yield return new WaitForSeconds(lifetime);
        clip.Play();
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits) {
            if (hit.gameObject.CompareTag("Player") && !hit.gameObject.GetComponentInParent<Player>().invulnerable && hit.gameObject.GetComponentInParent<Player>() != owner) {
                Player p = hit.gameObject.GetComponentInParent<Player>();
                p.Stun();
                p.Damage(damage, owner);
            }
        }
        rb.velocity = Vector3.zero;
        ps.Play();
        Destroy(gameObject, ps.main.duration);
    }
}
