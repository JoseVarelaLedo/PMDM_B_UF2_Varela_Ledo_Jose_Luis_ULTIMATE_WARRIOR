using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//funcionamiento análogo al EnemyShootController, pero para los disparos del jugador
//única diferencia es que se llama al método para incrementar el contador de impactos
//del script asociado al enemigo con el que se colisiona, y así poder destruirlo
public class ShootController : MonoBehaviour
{
    [SerializeField] float speed;
    [SerializeField] float temp;    
    [SerializeField] AudioClip impactFX;    
    [SerializeField] GameObject impact;  
    AudioSource sfx;
    SpriteRenderer spriteRenderer;
    List <string> enemies;   
    float playerScaleX; // Escala local del jugador
    

    void Start()
    {
        sfx = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>(); //obtener referencia al componente SpriteRenderer propio       
        enemies = EnemyController.GetPublicTags();
    }

    void Update()
    {
        temp -= Time.deltaTime;
        if (temp<0){
            Destroy (gameObject);
        }              
        //mover el disparo en la dirección determinada por la escala local del jugador
        Vector2 moveDirection = playerScaleX > 0 ? Vector2.right : Vector2.left;
        //invertir el sprite si el disparo va hacia la izquierda
        spriteRenderer.flipX = moveDirection == Vector2.left;
        transform.Translate(moveDirection * speed * Time.deltaTime);         
    }

    void OnTriggerEnter2D(Collider2D other) 
    {        
        string tag = other.gameObject.tag;
        if (enemies.Contains(tag))
        {              
            sfx.clip = impactFX;
            sfx.Play();
            other.gameObject.GetComponent<EnemyController>().IncreaseHitCount();
            GameObject hit = Instantiate(impact, new Vector3(other.transform.position.x, transform.position.y, 0), Quaternion.identity);
            Destroy (hit, 1f);  
            Destroy (gameObject);                    
        }
        else if (tag == "Terrain" || tag == "Platforms")
        {            
            Destroy (gameObject);
        }
    }

     //método para obtener la orientación del jugador
    public void SetPlayerScale(float scaleX)
    {
        playerScaleX = scaleX;
    }
}
