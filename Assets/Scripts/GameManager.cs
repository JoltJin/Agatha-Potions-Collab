using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public struct IngredientInfo
{
    public Texture2D texture;
    public Color color;
}


[Serializable]
public struct EnemySpawnInfo
{
    public Sprite sprite;
    public Color color;
    public int[] weaknessPotionIngredients;
    public Sprite[] walkAnim, dieAnim;
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

    public bool infiniteRandomMode;
    public EnemySpawnInfo[] enemies; //store enemy templates in here, their array index is their id to use in things like levelEnemyIds
    public int[] levelEnemyIds;
    public IngredientInfo[] ingredients; //^^ same thing but for ingredients
    public int[] levelIngredientIds;
    public GameObject enemyPrefab;
    public RawImage[] ingredientImages;
    public RawImage[] selectedIngredientImages;
    public GameObject potionPrefab;
    public RawImage[] bubbleImages;
    public Image[] healthCandies;
    public Sprite missingHealthImg;

    int agathaHealth = 6;
    int[] selectedIngredients = {-1, -1, -1}; //-1 = not selected yet
    short selectedCount = 0;
    bool attacking;
    Canvas canvas;
    Queue<GameObject> spawnedEnemies = new Queue<GameObject>();

    private void Start()
    {
        canvas = FindObjectOfType<Canvas>();
        //set ingredients
        for(int i = 0; i < ingredientImages.Length; i++)
        {
            ingredientImages[i].texture = ingredients[levelIngredientIds[i]].texture;
            ingredientImages[i].color = ingredients[levelIngredientIds[i]].color;
            ingredientImages[i].gameObject.GetComponent<Ingredient>().id = levelIngredientIds[i];
        }

        if (infiniteRandomMode)
        {
            StartCoroutine(Infinite());
        }
        else
        {
            //spawn enemies
            //could spawn them as the level unfolds if needed but for now just spawn them off-screen :sip:
            Vector3 position = new Vector3(10.0f, 1.03f, 0.0f); //spawn pos for the first enemy
            for (int i = 0; i < levelEnemyIds.Length; i++)
            {
                SpawnEnemy(levelEnemyIds[i]);
                position.x += 3.0f; //distance between enemies
            }
        }
        StartCoroutine(UpdateBubble());
    }

    void SpawnEnemy(int id)
    {
        GameObject enemy = Instantiate(enemyPrefab, new Vector3(10.0f, 1.03f, 0.0f), Quaternion.identity);
        SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
        renderer.sprite = enemies[id].sprite;
        renderer.color = enemies[id].color;
        Enemy e = enemy.GetComponent<Enemy>();
        e.ingredientsWeakness = enemies[id].weaknessPotionIngredients;
        e.walkAnim = enemies[id].walkAnim;
        e.dieAnim = enemies[id].dieAnim;
        spawnedEnemies.Enqueue(enemy);
    }

    void SpawnRandom()
    {
        int e = Random.Range(0, enemies.Length);
        SpawnEnemy(e);
    }

    IEnumerator Infinite()
    {
        while (true)
        {
            if (agathaHealth <= 0)
            {
                break;
            }
            SpawnRandom();
            yield return new WaitForSeconds(3.0f);
        }
    }

    public void enemyDefeated() {
        if (agathaHealth <= 0) return;
        spawnedEnemies.Dequeue();
        StartCoroutine(UpdateBubble());
    }

    IEnumerator UpdateBubble()
    {
        for (int i = 0; i < 3; i++)
        {
            bubbleImages[i].color = Color.clear;
        }
        yield return new WaitForSeconds(0.5f);
        Enemy leftmost = spawnedEnemies.Peek().GetComponent<Enemy>();
        for(int i = 0; i < 3; i++)
        {
            bubbleImages[i].texture = ingredients[leftmost.ingredientsWeakness[i]].texture;
            bubbleImages[i].color = ingredients[leftmost.ingredientsWeakness[i]].color;
        }
    }

    public void DamageAgatha(int dmg)
    {
        agathaHealth -= dmg;
        if (agathaHealth <= 0)
        {
            print("ded");
            SceneManager.LoadScene(0);
            return;
        }
        for (int i = agathaHealth; i < 6; i++)
        {
            healthCandies[i].sprite = missingHealthImg;
        }
        print("hp now " + agathaHealth);
        
    }
    
    public void ClickedIngredient(int id, Vector3 position) //todo use position for animating the ingredient drag to the cauldron
    {
        if (attacking) return;

        selectedIngredients[selectedCount] = id;
        selectedIngredientImages[selectedCount].texture = ingredients[id].texture;
        selectedIngredientImages[selectedCount].color = ingredients[id].color;
        selectedCount++;
        if(selectedCount >= 3)
        {
            print("Created potion with ids " + selectedIngredients[0] + ", " + selectedIngredients[1] + ", " + selectedIngredients[2]);
            StartCoroutine(CraftAndThrow());
            attacking = true;
        }
    }

    IEnumerator CraftAndThrow()
    {
        Vector3[] ogpos = { selectedIngredientImages[0].transform.position, selectedIngredientImages[1].transform.position, selectedIngredientImages[2].transform.position };
        //wait a bit, maybe add a camera effect or something
        yield return new WaitForSeconds(0.5f);
        //move selected ingredients images in cauldron
        Vector3 cauldronPos = new Vector3(420, 125, 0);
        while (Vector3.Distance(selectedIngredientImages[0].transform.position, cauldronPos) > 0.5 &&
            Vector3.Distance(selectedIngredientImages[1].transform.position, cauldronPos) > 0.5 &&
            Vector3.Distance(selectedIngredientImages[2].transform.position, cauldronPos) > 0.5)
        {
            foreach(RawImage img in selectedIngredientImages)
            {
                img.transform.position += (cauldronPos - img.transform.position).normalized * 2.0f; //speed
            }
            yield return new WaitForSeconds(0.01f);
        }
        //then remove them
        foreach (RawImage img in selectedIngredientImages)
        {
            img.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        //then spawn a potion with it
        GameObject potion = Instantiate(potionPrefab, canvas.transform);
        potion.transform.position = cauldronPos;
        //fly up
        while (potion.transform.position.y < Screen.height * 1.3)
        {
            potion.transform.position += new Vector3(0.0f, 15.0f, 0.0f);
            yield return new WaitForSeconds(0.01f);
        }
        //then move that potion to the leftmost enemy
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        Enemy leftMost = enemies[0];
        foreach(Enemy e in enemies)
        {
            if(e.transform.position.x < leftMost.transform.position.x)
            {
                leftMost = e;
            }
        }
        while (true) //cursed but should break out of it when reached
        {
            if (leftMost.IsDestroyed())
            {
                Destroy(potion);
                break; //if enemy reached agatha while potion is being thrown and is not longer valid
            }
            //we have to use WorldToScreenPoint because enemies are sprites in world and the rest are images on canvas... maybe we should make them all the same thing
            Vector3 enemyScreenPos = Camera.main.WorldToScreenPoint(leftMost.transform.position);
            float distance = Vector3.Distance(enemyScreenPos, potion.transform.position);
            if(distance < 2.0)
            {
                //this can probably be simplified but it's ok
                bool hit = true;
                foreach(int ingredient in selectedIngredients)
                {
                    bool inWeakness = false;
                    foreach (int weakness in leftMost.ingredientsWeakness)
                    {
                        if(weakness == ingredient)
                        {
                            inWeakness = true;
                        }
                    }
                    if (!inWeakness)
                    {
                        hit = false;
                        break;
                    }
                }
                if (hit)
                {
                    leftMost.Kill();
                }
                Destroy(potion);
                break;
            }
            else
            {
                potion.transform.position += (enemyScreenPos - potion.transform.position).normalized * 8.0f; //speed
                yield return new WaitForSeconds(0.01f);
            }
        }

        for(int i = 0; i < 3; i++)
        {
            selectedIngredientImages[i].transform.position = ogpos[i];
        }
        selectedIngredients[0] = -1;
        selectedIngredients[1] = -1;
        selectedIngredients[2] = -1;
        selectedCount = 0;
        attacking = false;
    }
}
