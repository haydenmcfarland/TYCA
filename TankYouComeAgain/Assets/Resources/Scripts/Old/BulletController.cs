using System.Collections;
using UnityEngine;

public class BulletController : MonoBehaviour {

    private float maxSize;
    private float limitSize;
    public string bulletName = "Bullet1";

	// Use this for initialization
	void Start () {
        maxSize = transform.localScale.x;
        limitSize = transform.localScale.x * 10;
	}
	
	// Update is called once per frame
	void Update () {
		
	}

    private void OnCollisionEnter2D(Collision2D collision)
    {
        if (collision.transform.name.Contains(bulletName))
        {
            if (collision.transform.GetComponent<Rigidbody2D>().velocity.magnitude < GetComponent<Rigidbody2D>().velocity.magnitude)
            {
                if (collision.transform.localScale.x > transform.localScale.x)
                {
                    GetComponent<Rigidbody2D>().velocity = collision.transform.GetComponent<Rigidbody2D>().velocity*2;
                    transform.position = collision.transform.position;
                    transform.localScale = collision.transform.localScale;
                    maxSize = collision.transform.localScale.x;

                    Destroy(collision.gameObject);
                }
                else
                {
                    Destroy(collision.gameObject);
                    maxSize *= 2;
                    StartCoroutine(Scale());
                }
            }
        }
    }

    IEnumerator Scale()
    {
        float timer = 0;
        while(true)
        {
            while(maxSize > transform.localScale.x && transform.localScale.x <= limitSize)
            {
                timer += Time.deltaTime;
                transform.localScale += new Vector3(1, 1, 0) * Time.deltaTime;
                yield return null;
            }

            yield return null;
        }
    }
}


