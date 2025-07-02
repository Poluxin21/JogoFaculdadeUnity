using UnityEngine;

public class SimboloBarreira : MonoBehaviour
{
    [Header("Configura��es")]
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
            Debug.LogError("Boss RainhaNyarla n�o encontrado na cena!");
        }

        vidaAtual = vida;
    }

    // Este m�todo ser� chamado quando o player atacar o s�mbolo
    public void ReceberDano(float dano)
    {
        if (jaFoiDestruido) return;

        vidaAtual -= dano;

        Debug.Log($"S�mbolo recebeu {dano} de dano. Vida restante: {vidaAtual}");

        // Efeito visual de hit (opcional)
        // Aqui voc� pode adicionar um flash, shake, etc.

        if (vidaAtual <= 0)
        {
            DestruirSimbolo();
        }
    }

    void DestruirSimbolo()
    {
        if (jaFoiDestruido) return;
        jaFoiDestruido = true;

        // Efeito de destrui��o
        if (efeitoDestruicao != null)
        {
            Instantiate(efeitoDestruicao, transform.position, transform.rotation);
        }

        // Notifica o boss
        if (boss != null)
        {
            boss.DestruirSimbolo();
        }

        Debug.Log("S�mbolo destru�do!");

        // Desativa o s�mbolo ao inv�s de destruir para manter controle
        gameObject.SetActive(false);
    }

    // Para detectar colis�o com ataques do player
    void OnTriggerEnter2D(Collider2D other)
    {
        if (jaFoiDestruido) return;

        // Verifica se foi atacado pelo player
        if (other.CompareTag("Player"))
        {
            // Verifica se o player est� atacando
            PlayerController player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                // Como o player n�o tem uma vari�vel p�blica para isAttacking,
                // vamos assumir que se colidiu aqui, foi um ataque
                ReceberDano(1f);
            }
        }
    }

    // M�todo para reativar o s�mbolo quando uma nova barreira for criada
    public void ReativarSimbolo()
    {
        jaFoiDestruido = false;
        vidaAtual = vida;
        gameObject.SetActive(true);
    }
}