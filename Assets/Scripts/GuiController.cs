using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


public class GuiController : MonoBehaviour
{
    [SerializeField] Text txtTitle;
    [SerializeField] Text txtStart;
    const float DURATION = 4f;
    void Start() 
    {
         StartCoroutine ("ChangeColor");         
    } 
    IEnumerator ChangeColor() 
    { float t = 0; 
        while (t < DURATION) { 
            t += Time.deltaTime; 
            txtTitle.color = Color.Lerp(Color.black, Color.white, t/DURATION); 
            txtStart.color = Color.Lerp(Color.red, Color.yellow, t/DURATION);
            yield return null; 
        } StartCoroutine ("ChangeColor"); //reiniciar corrutina, como un bucle
    }
}
