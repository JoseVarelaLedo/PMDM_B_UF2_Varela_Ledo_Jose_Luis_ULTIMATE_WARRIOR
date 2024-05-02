using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using UnityEngine;


//funcionamiento muy similar al controlador InstructionsController
public class GameOverController : MonoBehaviour
{
    AudioSource sfx;     
    [SerializeField] AudioClip start;
    [SerializeField] Text txtMessage;  
    const float DURATION = 4f;   
   
    void Start()
    {
        sfx = GetComponent<AudioSource>();
        StartCoroutine ("ChangeColor");
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.anyKeyDown) 
        { 
            sfx.clip = start;
            sfx.Play();           
            StartCoroutine("StartAgain");             
        } 
    }
    IEnumerator ChangeColor() 
    {
        float t = 0; 
        while (t < DURATION) { 
            t += Time.deltaTime;            
            txtMessage.color = Color.Lerp(Color.yellow, Color.green, t/DURATION);
            yield return null; 
        } StartCoroutine ("ChangeColor"); //reiniciar corrutina, como un bucle
    }

    IEnumerator StartAgain() 
    {        
       SceneManager.LoadScene(1);
       yield return null; 
    } 
}
