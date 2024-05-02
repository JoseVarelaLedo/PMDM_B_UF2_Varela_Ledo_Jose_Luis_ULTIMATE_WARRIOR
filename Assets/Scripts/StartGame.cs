using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

//funcionamiento muy similar a InstructionsController, sólo que para la pantalla inicial
public class StartGame : MonoBehaviour 
{ 
    AudioSource sfx;     
    [SerializeField] Text title; 
    [SerializeField] Text pressStart;
    [SerializeField] AudioClip start;
    
    void Start()
    {
        sfx = GetComponent<AudioSource>();
    }
    void Update() 
    { 
        if (Input.anyKeyDown) 
        { 
            sfx.clip = start;
            sfx.Play();
            title.enabled = false; 
            pressStart.enabled = false;
            StartCoroutine("StartNextLevel"); 
        } 
    } 
    IEnumerator StartNextLevel() 
    {        
       SceneManager.LoadScene(1);
       yield return null; 
    } 
}
