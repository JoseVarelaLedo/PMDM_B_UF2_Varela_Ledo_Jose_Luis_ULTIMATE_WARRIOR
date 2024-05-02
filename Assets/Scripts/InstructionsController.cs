using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using System.Collections;
using System.IO;

public class InstructionsController : MonoBehaviour
{
    AudioSource sfx;     //referencia al propio Audiosource
    [SerializeField] AudioClip start; //el sonido que se producirá al iniciar
    [SerializeField] Text txtMessage; //referencia al mensaje que va a fluctuar con funciones Lerp
    [SerializeField] Text txtTimer; //texto del temporizador para iniciar el siguiente nivel
    const float DURATION = 4f;   //constante para la duración de la función Lerp
    const string DATA_FILE = "data.json"; //cada vez que se llega a esta pantalla, se genera un nuevo archivo de datos, de ahí que necesitemos la ruta
    float currentTime; //variable para el temporizador
    
    void Start()
    {
        currentTime =10; //inicializamos a 10, para que haya una cuenta regresiva desde ahí
        sfx = GetComponent<AudioSource>();
        if (SceneManager.GetActiveScene().buildIndex == 1 && File.Exists(DATA_FILE)) //como el controlador se comparte con más pantalals, sólo se va a crear nuevo archivo en la de instrucciones
        {
            File.Delete(DATA_FILE);
        }
        StartCoroutine ("ChangeColor");
        StartCoroutine ("StartTimer");
    }
    void Update() 
    { 
        if (Input.anyKeyDown) //se puede parar la cuenta atrás e iniciar el siguiente nivel pulsando cualquier tecla
        {                      
            StartCoroutine("StartNextLevel"); 
            
        } 
    } 
    IEnumerator StartNextLevel() //se reproduce sonido, se obtiene acceso al índice de la propia escena, y se carga la siguiente
    {        
        sfx.clip = start;
        sfx.Play();  
        int currentSceneIndex = SceneManager.GetActiveScene().buildIndex;
        SceneManager.LoadScene(currentSceneIndex + 1);
        yield return null; 
    } 

    IEnumerator ChangeColor() //corrutina para la fluctuación de color del mensaje
    {
        float t = 0; 
        while (t < DURATION) { 
            t += Time.deltaTime;            
            txtMessage.color = Color.Lerp(Color.red, Color.blue, t/DURATION);
            yield return null; 
        } StartCoroutine ("ChangeColor"); //reiniciar corrutina, haciendo un bucle
    }
    IEnumerator StartTimer() // temporizador para que la partida se inicie automáticamente en el tiempo indicado, si no se ha pulsado alguna tecla antes
    {
        while (currentTime > 0)
        {            
            currentTime -= 1f;       
            txtTimer.text = currentTime.ToString();
            yield return new WaitForSeconds(1f); // Esperar 1 segundo
        }
        
        StartCoroutine("StartNextLevel");       
    }
}
