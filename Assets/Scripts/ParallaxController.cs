using UnityEngine;

//se asigna este controlador a cada capa del fondo para, usando diferentes velocidades en cada capa, simular el paralaje
public class ParallaxController : MonoBehaviour
{
    [SerializeField] float parallax;
    Material mat;
    Transform cam;
    Vector3 initialPos;

    void Start()
    {
        mat = GetComponent<SpriteRenderer>().material;
        cam = Camera.main.transform;
        initialPos = transform.position;
    }
    
    void Update()
    {
        transform.position = new Vector3 (cam.position.x, initialPos.y, initialPos.z);
        mat.mainTextureOffset = new Vector2 (cam.position.x * parallax, 0);
    }
}
