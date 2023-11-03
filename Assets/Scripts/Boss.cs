using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Boss : Enemy
{
    int hp = 3;

    public void Go()
    {
        transform.localScale = new Vector3(2.0f, 2.0f, 2.0f);

        spriteRenderer = GetComponent<SpriteRenderer>();
        rb = GetComponent<Rigidbody2D>();
        rb.position = new Vector3(rb.position.x, walkY, 0.0f);
        fell = true;
        rb.velocity = Vector3.left * speed;
        rb.position = new Vector2(rb.position.x, walkY);
        animIndex = 0;
        StartCoroutine(DoWalkAnim());
    }

    public override void Kill()
    {
        hp--;
        if(hp <= 0)
        {
            base.Kill();
        }
    }
}
