using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.RuleTile.TilingRuleOutput;

public class Boss : Enemy
{
    int hp = 3;
    public int id;

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
        animDelayMult = 2.0f;
        StartCoroutine(DoWalkAnim());
    }

    public override void Kill()
    {
        hp--;
        if(hp <= 0)
        {
            base.Kill();
        }
        else
        {
            StopAllCoroutines();
            StartCoroutine(HitEffect());
        }
    }

    IEnumerator HitEffect()
    {
        rb.velocity = Vector3.zero;
        float newscale = 0.5f + 0.5f * hp;
        float scale = transform.localScale.x;
        float step = (newscale - scale) / 30.0f;
        for(int i = 0; i < 30; i++)
        {
            scale += step;
            transform.localScale = new Vector3(scale, scale, scale);
            transform.position = new Vector3(transform.position.x, scale * 0.8f - 0.3f, 0.0f);
            yield return new WaitForSeconds(0.02f);
        }
        GameManager.instance.SetRandomBossID(this);
        rb.velocity = Vector3.left * speed;
        StartCoroutine(DoWalkAnim());
    }
}
