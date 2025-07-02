using UnityEngine;
using System.Collections.Generic;

public class HitboxBoss : MonoBehaviour
{
    private float dano = 1f;
    private HashSet<GameObject> jogadoresAtingidos = new HashSet<GameObject>();

    public void SetDano(float novoDano)
    {
        dano = novoDano;
    }

    void OnEnable()
    {
        // Limpa a lista quando a hitbox é ativada
        jogadoresAtingidos.Clear();
        Debug.Log($"Hitbox Boss ativada com dano: {dano}");
    }

    void OnDisable()
    {
        // Limpa a lista quando a hitbox é desativada
        jogadoresAtingidos.Clear();
        Debug.Log("Hitbox Boss desativada");
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        Debug.Log($"Hitbox colidiu com: {other.name}, Tag: {other.tag}");
        
        if (other.CompareTag("Player"))
        {
            // Verifica se já atingiu este jogador neste ataque
            if (jogadoresAtingidos.Contains(other.gameObject))
            {
                Debug.Log("Jogador já foi atingido neste ataque");
                return;
            }

            // Tenta pegar o PlayerController diretamente ou no pai
            PlayerController playerController = other.GetComponent<PlayerController>();
            if (playerController == null)
            {
                playerController = other.GetComponentInParent<PlayerController>();
            }
            if (playerController == null)
            {
                playerController = other.GetComponentInChildren<PlayerController>();
            }

            if (playerController != null)
            {
                Debug.Log($"PlayerController encontrado! Estado invencível: {playerController.pState?.invincible}");
                
                if (playerController.pState == null)
                {
                    Debug.LogWarning("pState é null no PlayerController!");
                    return;
                }

                if (!playerController.pState.invincible)
                {
                    Debug.Log($"Aplicando dano de {dano} ao jogador");
                    playerController.TakeDamage(dano);
                    
                    jogadoresAtingidos.Add(other.gameObject);
                }
                else
                {
                    Debug.Log("Jogador está invencível");
                }
            }
            else
            {
                Debug.LogWarning("PlayerController não encontrado no jogador!");
            }
        }
        else
        {
            Debug.Log($"Objeto colidiu mas não tem tag Player: {other.tag}");
        }
    }

    public void ResetarJogadoresAtingidos()
    {
        jogadoresAtingidos.Clear();
    }
}