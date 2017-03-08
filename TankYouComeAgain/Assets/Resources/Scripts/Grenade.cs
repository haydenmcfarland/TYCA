using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Grenade : MonoBehaviour {
    public float lifetime = 5f;
    public float radius = 2f;
    public float damage = 25f;
    ParticleSystem ps;
    Rigidbody2D rb;
	// Use this for initialization
	void Start () {
        ps = GetComponent<ParticleSystem>();
        rb = GetComponent<Rigidbody2D>();
        StartCoroutine(Explode());
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    void OnCollisionEnter2D(Collision2D col) {
        if (col.gameObject.CompareTag("Player") && !col.gameObject.GetComponentInParent<Player>().invulnerable) {
            Player p = col.gameObject.GetComponentInParent<Player>();
            p.Stun();
            p.Damage(damage);
            Destroy(gameObject);
        }
    }

    IEnumerator Explode() {
        yield return new WaitForSeconds(lifetime);
        Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, radius);
        foreach (Collider2D hit in hits) {
            Debug.Log(hit);
            if (hit.gameObject.CompareTag("Player") && !hit.gameObject.GetComponentInParent<Player>().invulnerable) {
                Player p = hit.gameObject.GetComponentInParent<Player>();
                p.Stun();
                p.Damage(damage);
            }
        }
        rb.velocity = Vector3.zero;
        ps.Play();
        Destroy(gameObject, ps.main.duration);
    }
}
