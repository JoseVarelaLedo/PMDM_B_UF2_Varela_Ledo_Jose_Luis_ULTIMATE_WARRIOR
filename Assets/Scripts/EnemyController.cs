using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EnemyController : MonoBehaviour
{
    [SerializeField] GameObject shoot; //prefab para instanciar disparos
    [SerializeField] GameObject enemyExplosion; //prefab con la animación de la "explosión" de un enemigo
    [SerializeField] AudioClip enemyDestroy;    //sonido que se reproducirá al destruirse
    const float SHOOT_OFFSET = 0.5f;    //margen para que el disparo no salga directamente del sprite
    const float CONTACT_OFFSET = 0.5f;  //margen que necesitamos para detección de contactos con avatar del jugador
    const float JUMP_ANGLE = 45f;   //ángulo de salto del enemigo cuando está justo encima del jugador
    const float JUMP_FORCE = 10f;   //fuerza aplicada en el salto
    const float DESTROY_DELAY = 7f; //retraso en la destrucción cuando se queda atascado un enemigo
    const float JUMP_DELAY = 1.8f; //constante para que el enemigo intente un salto antes de ese tiempo si está atascado en un sitio
    Transform playerTransform;  //referencia al transform del avatar del jugador
    GameObject player;  //referencia al avatar del jugador
    AudioSource sfx;
    Animator anim; //referencia al propio Animator
    GameObject spawner; //referencia al objeto Spawner, para tener acceso a su script asociado
    string enemyType;   //referencia al string de la etiqueta presente en los diccionarios
    float enemySpeed;   //referencia a la velocidad   
    int hitCount; //contador de impactos para llegar a la destrucción
    bool isMoving;
    bool isShooting;

    //creamos 3 diccionarios para la velocidad de cada enemigo, los impactos necesarios para destruirle, y la puntuación que se consigue al destruirlos
    readonly Dictionary<string, float> enemiesSpeed = new Dictionary<string, float>
    {
        {"Crab", 1f},
        {"Jumper", 3f},
        {"Octopus", 5f},
        {"Slime", 3f},
        {"Ghost", 2f},
        {"Spider", 5f},
        {"SwampThing", 2f}
    };
    readonly Dictionary<string, int> hitCounter = new Dictionary<string, int>
    {
        {"Crab", 7},
        {"Jumper", 5},
        {"Octopus", 3},
        {"Slime", 10},
        {"Ghost", 9},
        {"Spider", 6},
        {"SwampThing", 12}
    };
    readonly Dictionary<string, int> enemiesScore = new Dictionary<string, int>
    {
        {"Crab", 25},
        {"Jumper", 40},
        {"Octopus", 50},
        {"Slime", 25},
        {"Ghost", 30},
        {"Spider", 20},
        {"SwampThing", 15}
    };
    //en el start inicializamos las referencias necesarias e iniciamos la corrutina que 
    //verifica si el enemigo se queda atascado en un mismo sitio durante el tiempo establecido
    //momento en el que se destruye y se genera un nuevo enemigo en una zona aleatoria de spawn
    void Start()
    {
        if (GameManager.GetInstance().IsPlaying())
        {
            enemyType = gameObject.tag;
            anim = GetComponent<Animator>();
            playerTransform = GameObject.FindGameObjectWithTag("Player").transform;
            player = GameObject.FindGameObjectWithTag("Player");
            sfx = GetComponent<AudioSource>();
            spawner = GameObject.FindGameObjectWithTag("SpawningZones");
            enemySpeed = enemiesSpeed[enemyType];
            isShooting = false;
            StartCoroutine("CheckPosition");
        }
    }
    //en el método Update obtenemos en todo momento la referencia al jugador para orientar y dirigir a los enemigos hacia él.
    void Update()
    {
        isMoving = false;
        if (GameManager.GetInstance().IsPlaying())
        {           
            if (player != null && player.GetComponent<PlayerController>().IsActive()) //siempre que esté activo el enemigo le busca y sigue
            {
                Vector3 playerPosition = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
                transform.Translate((playerPosition - transform.position).normalized * enemySpeed * Time.deltaTime);
                isMoving = true; //si se produce desplazamiento cambia el booleano
                Orientate(); //método para que el sprite del enemigo "mire" hacia el jugador
                Animate(); //método para iniciar animaciones
                EnemyDestroy(); //método que se invoca para las destrucciones de enemigos
                if (!isShooting)
                {
                    StartCoroutine("Shoot"); //corrutina para disparos del enemigo
                }
            }
        }

    }
    //en el OnCollisionEnter verificamos si estamos encima del jugador, y, para no 
    //atascarse ahí, todos menos el pulpo pegan un pequeño salto aleatorio hacia un lado u otro
    void OnCollisionEnter2D(Collision2D other)
    {
        if (!gameObject.CompareTag("Octopus") && other.gameObject.CompareTag("Player"))
        {
            // Verificar si el enemigo está por encima del jugador
            float playerTop = other.collider.bounds.max.y; // Posición máxima en Y del jugador
            float enemyBottom = transform.position.y - GetComponent<Collider2D>().bounds.extents.y; // Posición mínima en Y del enemigo

            if (enemyBottom >= playerTop - CONTACT_OFFSET)
            {
                // El enemigo está encima del jugador, así que realiza un pequeño salto
                Jump();
            }
        }
        //por algún resquicio de los colliders se cuela algún enemigo, para 
        //que no se quede allí, se destruye en contacto con esas zonas de destrucción 
        else if (other.gameObject.CompareTag("DestroyZone"))
        {
            Destroy(gameObject);    //se destruye
            spawner.GetComponent<Spawner>().AddEnemy();  //se llama al método que provoca que se generen nuevos enemigos
        }

    }

    //método para hacer que los enemigos que se quedan "atascados" en una posición se autoeliminen
    private IEnumerator CheckPosition()
    {
        Vector2 initialPosition = transform.position; //se parte de la posición inicial del enemigo
        float elapsedTime = 0f; //se inicializa una float para temporizar, a cero
        const float POSITION_OFFSET = 0.1f; //pequeño offset por si hubiese algún desplazamiento mínimo

        while (true)  //bucle infinito para monitorizar
        {
            yield return new WaitForSeconds(1f); //se espera un pequeño margen de un segundo

            //si la posición actual sigue siendo muy cercana a la posición inicial,
            //incrementa el tiempo transcurrido. Si la posición ha cambiado, reinicia el tiempo.
            if (Vector2.Distance(transform.position, initialPosition) <= POSITION_OFFSET)
            {
                elapsedTime += 1f;
                if (elapsedTime >= JUMP_DELAY) //primero tratamos de saltar
                {
                    Jump();
                }

                //si el tiempo transcurrido supera el límite, destruye el objeto y sale del bucle
                if (elapsedTime >= DESTROY_DELAY)
                {
                    Destroy(gameObject);
                    Jump();
                    spawner.GetComponent<Spawner>().AddEnemy();
                    yield break;
                }
            }
            else
            {
                //reinicia el tiempo y actualiza la posición inicial
                initialPosition = transform.position;
                elapsedTime = 0f;
            }
        }
    }
    //método para que los enemigos salten, salvo el pulpo, si están justo encima del jugador
    void Jump()
    {
        float randomAngle = Random.Range(JUMP_ANGLE, -JUMP_ANGLE);
        //como aprendimos en el BreakOut, hay que convertir el ángulo a radianes
        float radians = Mathf.Deg2Rad * randomAngle;

        //calcular el vector de dirección del salto con el ángulo aleatorio y dirección ascendente
        Vector2 jumpDirection = new Vector2(Mathf.Sin(radians), Mathf.Cos(radians));

        //aplicar la fuerza de salto al enemigo en la dirección calculada
        GetComponent<Rigidbody2D>().velocity = jumpDirection.normalized * JUMP_FORCE;
    }
    //no tiene mayor complejidad, si se llega al límite de impactos (y se supera, ya que pueden darse varios simultáneamente),
    //se reproduce sonido, se invoca el prefab animado de destrucción asignado desde el editor,
    //se destruye al propio enemigo, se llama al método del spawner para añadir enemigo, y al cabo de un segundo se destruye
    //la animación de explosión de enemigo. Por último se añaden los puntos correspondientes al marcador.
    void EnemyDestroy()
    {
        if (hitCount >= hitCounter[gameObject.tag])
        {
            GameObject explosion = Instantiate(enemyExplosion, new Vector3(transform.position.x, transform.position.y, 0), Quaternion.identity);
            sfx.clip = enemyDestroy;
            sfx.Play();
            Destroy(explosion, 1f);
            Destroy(gameObject);
            spawner.GetComponent<Spawner>().AddEnemy();
            GameManager.GetInstance().AddScore(enemiesScore[tag]);
        }
    }
    //las animaciones cambian en función de la etiqueta del enemigo
    void Animate()
    {
        if (gameObject.tag == "Crab")
            anim.SetBool("isWalking", isMoving);
        if (gameObject.tag == "Jumper")
            anim.SetBool("isJumping", isMoving);
        //... faltarían las de los enemigos restantes
    }

    //al igual que en el método Update obtenemos referencia del jugador para que el enemigo "mire" hacia él
    void Orientate()
    {
        float playerDirection = playerTransform.position.x - gameObject.transform.position.x; //obtenemos el vector de orientación
        Vector3 enemyScale = gameObject.transform.localScale; //y la escala local del avatar del jugador
        if (playerDirection < 0f)
        {
            enemyScale.x = Mathf.Abs(enemyScale.x);
        }
        else
        {
            enemyScale.x = -Mathf.Abs(enemyScale.x);
        }
        gameObject.transform.localScale = enemyScale; //en función de hacia dónde esté el jugador, el sprite del enemigo mira hacia un lado o a otro
    }
    //el personaje enemigo que dispara lo hace con el prefab del disparo orientado en la dirección en la que mire
    IEnumerator Shoot()
    {
        isShooting = true;
        if (gameObject.tag == "Jumper" || gameObject.tag == "Ghost") //sólo disparan el Jumper y el Ghost
        {
            float vectorX = transform.localScale.x>0?1:-1;
            GameObject newShoot = Instantiate(shoot, new Vector3(transform.position.x + SHOOT_OFFSET * vectorX, transform.position.y, 0), Quaternion.identity);
            newShoot.GetComponent<EnemyShootController>().SetPlayerScale(transform.localScale.x * -1);
        }
        yield return new WaitForSeconds(3); //disparan cada 3 segundos
        isShooting = false;
    }

    public void IncreaseHitCount() //método público para que desde el ShootController se incremente el contador de impactos con cada uno producido
    {
        hitCount++;
    }

    //tenemos dos métodos porque necesitaba uno estático, o se complicaba la lógica de acceso. Hay que simplificar esto.
    public List<string> GetTags()
    {
        return new List<string>(enemiesSpeed.Keys);
    }

    public static List<string> GetPublicTags()
    {
        return new List<string> { "Crab", "Jumper", "Octopus", "Ghost", "Slime", "SwampThing", "Spider" };
    }
}
