using System;
using System.Collections;
using System.Collections.Generic;
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

    int[] selectedIngredients = {-1, -1, -1}; //-1 = not selected yet
    short selectedCount = 0;

    private void Start()
    {
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
    
    public void ClickedIngredient(int id, Vector3 position) //todo use position for animating the ingredient drag to the cauldron
    {
        selectedIngredients[selectedCount] = id;
        selectedCount++;
        if(selectedCount >= 3)
        {
            print("Created potion with ids " + selectedIngredients[0] + ", " + selectedIngredients[1] + ", " + selectedIngredients[2]);
            selectedIngredients[0] = -1;
            selectedIngredients[1] = -1;
            selectedIngredients[2] = -1;
            selectedCount = 0;
        }
    }
}
