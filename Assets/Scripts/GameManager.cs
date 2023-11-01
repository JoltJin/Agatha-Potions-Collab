using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using TMPro;
using Unity.Burst.CompilerServices;
using Unity.Burst.Intrinsics;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Random = UnityEngine.Random;

[Serializable]
public struct IngredientInfo
{
    public Sprite sprite;
    public Color color;
}


[Serializable]
public struct EnemySpawnInfo
{
    public Sprite sprite;
    public Color color;
    public int[] weaknessPotionIngredients;
    public Sprite[] walkAnim, dieAnim, fallAnim, landAnim;
}


[Serializable]
public struct PotionCombo
{
    public int[] PotionIngredients;
    public Sprite sprite;
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

    public EnemySpawnInfo[] enemies; //store enemy templates in here, their array index is their id to use in things like levelEnemyIds
    public int[] levelEnemyIds;
    public IngredientInfo[] ingredients; //^^ same thing but for ingredients
    public int[] levelIngredientIds;
    public GameObject enemyPrefab;
    public Image[] ingredientImages;
    public Image[] selectedIngredientImages;
    public GameObject potionPrefab;
    public Image[] bubbleImages;
    public Image[] healthCandies;
    public Sprite missingHealthImg;
    public PotionCombo[] potions;
    public TextMeshProUGUI scoreTxt;
    public Transform cauldronMixPoint;
    public Canvas canvas;

    int agathaHealth = 6;
    int[] selectedIngredients = {-1, -1, -1}; //-1 = not selected yet
    short selectedCount = 0;
    bool attacking;
    int wave = 0;
    Queue<GameObject> spawnedEnemies = new Queue<GameObject>();
    int combo = 0;
    bool speedPotionOn = false;
    bool storyDoneSpawn = false;

    public static bool infiniteRandomMode;
    public static double currentScore = 0.0;
    public static EndInfo endInfo;
    public static SaveData savedata;

    private void Start()
    {
        currentScore = 0.0;
        //set ingredients
        for(int i = 0; i < ingredientImages.Length; i++)
        {
            ingredientImages[i].sprite = ingredients[levelIngredientIds[i]].sprite;
            ingredientImages[i].color = ingredients[levelIngredientIds[i]].color;
            ingredientImages[i].gameObject.GetComponent<Ingredient>().id = levelIngredientIds[i];
        }

        if (infiniteRandomMode)
        {
            StartCoroutine(Infinite());
        }
        else
        {
            StartCoroutine(Story());
        }
        StartCoroutine(UpdateBubble());
        UpdateScoreTxt();

        if (!infiniteRandomMode)
        {
            //could be made better but who cares :sunglasses:
            if(savedata.storyChapter < 3)
            {
                agathaHealth = 1;
                for (int i = agathaHealth; i < 6; i++)
                {
                    healthCandies[i].color = Color.clear;
                }
            }
            else if (savedata.storyChapter < 5)
            {
                agathaHealth = 2;
                for (int i = agathaHealth; i < 6; i++)
                {
                    healthCandies[i].color = Color.clear;
                }
            }
            else if (savedata.storyChapter < 6)
            {
                agathaHealth = 3;
                for (int i = agathaHealth; i < 6; i++)
                {
                    healthCandies[i].color = Color.clear;
                }
            }
            else if (savedata.storyChapter < 7)
            {
                agathaHealth = 4;
                for (int i = agathaHealth; i < 6; i++)
                {
                    healthCandies[i].color = Color.clear;
                }
            }
            else if (savedata.storyChapter < 9)
            {
                agathaHealth = 5;
                for (int i = agathaHealth; i < 6; i++)
                {
                    healthCandies[i].color = Color.clear;
                }
            }
        }
    }

    void SpawnEnemy(int id)
    {
        GameObject enemy = Instantiate(enemyPrefab, new Vector3(8.6f, 5.7f, 0.0f), Quaternion.identity);
        SpriteRenderer renderer = enemy.GetComponent<SpriteRenderer>();
        renderer.sprite = enemies[id].sprite;
        renderer.color = enemies[id].color;
        Enemy e = enemy.GetComponent<Enemy>();
        e.ingredientsWeakness = enemies[id].weaknessPotionIngredients;
        e.walkAnim = enemies[id].walkAnim;
        e.dieAnim = enemies[id].dieAnim;
        e.fallAnim = enemies[id].fallAnim;
        e.landAnim = enemies[id].landAnim;
        if (speedPotionOn)
        {
            e.speed *= 1.5f;
        }
        spawnedEnemies.Enqueue(enemy);
        if (spawnedEnemies.Count == 1)
        {
            StartCoroutine(UpdateBubble());
        }
    }

    void SpawnRandom()
    {
        int e = Random.Range(0, enemies.Length);
        SpawnEnemy(e);
    }

    IEnumerator Story()
    {
        float enemyDelay = 6.0f - (savedata.storyChapter / 5.0f);
        if (enemyDelay < 3.5f)
            enemyDelay = 3.5f;
        for (int i = 0; i < savedata.storyChapter; i++)
        {
            yield return new WaitForSeconds(speedPotionOn ? enemyDelay * 0.66f : enemyDelay);
            if (agathaHealth <= 0) break;
            SpawnRandom();
        }
        storyDoneSpawn = true;
    }

    IEnumerator Infinite()
    {
        while (true)
        {
            if (agathaHealth <= 0) break;
            wave++;
            print("Wave " + wave);
            float enemyDelay = 6.0f - (wave / 5.0f);
            if (enemyDelay < 3.5f)
                enemyDelay = 3.5f;
            for(int i = 0; i < wave; i++)
            {
                if (agathaHealth <= 0) break;
                SpawnRandom();
                yield return new WaitForSeconds(speedPotionOn ? enemyDelay * 0.66f : enemyDelay);
            }
            float waveDelay = 6.0f - ((float)wave)/3;
            if (waveDelay < 3.0f)
                waveDelay = 3.0f;
            yield return new WaitForSeconds(speedPotionOn ? waveDelay * 0.66f : waveDelay);
        }
    }

    void UpdateScoreTxt()
    {
        scoreTxt.text = currentScore.ToString("F0") + "\nx" + combo.ToString();
    }

    public void enemyDefeated() {
        if (agathaHealth <= 0) return;
        
        UpdateScoreTxt();
        spawnedEnemies.Dequeue();
        StartCoroutine(UpdateBubble());

        if(storyDoneSpawn && !infiniteRandomMode && spawnedEnemies.Count == 0)
        {
            //cleared story level
            SceneManager.LoadScene("Start");
            savedata.storyChapter++;
            if(savedata.storyChapter > 10)
            {
                savedata.storyChapter = 1;
            }
            SaveManager.SaveData();
        }
    }

    IEnumerator UpdateBubble()
    {
        for (int i = 0; i < 3; i++)
        {
            bubbleImages[i].color = Color.clear;
        }
        yield return new WaitForSeconds(0.5f);
        if(spawnedEnemies.Count > 0)
        {
            Enemy leftmost = spawnedEnemies.Peek().GetComponent<Enemy>();
            for (int i = 0; i < 3; i++)
            {
                bubbleImages[i].sprite = ingredients[leftmost.ingredientsWeakness[i]].sprite;
                bubbleImages[i].color = ingredients[leftmost.ingredientsWeakness[i]].color;
            }
        }
    }

    public void DamageAgatha(int dmg)
    {
        agathaHealth -= dmg;
        combo = 0;
        if (agathaHealth <= 0)
        {
            print("ded");
            endInfo = new EndInfo(infiniteRandomMode, wave);
            if (currentScore > savedata.highScore)
            {
                print("new highscore: " + currentScore);
                savedata.highScore = (float)currentScore;
                SaveManager.SaveData();
            }
            SceneManager.LoadScene("Death");
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
        selectedIngredientImages[selectedCount].sprite = ingredients[id].sprite;
        selectedIngredientImages[selectedCount].color = ingredients[id].color;
        selectedCount++;
        if(selectedCount >= 3)
        {
            //print("Created potion with ids " + selectedIngredients[0] + ", " + selectedIngredients[1] + ", " + selectedIngredients[2]);
            StartCoroutine(CraftAndThrow());
            attacking = true;
        }
    }

    IEnumerator ThrowSpeedPotion()
    {
        //StopCoroutine("ThrowSpeedPotion");
        if (!speedPotionOn)
        {
            print("Start speed potion");
            
            Enemy[] enemies = FindObjectsOfType<Enemy>();
            if (enemies.Length > 0)
            {
                speedPotionOn = true;
                Color prevcolor = scoreTxt.color;
                scoreTxt.color = new Color(0.0f, 0.8f, 1.0f);
                float ogspeed = enemies[0].speed;
                float newspeed = ogspeed * 1.5f;
                foreach (Enemy enemy in enemies)
                {
                    enemy.SetSpeed(newspeed);
                }
                yield return new WaitForSeconds(20.0f);
                speedPotionOn = false;
                foreach (Enemy enemy in enemies)
                {
                    enemy.SetSpeed(ogspeed);
                }
                scoreTxt.color = prevcolor;
            }
            print("end speed potion");
        }
    }

    IEnumerator CraftAndThrow()
    {
        Vector3[] ogpos = { selectedIngredientImages[0].transform.position, selectedIngredientImages[1].transform.position, selectedIngredientImages[2].transform.position };
        //wait a bit, maybe add a camera effect or something
        yield return new WaitForSeconds(0.5f);
        //move selected ingredients images in cauldron
        Vector3 cauldronPos = cauldronMixPoint.position;
        while (Vector3.Distance(selectedIngredientImages[0].transform.position, cauldronPos) > 0.2 &&
            Vector3.Distance(selectedIngredientImages[1].transform.position, cauldronPos) > 0.2 &&
            Vector3.Distance(selectedIngredientImages[2].transform.position, cauldronPos) > 0.2)
        {
            foreach(Image img in selectedIngredientImages)
            {
                img.transform.position += (cauldronPos - img.transform.position).normalized * 0.03f; //speed
            }
            yield return new WaitForSeconds(0.01f);
        }
        //then remove them
        int[] thrownIngredients = { selectedIngredients[0], selectedIngredients[1], selectedIngredients[2] };
        foreach (Image img in selectedIngredientImages)
        {
            img.color = new Color(0.0f, 0.0f, 0.0f, 0.0f);
        }
        selectedIngredients[0] = -1;
        selectedIngredients[1] = -1;
        selectedIngredients[2] = -1;
        for (int i = 0; i < 3; i++)
        {
            selectedIngredientImages[i].transform.position = ogpos[i];
        }
        selectedCount = 0;
        attacking = false;
        //then spawn a potion with it
        GameObject potion = Instantiate(potionPrefab, canvas.transform);
        Image potionimg = potion.GetComponent<Image>();
        foreach (PotionCombo potionCombo in potions)
        {
            if (thrownIngredients[0] == potionCombo.PotionIngredients[0] &&
                thrownIngredients[1] == potionCombo.PotionIngredients[1] &&
                thrownIngredients[2] == potionCombo.PotionIngredients[2])
            {
                potionimg.sprite = potionCombo.sprite;
                break;
            }
        }
        potion.transform.position = cauldronPos;
        //fly up
        float speedy = 0.3f;
        while (potion.transform.position.y < 6.5f && speedy > 0.0f)
        {
            potion.transform.position += new Vector3(0.0f, speedy, 0.0f);
            speedy -= 0.004f;
            yield return new WaitForSeconds(0.01f);
        }
        //speed potion
        if (thrownIngredients[0] == 3 && thrownIngredients[1] == 7 && thrownIngredients[2] == 3)
        {
            StartCoroutine(ThrowSpeedPotion());
        }
        //then move that potion to the leftmost enemy
        Enemy[] enemies = FindObjectsOfType<Enemy>();
        if (enemies.Length <= 0)
        {
            Destroy(potion);
        }
        else
        {
            Enemy leftMost = enemies[0];
            foreach (Enemy e in enemies)
            {
                if (e.transform.position.x < leftMost.transform.position.x)
                {
                    leftMost = e;
                }
            }
            float fallspeed = 0.15f;
            while (true) //cursed but should break out of it when reached
            {
                if (leftMost.IsDestroyed())
                {
                    Destroy(potion);
                    break; //if enemy reached agatha while potion is being thrown and is not longer valid
                }
                Vector3 enemyScreenPos = (leftMost.transform.position);
                float distance = Vector3.Distance(enemyScreenPos, potion.transform.position);
                if (distance < 1.0)
                {
                    if (thrownIngredients[0] == leftMost.ingredientsWeakness[0] &&
                        thrownIngredients[1] == leftMost.ingredientsWeakness[1] &&
                        thrownIngredients[2] == leftMost.ingredientsWeakness[2])
                    {
                        leftMost.Kill();
                        combo++;
                        currentScore += 10.0 * Math.Log10(combo + 1) * (speedPotionOn ? 1.5f : 1.0f);
                    }
                    Destroy(potion);
                    break;
                }
                else
                {
                    potion.transform.position += (enemyScreenPos - potion.transform.position).normalized * fallspeed;
                    fallspeed += 0.005f;
                    yield return new WaitForSeconds(0.01f);
                }
            }
        }
    }
}
