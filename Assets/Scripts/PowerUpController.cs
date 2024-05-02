using UnityEngine;


//script para llamar a los métodos adecuados en función del powerup que se colecciona
public class PowerUpController : MonoBehaviour
{    
    const int HEALTH_TO_ADD = 25;
    void OnTriggerEnter2D(Collider2D other) {          
        if (other.gameObject.tag == "Player") //evidentemente sólo el jugador puede coleccionarlos
        {
            if (gameObject.CompareTag("Heart")){
                GameManager.GetInstance().AddHealth(HEALTH_TO_ADD); //si es un corazón se incrementa la salud
                Destroy (gameObject); //se destruye al obtenerlo
            }
            else if (gameObject.CompareTag("Energy"))
            {                
                GameManager.GetInstance().DecreaseEnergy(); //si es un orbe de energía se decrementa la cuenta de los necesarios para pasar de nivel
                Destroy (gameObject); //se destruye al obtenerlo
            }           
        }
    }
}
