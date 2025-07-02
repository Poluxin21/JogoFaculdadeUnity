using UnityEngine;

public class ProjetilCavaleiro : MonoBehaviour
{
    private Vector2 direcao;
    private float velocidade;
    private float dano;
    private float tempoVida = 5f;
    private bool estatico = false;
    
    public void Inicializar(Vector2 dir, float vel, float dmg)
    {
        direcao = dir;
        velocidade = vel;
        dano = dmg;
        
        // Se a direção for zero, é um projétil estático (tremor)
        if (direcao == Vector2.zero)
        {
            estatico = true;
            tempoVida = 2f;
        }
        
        if (!estatico)
        {
            float angulo = Mathf.Atan2(direcao.y, direcao.x) * Mathf.Rad2Deg;
            transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
        }
        
        Destroy(gameObject, tempoVida);
    }
    
    void Update()
    {
        if (!estatico)
        {
            transform.Translate(direcao * velocidade * Time.deltaTime, Space.World);
        }
    }
    
    void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag("Player"))
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(dano);
            }
            
            if (!estatico)
            {
                Destroy(gameObject);
            }
        }
        else if (other.CompareTag("Parede") || other.CompareTag("Obstaculo"))
        {
            if (!estatico)
            {
                Destroy(gameObject);
            }
        }
    }
}