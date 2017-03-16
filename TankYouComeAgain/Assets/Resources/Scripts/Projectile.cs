using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour {
    public float lifetime;
    public float damage = 10.0f;
    public Player owner;
    ParticleSystem particleSys;
    AudioSource clip;

    // Use this for initialization
    void Start() {
        particleSys = GetComponent<ParticleSystem>();
        clip = GetComponent<AudioSource>();
        Game.instance.PlayClip(clip);
    }

    // Update is called once per frame
    void Update() {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        StartCoroutine(PrettyDelete());
    }

    IEnumerator PrettyDelete()
    {
        GetComponent<Collider2D>().enabled = false;
        GetComponent<SpriteRenderer>().enabled = false;
        particleSys.Stop();
        yield return new WaitWhile(particleSys.IsAlive);
        Destroy(gameObject);
    }
}
