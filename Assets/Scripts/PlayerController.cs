using System;
using System.Collections.Generic;

using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] float speed; //velocidad que le asignaremos desde el editor
    [SerializeField] float jumpSpeed; //variable para asignar la fuerza de salto
    [SerializeField] GameObject shoot; //prefab de los disparos
    [SerializeField] AudioClip jumpFX; //audioclip de salto
    [SerializeField] AudioClip hurtFX; //audioclip de daño cuando se entra en contacto con un enemigo
    AudioSource sfx; //referencia al propio AudioSource
    Rigidbody2D rb; //referencias al propio RigidBody, Animator y Collider, para operaciones de cambio de animación y de suspensión de componentes en caso de pausa o muerte
    Animator anim;
    Collider2D col;
    const float SHOOT_OFFSET = 0.5f; //margen para que los disparos no salgan del propio sprite del avatar del jugador    
    float moveX; //referencia al movimiento horizontal, hacia izq o dcha
    bool active; //booleano para marcar al jugador como activo
    bool jump; //booleanos para cambios de estado de las animaciones
    bool run;    
    //en el Start() se inicializan las referencias y se marca como activo al avatar del jugador
    void Start()
    {
        active = true;
        rb = GetComponent<Rigidbody2D>();
        col = GetComponent<Collider2D>();
        anim = GetComponent<Animator>();
        sfx = GetComponent<AudioSource>();
    }

    void Update()
    {
        moveX = Input.GetAxis("Horizontal"); //obtenemos el movimiento en la coord. X accediendo a las pulsaciones de teclas o movimiento joystick en horizontal
        if (!jump && Input.GetButtonDown("Jump")) //ídem salto
        {
            jump = true; //transición a estado de "saltando"
        }
        if (Input.GetKeyDown(KeyCode.M) && active && gameObject.GetComponent<SpriteRenderer>().enabled == true) //si se pulsa m inicia el método para disparar
        {
            Shoot();
        }
    }

    void FixedUpdate()
    {
        if (active && gameObject.GetComponent<SpriteRenderer>().enabled) //sólo estando activos y siendo visibles (no en pausa o muerto) se puede saltar o correr, o que gire el sprite
        {
            Run();
            Flip();
            Jump();
        }
    }

    //si se entra en contacto con un enemigo se reproduce sonido de daño y se modifica la salud del jugador en el método correspondiente del GameManager
    private void OnCollisionEnter2D(Collision2D other)
    {
        EnemyController enemyController = other.gameObject.GetComponent<EnemyController>();
        if (enemyController != null)
        {
            string tag = other.gameObject.tag;
            List<string> enemyTags = enemyController.GetTags();
            if (enemyTags.Contains(tag))
            {
                sfx.clip = hurtFX;
                sfx.Play();
                GameManager.GetInstance().ModifyHealth();
            }
        }
    }    
    //se mueve al jugador en la dirección indicada y se activa la animación correspondiente 
    void Run()
    {
        Vector2 vel = new Vector2(moveX * speed * Time.fixedDeltaTime, rb.velocity.y); //vector en X por fuerza serializaada por el fixedDeltaTime
        rb.velocity = vel; //se añade fuerza de 
        run = Math.Abs(rb.velocity.x) > Mathf.Epsilon; //el booleano varía si se detecta movimiento
        anim.SetBool("isRunning", run); //activación de la animación, en función del booleano run, que varía en tiempo real de ejecución

    }
    //girar al jugador según se desplace a un lado o a otro
    void Flip()
    {
        float vx = rb.velocity.x;
        if (Mathf.Abs(vx) > Mathf.Epsilon)
        {
            transform.localScale = new Vector2(Mathf.Sign(vx), 1);
        }
    }    
    void Jump()
    {
        if (!jump)
        {
            return;
        }
        jump = false;
        if (!col.IsTouchingLayers(LayerMask.GetMask("Terrain", "Platforms", "Enemies"))) //sólo se salta en contacto con capas "saltables"
        {
            return; //de modo que si no estamos en contacto con una de esas capas, se sale del método
        }
        anim.SetTrigger("isJumping"); //se varía la animación
        sfx.clip = jumpFX; //se reproduce sonido de salto
        sfx.Play();
        rb.velocity += new Vector2(0, jumpSpeed); //se aplica fuerza de salto
    }
    void Shoot()
    {
        if (run)
        {
            anim.SetBool("isShootingAndRunning", true); //se activa una animación u otra en función de si dispara quieto o moviéndose
        }
        else
        {
            anim.SetBool("isStanded", true);
        }
        float vectorX = transform.localScale.x>0?1:-1;
        GameObject newShoot = Instantiate(shoot, new Vector3(transform.position.x + SHOOT_OFFSET * vectorX, transform.position.y+0.2f, 0), Quaternion.identity);
        //pasar la escala local del jugador al disparo, para orientar su sprite
        newShoot.GetComponent<ShootController>().SetPlayerScale(transform.localScale.x);
        //reiniciar las animaciones después de un tiempo determinado
        Invoke("ResetAnimations", 0.5f); //ajustamos el tiempo después de muchas pruebas
    }

    //método para que no se queden "enganchadas" las animaciones de disparo
    void ResetAnimations()
    {
        anim.SetBool("isShootingAndRunning", false);
        anim.SetBool("isStanded", false);
    }

    public void SetActive(bool active)
    {
        this.active = active;        
        rb.simulated = active; //reactivamos RigidBody        
        anim.enabled = active; //si el juego se reanuda, también reactivamos los componentes de animación
    }

    public bool IsActive() //acceso público al estado de activo o no
    {
        return active;
    }
}