using UnityEngine;
using System.Collections;
using UnityEngine.Networking;

public class Projectile : NetworkBehaviour {
    public float lifetime;
    public float damage = 10.0f;
    public int assignedID = -1;

    // Use this for initialization
    void Start() {
    }

    // Update is called once per frame
    void Update() {
        Destroy(gameObject, lifetime);
    }

    void OnCollisionEnter2D(Collision2D collision) {
        if (collision.gameObject.CompareTag("Player") && collision.gameObject.GetComponent<Player>().id != assignedID) {
            if (isServer) {
                collision.gameObject.GetComponent<Player>().Damage(damage);
            }
        }
        Destroy(gameObject);
    }


}
