using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public struct EnemySpawnInfo
{
    public Sprite sprite;
    public Color color;
}

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    private void Awake()
    {
        if (instance)
        {
            Destroy(gameObject);
            return;
        }
        instance = this;
    }

    public int agathaHealth = 10;
    public EnemySpawnInfo[] enemies; //store enemy templates in here, their array index is their id to use in things like levelEnemyIds
    public int[] levelEnemyIds;
    public GameObject enemyPrefab;

    private void Start()
    {
        //could spawn them as the level unfolds if needed but for now just spawn them off-screen :sip:
        Vector3 position = new Vector3(10.0f, 1.03f, 0.0f); //spawn pos for the first enemy
        for(int i = 0; i < levelEnemyIds.Length; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
            renderer.sprite = enemies[levelEnemyIds[i]].sprite;
            renderer.color = enemies[levelEnemyIds[i]].color;

            position.x += 2.0f; //distance between enemies
        }
    }

    public void DamageAgatha(int dmg)
    {
        agathaHealth -= dmg;
        print("hp now " + agathaHealth);
        if (agathaHealth <= 0)
        {
            print("ded");
        }
    }
    

}
