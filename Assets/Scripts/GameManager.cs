using System;
using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

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
    public IngredientInfo[] ingredients; //^^ same thing but for ingredients
    public int[] levelIngredientIds;
    public GameObject enemyPrefab;
    public RawImage[] ingredientImages;
    public RawImage[] selectedIngredientImages;
    public GameObject potionPrefab;
    public RawImage[] bubbleImages;

    int[] selectedIngredients = {-1, -1, -1}; //-1 = not selected yet
    short selectedCount = 0;
    bool attacking;
    Canvas canvas;
    int enemiesDefeated;

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

        //spawn enemies
        //could spawn them as the level unfolds if needed but for now just spawn them off-screen :sip:
        Vector3 position = new Vector3(10.0f, 1.03f, 0.0f); //spawn pos for the first enemy
        for(int i = 0; i < levelEnemyIds.Length; i++)
        {
            GameObject enemy = Instantiate(enemyPrefab, position, Quaternion.identity);
            SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
            renderer.sprite = enemies[levelEnemyIds[i]].sprite;
            renderer.color = enemies[levelEnemyIds[i]].color;
            enemy.GetComponent<Enemy>().ingredientsWeakness = enemies[levelEnemyIds[i]].weaknessPotionIngredients;

            position.x += 2.0f; //distance between enemies
        }
        UpdateBubble();
    }

    public void enemyDefeated() { 
        enemiesDefeated++;
        if (enemiesDefeated >= levelEnemyIds.Length)
        {
            print("LEVEL CLEARED");
            return;
        }
        UpdateBubble();
    }

    void UpdateBubble()
    {
        EnemySpawnInfo leftmost = enemies[levelEnemyIds[enemiesDefeated]];
        for(int i = 0; i < 3; i++)
        {
            bubbleImages[i].texture = ingredients[leftmost.weaknessPotionIngredients[i]].texture;
            bubbleImages[i].color = ingredients[leftmost.weaknessPotionIngredients[i]].color;
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
        //then spawn a potion with it, todo: potion
        GameObject potion = Instantiate(potionPrefab, canvas.transform);
        potion.transform.position = cauldronPos;
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
            if(distance < 0.5)
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
                    Destroy(leftMost.gameObject);
                }
                Destroy(potion);
                break;
            }
            else
            {
                potion.transform.position += (enemyScreenPos - potion.transform.position).normalized * 3.0f; //speed
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
