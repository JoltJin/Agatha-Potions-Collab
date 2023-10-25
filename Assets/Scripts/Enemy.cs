using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Enemy : MonoBehaviour
{
    public float speed = 5.0f;
    public int[] ingredientsWeakness;
    public Sprite[] walkAnim, dieAnim, fallAnim;
    public float deathPosX = -4.0f;

    Rigidbody2D rb;
    bool dead;
    int animIndex;
    SpriteRenderer spriteRenderer;

    // Start is called before the first frame update
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
        rb.velocity = Vector3.down * 3;
        spriteRenderer = GetComponent<SpriteRenderer>();
        animIndex = 0;
        StartCoroutine(DoFallAnim());
    }

    IEnumerator DoFallAnim()
    {
        while (!dead && rb.position.y > 1.0f)
        {
            spriteRenderer.sprite = fallAnim[animIndex++];
            animIndex %= fallAnim.Length;
            yield return new WaitForSeconds(0.15f);
        }
        rb.velocity = Vector3.left * speed;
        rb.position = new Vector2(rb.position.x, 1.0f);
        animIndex = 0;
        StartCoroutine(DoWalkAnim());
    }

    IEnumerator DoWalkAnim()
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

    public void Kill()
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
