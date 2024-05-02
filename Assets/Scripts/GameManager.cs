using System.Collections;
using UnityEngine.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
public class GameManager : MonoBehaviour
{
    const int LIVES = 3; //vidas iniciales
    const int MAX_LIVES = 4; //máximas vidas que puede haber
    const string DATA_FILE = "data.json"; //archivo donde se serializan los datos de la partida
    static GameManager instance;    //referencia a la propia instancia existente   
    [SerializeField] Text txtScore; //referencia al texto del canvas para la puntuación
    [SerializeField] Text txtHealth; //referencia al texto del canvas para la salud
    [SerializeField] Text txtTime; //referencia al texto del canvas para  el temporizador
    [SerializeField] Text txtMessage; //referencia al texto del canvas para los mensajes de pausa, gameover, paso de nivel, victoria,...
    [SerializeField] Text txtEnergy; //referencia al texto del canvas para los orbes de energía restantes
    [SerializeField] Image[] imgLives; //referencia al array de imágenes de vidas restantes
    [SerializeField] AudioClip gameOverFX; //audioclip gamOver
    [SerializeField] AudioClip pauseFX; //audioclip pausa
    [SerializeField] AudioClip deathFX; //audioclip muerte jugador
    [SerializeField] AudioClip levelUpFX; //audioclip paso de nivel
    [SerializeField] AudioClip extraLifeFX; //audioclip vida extra
    [SerializeField] AudioClip healthUpFX; //audioclip adquisición extra vida
    [SerializeField] AudioClip energyCollectFX; //audioclip adquisición orbe de energía
    [SerializeField] GameObject explosionPrefab; //prefab animado muerte de jugador
    [SerializeField] float initialTime; //tiempo inicial de la escena, asignado desde el editor
    [SerializeField] int energyNeeded; //cantidad de orbes de energía necesarios para pasar de nivel
    [SerializeField] GameObject energyPrefab; 
    AudioSource sfx; //referencia al propio AudioSource
    GameObject player; //referencia al avatar del jugador
    Collider2D playerCollider; //referencia al collider del jugador
    Rigidbody2D playerRb; //referencia al RigidBody del jugador
    GameData gameData; //referncia a la clase para serializar los datos en el archivo json
    int energyCount; //contador de orbes de energía adquiridos
    float currentTime; //tiempo actual
    int score; //puntuación
    int health; //salud
    int lives; //vidas actuales
    int currentSceneIndex; //referencia al índice buildsettings de la escena en curso
    bool paused; //booleano para indicar el estado de si estamos o no pausados   
    bool playing; //variable para indicar estado jugable o no 
   
    //acceso público a la instancia de juego
    public static GameManager GetInstance()
    {
        return instance;
    }
    //patrón Singleton
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(gameObject);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            return;
        }    

        gameData = LoadData();      //creamos objeto de tipo GameData para serializar

        score = gameData.score; //obtenemos los datos de puntuación, vidas y salud del archivo serializado
        lives = gameData.lives;
        health = gameData.health;
        currentTime = initialTime; //el tiempo se inicia con los valores de tiempo inicial
        
        txtScore.text = string.Format("{0,4:D4}", score);  //se "pintan" los textos del canvas de puntuación y salud
        txtHealth.text = string.Format("{0,2:D2}", health);
    }
    //en el método Start() se inicializa el temporizador en su corrutina, y se obtiene referencia al avatar del jugador y sus componentes necesarios
    //se obtiene el índice de la escena actual, y si no estamos en la primera pantalla jugable se inicia la corrutina para
    //comprobar si obtenemos las esferas de energía necesarias para pasar de fase
    void Start()
    {
        playing = true;
        StartCoroutine("StartTimer");
        player = GameObject.FindGameObjectWithTag("Player");
        sfx = GetComponent<AudioSource>();
        playerCollider = player.GetComponent<Collider2D>();
        playerRb = player.GetComponent<Rigidbody2D>();
        energyCount = 0;
        currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        if (currentSceneIndex > 2)
        {
            StartCoroutine ("CheckEnergy");
        }
    }
    GameData LoadData() //método para crear archivo de tipo GameData para serializar los datos de partida
    {
        if (File.Exists(DATA_FILE))
        {
            string fileText = File.ReadAllText(DATA_FILE);
            return JsonUtility.FromJson<GameData>(fileText);
        }
        else
        {
            return new GameData(0,LIVES,99);            
        }
    }       

    void SaveData() //método para grabar los datos en cada operación necesaria
    {
        gameData.score = score;
        gameData.lives = lives;
        gameData.health = health;
        string jsonData = JsonUtility.ToJson(gameData);
        File.WriteAllText(DATA_FILE, jsonData);
    }

    void Update()
    {        
        if (health <= 0) //se chequea si la salud ha llegado a cero, y se llama al método para perder vidas
        {
            LoseLife();
        }
        if (lives == 0) //si las vidas llegan a cero, se llama al método para fin de partida
        {
            GameOver();
        }
        if (score >0 && score % 5000 == 0 && lives < MAX_LIVES) //cada múltiplo de 5000 puntos da una vida, hasta un máximo de 4
        {
            lives++;
            sfx.clip = extraLifeFX;
            sfx.Play();
            SaveData(); //si aumentan las vidas se guarda dicho dato
        }

        if (Input.GetKeyDown(KeyCode.P)) //método para pausa
        {

            if (!paused)
            {
                Pause();
            }
            else
            {
                Resume();
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape)) //salir de la partida
        {
            Application.Quit();
        }
    }

    IEnumerator StartTimer() //método para el temporizador
    {
        while (currentTime > 0)
        {            
            currentTime -= 1f; //restar un segundo al temporizador            
            txtTime.text = FormatTime(currentTime); //actualizar el texto del temporizador
            yield return new WaitForSeconds(1f); //esperar 1 segundo
        }
        if (currentSceneIndex == 2) //la primera escena jugable sólo exige sobrevivir, así que si llegamos al final del tiempo, se pasa de fase
        {            
            StartCoroutine("NextLevel"); //temporizador llegó a cero, llamar al método NextLevel
        }else{
            LoseLife(); //si hemos llegado al final es porque no hemos recogido las esferas necesarias, de modo que se pierde vida
            Time.timeScale = 0.3f;
            txtMessage.text = "Time Over.";       
            yield return new WaitForSeconds(2);
            Time.timeScale = 1f;
            txtMessage.text = "";
            SaveData(); //se guardan los datos
            SceneManager.LoadScene(currentSceneIndex); //se vuelve a cargar la escena actual
        }        
    }

    //método para formatear el temporizador
    string FormatTime(float timeInSeconds)
    {
        int minutes = Mathf.FloorToInt(timeInSeconds / 60);
        int seconds = Mathf.FloorToInt(timeInSeconds % 60);
        return string.Format("{0:00}:{1:00}", minutes, seconds);
    }
    void LoseLife()
    {
        lives--; //una vida menos
        health = 99; //se vuelve a poner la vida a 99
        SaveData(); //se guardan esos datos
         
        Time.timeScale = 0.5f; //ralentizamos el tiempo
        txtMessage.text = "You Died";
        sfx.clip = deathFX; //sonido de "muerte" del jugador
        sfx.Play();
        StartCoroutine(PlayerDeathAnimation()); //se invoca el método para instanciar un prefab animado de "explosión" del jugador
        StartCoroutine(ResumeGameAfterDeath()); //se reinicia el juego
    }
    IEnumerator PlayerDeathAnimation()
    {
        player.GetComponent<SpriteRenderer>().enabled = false; //se oculta el sprite del jugador
        playerCollider.enabled = false; //se inhibe su collider (para que no siga perdiendo salud)
        playerRb.simulated = false; //su RigidBody también se inhibe para que no pueda desplazarse
        Vector3 playerPosition = new Vector3(player.transform.position.x, player.transform.position.y, 0); //se obtiene referencia de la posición antes de "morir"
        GameObject explosion = Instantiate(explosionPrefab, playerPosition, Quaternion.identity); //se instancia prefab de explosión en esa posición
        yield return new WaitForSeconds(1); //se espera el tiempo indicadp
        Destroy(explosion); //se destruye el prefab de explosión
        player.GetComponent<SpriteRenderer>().enabled = true; //se reactivan componentes del jugador
        playerCollider.enabled = true;
        playerRb.simulated = true;
    }

    IEnumerator ResumeGameAfterDeath()
    {
        yield return new WaitForSeconds(3f); //pasado un tiempo se devuelve la escala del tiempo a la normalidad
        Time.timeScale = 1f;
        player.SetActive(true);
        txtMessage.text = ""; //se borra mensaje
    }

    void Pause()
    {
        player.SetActive(false); //se paraliza por completo toda actividad del jugador, se paraliza el tiempo, y se muestra mensaje
        Time.timeScale = 0;
        txtMessage.text = "PAUSED. PRESS P TO RESTART";
        paused = true;
        sfx.clip = pauseFX;
        sfx.Play();
    }

    void Resume()
    {
        player.SetActive(true); //efecto contrario al método Pause()
        Time.timeScale = 1;
        txtMessage.text = "";
        paused = false;
        sfx.clip = pauseFX;
        sfx.Play();
    }

    void GameOver() //reproducción sonido, borrado de datos de partida, y carga de escena de GameOver
    {
        playing = false;
        Time.timeScale = 0f;
        sfx.clip = gameOverFX; //reproducción sonido
        sfx.Play();
        Stop(); //método que para la partida       
        Invoke("SceneGameOver", 2f);
    }
   
    void Stop()
    {
        player.SetActive(false);
        Time.timeScale = 0.2f;
        txtMessage.text = "GAME OVER";
        txtHealth.text = "";
        txtScore.text = "DEAD";
        paused = true;
    }

    void SceneGameOver()
    {        
        Time.timeScale = 1f;
        int sceneCount = SceneManager.sceneCountInBuildSettings; //obtener el número total de escenas en la configuración de buildsettings        
        int lastSceneIndex = sceneCount - 1; //acceder a la última escena utilizando su índice (el índice comienza desde 0)
        SceneManager.LoadScene(lastSceneIndex);  //cargar la última escena
    }

    public void AddScore(int points)
    {
        score += points;
        gameData.score = score;
        SaveData(); //tras cada operación de añadido de puntuación se guardan datos
    }

    public void ModifyHealth()
    {
        health--;
        SaveData(); //tras cada decremento de salud se guardan datos
    }

    public void AddHealth(int healthPoints)
    {
        sfx.clip = healthUpFX;
        sfx.Play();
        health +=healthPoints;
        SaveData(); //tras cada aumento de salud, se guaradn datos
    }

    public void DecreaseEnergy()
    {
        sfx.clip = energyCollectFX;
        sfx.Play();
        energyCount ++;
    }

    IEnumerator CheckEnergy() //corrutina para comprobar si se consiguen los orbes de energía necesarios para pasr de fasse
    {             
        while (true)
        {
            int energyRest = energyNeeded - energyCount;
            txtEnergy.text = energyRest.ToString();
            if (energyCount >= energyNeeded)
                {
                    StartCoroutine ("NextLevel"); //si se consiguen los orbes se pasa de nivel
                    yield break;
                }
            yield return null;
        }
    }    

    IEnumerator NextLevel()
    {
        sfx.clip = levelUpFX;
        sfx.Play();
        Time.timeScale = 0.3f;
        txtMessage.text = "You conquered this level.\n\n Go to next.";       
        yield return new WaitForSeconds(2);
        Time.timeScale = 1f;
        txtMessage.text = "";
        SaveData();
        SceneManager.LoadScene(currentSceneIndex + 1);
    }

    private void OnGUI()
    {
        for (int i = 0; i < imgLives.Length; i++)
        {
            imgLives[i].enabled = i < lives - 1;
        }
        txtScore.text = string.Format("{0,4:D4}", gameData.score);
        txtHealth.text = string.Format("{0,2:D2}", gameData.health);
    }
    public bool IsPlaying() //método para permitir acceso externo al estado de jugable o no
    {
        return playing;
    }
}