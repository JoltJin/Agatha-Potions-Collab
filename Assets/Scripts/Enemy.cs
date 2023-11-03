using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 5.0f;
    public int[] ingredientsWeakness;
    public Sprite[] walkAnim, dieAnim, fallAnim, landAnim;
    public float deathPosX = -4.0f, walkY = 0.5f, fallspeed = 0.1f;

    protected Rigidbody2D rb;
    protected bool dead, fell;
    protected int animIndex;
    protected SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        //rb.velocity = Vector3.down * 3;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animIndex = 0;
        fell = false;
        StartCoroutine(DoFallAnim());
    }

    IEnumerator DoFallAnim()
    {
        while (!dead && rb.position.y > walkY)
        {
            spriteRenderer.sprite = fallAnim[animIndex++];
            animIndex %= fallAnim.Length;
            for(int i = 0; i < 6; i++)
            {
                if (rb.position.y < walkY + fallspeed)
                {
                    rb.position -= new Vector2(0.0f, rb.position.y - walkY);
                }
                else
                {
                    rb.position -= new Vector2(0.02f, fallspeed);
                }
                yield return new WaitForSeconds(0.025f);
            }
        }
        spriteRenderer.sprite = landAnim[0];
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.sprite = landAnim[1];
        yield return new WaitForSeconds(0.15f);
        spriteRenderer.sprite = landAnim[0];
        yield return new WaitForSeconds(0.15f);
        fell = true;
        rb.velocity = Vector3.left * speed;
        rb.position = new Vector2(rb.position.x, walkY);
        animIndex = 0;
        StartCoroutine(DoWalkAnim());
    }

    protected IEnumerator DoWalkAnim()
    {
        while (!dead)
        {
            spriteRenderer.sprite = walkAnim[animIndex++];
            animIndex %= walkAnim.Length;
            yield return new WaitForSeconds(0.15f);
        }
    }

    IEnumerator DoDeathAnim()
    {
        animIndex = 0;
        while (animIndex < dieAnim.Length)
        {
            spriteRenderer.sprite = dieAnim[animIndex++];
            yield return new WaitForSeconds(0.15f);
        }
        Destroy(gameObject);
    }

    public void SetSpeed(float speed)
    {
        this.speed = speed;
        if (fell && rb)
        {
            rb.velocity = Vector3.left * speed;
        }
    }

    public virtual void Kill()
    {
        StopAllCoroutines();
        dead = true;
        rb.velocity = Vector3.zero;
        StartCoroutine(DoDeathAnim());
    }

    void FixedUpdate()
    {
        if(dead) return; //safety to prevent double damage if the destroy takes a bit of time

        if(rb.position.x <= deathPosX) //reached agatha
        {
            GameManager.instance.DamageAgatha(1);
            dead = true;
            Destroy(gameObject);
        }
    }

    private void OnDestroy()
    {
        GameManager.instance.enemyDefeated();
    }
}
