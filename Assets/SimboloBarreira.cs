using UnityEngine;

public class SimboloBarreira : MonoBehaviour
{
    [Header("Configurações")]
    public float vida = 1f; // Quantos hits para destruir
    public ParticleSystem efeitoDestruicao; // Efeito visual opcional

    private RainhaNyarla boss;
    private float vidaAtual;
    private bool jaFoiDestruido = false;

    void Start()
    {
        // Busca o boss na cena
        boss = FindObjectOfType<RainhaNyarla>();
        if (boss == null)
        {
            Debug.LogError("Boss RainhaNyarla não encontrado na cena!");
        }

        vidaAtual = vida;
    }

    // Este método será chamado quando o player atacar o símbolo
    public void ReceberDano(float dano)
    {
        if (jaFoiDestruido) return;

        vidaAtual -= dano;

        Debug.Log($"Símbolo recebeu {dano} de dano. Vida restante: {vidaAtual}");

        // Efeito visual de hit (opcional)
        // Aqui você pode adicionar um flash, shake, etc.

        if (vidaAtual <= 0)
        {
            DestruirSimbolo();
        }
    }

    void DestruirSimbolo()
    {
        if (jaFoiDestruido) return;
        jaFoiDestruido = true;

        // Efeito de destruição
        if (efeitoDestruicao != null)
        {
            Instantiate(efeitoDestruicao, transform.position, transform.rotation);
        }

        // Notifica o boss
        if (boss != null)
        {
            boss.DestruirSimbolo();
        }

        Debug.Log("Símbolo destruído!");

        // Desativa o símbolo ao invés de destruir para manter controle
        gameObject.SetActive(false);
    }

    // Para detectar colisão com ataques do player
    void OnTriggerEnter2D(Collider2D other)
    {
        if (jaFoiDestruido) return;

        // Verifica se foi atacado pelo player
        if (other.CompareTag("Player"))
        {
            // Verifica se o player está atacando
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Como o player não tem uma variável pública para isAttacking,
                // vamos assumir que se colidiu aqui, foi um ataque
                ReceberDano(1f);
            }
        }
    }

    // Método para reativar o símbolo quando uma nova barreira for criada
    public void ReativarSimbolo()
    {
        jaFoiDestruido = false;
        vidaAtual = vida;
        gameObject.SetActive(true);
    }
}