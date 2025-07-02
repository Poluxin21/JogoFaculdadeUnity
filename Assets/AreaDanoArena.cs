using UnityEngine;
using GameObject = UnityEngine.GameObject;

public class AreaDanoArena : MonoBehaviour
{
    private float dano;
    private bool danoAtivo = false;
    private float tempoDano = 0.5f;
    private float proximoDano = 0f;
    
    public void AtivarDano(float danoPorSegundo)
    {
        dano = danoPorSegundo;
        danoAtivo = true;
        
        // Torna a área mais visível
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color cor = sr.color;
            cor.a = 0.7f;
            sr.color = cor;
        }
    }
    
    void Update()
    {
        if (danoAtivo && Time.time >= proximoDano)
        {
            proximoDano = Time.time + tempoDano;
        }
    }
    
    void OnTriggerStay2D(Collider2D other)
    {
        if (danoAtivo && other.CompareTag("Player") && Time.time >= proximoDano)
        {
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeDamage(dano);
                proximoDano = Time.time + tempoDano;
            }
        }
    }
}