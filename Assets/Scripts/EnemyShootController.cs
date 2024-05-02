using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyShootController : MonoBehaviour
{
    const float SPEED = 20; 
    const float HIT_OFFSET = 0.5f; //pequeño margen para que el disparo no salga directamente del sprite
    float temp;    //tiempo de vida del proyectil antes de autodestruirse
    [SerializeField] AudioClip playerHurtFX; //referencia al clip de audio que se reproduce cuando se provoca daño al jugador
    [SerializeField] AudioClip impactFX; //audioclip de impacto con el jugador
    [SerializeField] GameObject impact;   //prefab de impacto en el jugador
    Animator playerAnim; //referencia al Animator del jugador
    AudioSource sfx; //referencia al propio AudioSource del objeto
    SpriteRenderer spriteRenderer; //referncia al propio SpriteRenderer, para cambiar la orientación del sprite del disparo
    float scaleX; //variable para reorientar el disparo hacia un lado o hacia el otro

    void Start() //inicialización de las referencias necesarias
    {
        playerAnim = GameObject.FindGameObjectWithTag("Player").GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
        spriteRenderer = GetComponent<SpriteRenderer>();     
        temp = 3; //inicializamos a 3 el tiempo que "vivirá" un disparo
    }

    //los disparos se destruyen tras un tiempo
    //su sprite se orienta hacia un lado o hacia otro en función de hacia dónde mire el jugador
    //obteniendo su valor scaleX en el método SetPlayerScale
    void Update()
    {
        temp -= Time.deltaTime; //temporizador para autodestruir el disparo
        if (temp < 0)
        {
            Destroy(gameObject);
        }   
        Vector2 moveDirection = scaleX > 0 ? Vector2.right : Vector2.left; //reorientación del sprite del disparo
        spriteRenderer.flipX = moveDirection == Vector2.left;
        transform.Translate(moveDirection * SPEED * Time.deltaTime); //movimiento del disparo
    }

    //se detectan colisiones de los disparos con el jugador, y se activa la animación de daño al jugador
    //se generan los sonidos correspondientes, y se instancia el prefab de impacto
    void OnTriggerEnter2D(Collider2D other)
    {
        string tag = other.gameObject.tag;
        if (tag == "Player")       
        {            
            GameManager.GetInstance().ModifyHealth();
            sfx.clip = impactFX;
            sfx.Play();
            playerAnim.SetTrigger("isShooted");
            GameObject hit = Instantiate(impact, new Vector3(other.transform.position.x + HIT_OFFSET, transform.position.y, 0), Quaternion.identity);
            Destroy (hit, 1f);
            sfx.clip = playerHurtFX;
            sfx.Play();
            playerAnim.SetTrigger ("toNormal");
            Destroy (gameObject);
        }
        else if (tag == "Terrain" || tag == "Platforms") //si el impacto es contra el terreno o las plataformas, se destruye el disparo
        {            
            Destroy(gameObject);
        }
    }
    //obtención de la dirección en la que orientar el disparo
    public void SetPlayerScale(float scaleX)
    {
        this.scaleX = scaleX;
    }

}
