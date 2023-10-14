using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 5.0f;

    Rigidbody2D rb;
    bool dead;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector3.left * speed;
    }

    void FixedUpdate()
    {
        if(dead) return; //safety to prevent double damage if the destroy takes a bit of time

        if(rb.position.x <= -2.5) //reached agatha
        {
            GameManager.instance.DamageAgatha(1);
            dead = true;
            Destroy(gameObject);
        }
    }
}
