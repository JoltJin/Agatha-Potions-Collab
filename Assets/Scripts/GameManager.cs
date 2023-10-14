using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
