using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Ingredient : MonoBehaviour, IPointerClickHandler
{
    public int id;

    public void OnPointerClick(PointerEventData eventData)
    {
        //print("Clicked " + id);
        GameManager.instance.ClickedIngredient(id, gameObject.transform.position);
    }
}
