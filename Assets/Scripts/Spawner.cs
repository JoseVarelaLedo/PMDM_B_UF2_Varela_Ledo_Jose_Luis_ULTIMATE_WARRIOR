using System.Collections;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.SceneManagement;

public class Spawner : MonoBehaviour{
    [SerializeField] float delaySpawning; //retraso antes de que se produzcan nacimientos de objetos
    [SerializeField] float intervalEnemies; //intervalo entre "nacimientos" de enemigos    
    [SerializeField] float intervalHealthUp; //ídem salud
    [SerializeField] float intervalEnergySpawn; //ídem energía
    [SerializeField] GameObject[] enemyPrefabs; //referencia al array de prefabs posibles de enemigos para cada escena
    [SerializeField] Transform[] spawnPoints; //referencia al array de puntos de "nacimiento"
    [SerializeField] GameObject healthUpPrefab; //prefab del powerup de salud
    [SerializeField] GameObject energyPrefab; //prefab del powerup de energía
    Transform player; //referencia al transform del jugador
    const int MAX_ENEMIES = 20; //máximo número de enemigos permitidos en el escenario; se limita a 20 por cuestiones de rendimiento
    int currentEnemies = 0; //número actual de enemigos en el escenario
    void Start()
    {
        //buscar el jugador automáticamente en tiempo de ejecución (hay que marcarle con la etiqueta "Player")
        player = GameObject.FindGameObjectWithTag("Player").transform; //acceso al transform del avatar del jugador
        StartCoroutine("SpawnRandomEnemies");  //corrutina para crear enemigos en puntos aleatorios
        StartCoroutine("SpawnHealth"); //ídem powerups de salud
        if (SceneManager.GetActiveScene().buildIndex >2) //como el scrip se comparte para varias escenas, y en la primera jugable no usamos powerups energía, sólo se inicia esta corrutina en pantallas diferentes de la primera jugable
        {
            StartCoroutine ("SpawnEnergy");
        }
    }    
    //método para añadir enemigos cada vez que se destruyan disminuyendo el contador de los que existen en el escenario
    public void AddEnemy(){
        currentEnemies--;
    }

    IEnumerator SpawnRandomEnemies()
    {
        yield return new WaitForSeconds(delaySpawning); //se espera un pequeño tiempo antes de empezar a generar enemigos
        //sólo instanciar un enemigo si no hemos alcanzado el límite
        while (currentEnemies < MAX_ENEMIES)
        {               
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length); //índice aleatorio para elegir un punto de "nacimiento"
            Transform spawnPoint = spawnPoints[randomSpawnIndex];

            //eligir un prefab de enemigo aleatorio
            GameObject enemyPrefab = enemyPrefabs[Random.Range(0, enemyPrefabs.Length)];

            //instanciar al enemigo en el punto de spawn actual
            GameObject enemyInstance = Instantiate(enemyPrefab, spawnPoint.position, Quaternion.identity);
            //determinar la dirección en la que está mirando el jugador
            float playerDirection = player.position.x - enemyInstance.transform.position.x;

            //modificar la orientación del sprite del enemigo según la dirección del jugador
            Vector3 enemyScale = enemyInstance.transform.localScale;
            if (playerDirection < 0f) 
            {
                enemyScale.x = Mathf.Abs(enemyScale.x); 
            }
            else
            {
                enemyScale.x = -Mathf.Abs(enemyScale.x); 
            }
            enemyInstance.transform.localScale = enemyScale;

            //incrementar el contador de enemigos
            currentEnemies++;
            yield return new WaitForSeconds(intervalEnemies);   //tras el intervalo indicado se vuelve a iniciar el bucle         
        }
        if (currentEnemies >= MAX_ENEMIES) //esta condición se puso porque a veces se destruían varios simultáneamente, y no se llegaba a cumplir la condición de ==, y la corrutina se paraba
        {
            StartCoroutine("SpawnRandomEnemies");
        }
    }
    IEnumerator SpawnHealth() //funcionamiento análogo al spawn de enemigos, pero como sólo hay un prefab, el único componente aleatorio es la posición de nacimiento.
    {
        yield return new WaitForSeconds(delaySpawning);
        int randomSpawnIndex = Random.Range(0, spawnPoints.Length);
        Transform spawnPoint = spawnPoints[randomSpawnIndex];
        GameObject health = Instantiate (healthUpPrefab, spawnPoint.position, Quaternion.identity);
        yield return new WaitForSeconds(intervalHealthUp);  
        Destroy (health);
        StartCoroutine("SpawnHealth"); //reinicio de la corrutina
    }
    IEnumerator SpawnEnergy() //funcionamiento análogo al spawn de enemigos, pero como sólo hay un prefab, el único componente aleatorio es la posición de nacimiento.
    {
        yield return new WaitForSeconds(delaySpawning);        
        GameObject [] energyOrbs = new GameObject[3];                                               //instanciamos 3 objetos simultáneos en diferentes puntos
        for (int i = 0 ; i< energyOrbs.Length; i++)                                                 //porque de lo contrario la dificultar   
        {                                               
            int randomSpawnIndex = Random.Range(0, spawnPoints.Length);                             //para encontrarlos a tiempo era muy alta
            Transform spawnPoint = spawnPoints[randomSpawnIndex];
            energyOrbs[i] = Instantiate (energyPrefab, spawnPoint.position, Quaternion.identity);
        }       
        yield return new WaitForSeconds(intervalEnergySpawn);  
        foreach (GameObject energyOrb in energyOrbs)
        {
            Destroy (energyOrb);
        }        
        StartCoroutine("SpawnEnergy"); //reinicio de la corrutina
    }
}
