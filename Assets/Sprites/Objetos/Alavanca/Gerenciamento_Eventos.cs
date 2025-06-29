using UnityEngine;
using UnityEngine.SceneManagement;

public class Gerenciamento_Eventos : MonoBehaviour
{
    // Vari�veis p�blicas configur�veis no Inspector
    [Header("Configura��es Gerais")]
    public int alavancasAtivadas = 0; // Contador de alavancas ativadas
    public bool eventoParaBarreira = false; // Indica se o evento � para barreiras (true) ou port�o (false)
    public Animator animatorPort�o; // Refer�ncia ao Animator do port�o (s� usado para port�es)

    [Header("Estado da Porta")]
    public bool portaAberta = false; // Estado da porta (aberta ou fechada)

    [Header("Bot�o de Transi��o")]
    public GameObject botaoTransicao; // Objeto do bot�o que aparece/some

    private void Start()
    {
        // Inicializa o bot�o de transi��o como desativado
        if (botaoTransicao != null)
        {
            botaoTransicao.SetActive(false);
        }
    }

    
    /// Atualiza o contador de alavancas ativadas.
    public void Atualiza��oAlavancasAtivadas()
    {
        alavancasAtivadas++; // Incrementa o contador de alavancas ativadas
    }

    
    /// Compara o n�mero de alavancas ativadas com a condi��o necess�ria.
    public void ComparacaoAlavancasAtivadas(int condicao, bool eventoBarreira)
    {
        // Define se o evento � para barreiras
        eventoParaBarreira = eventoBarreira;

        // Verifica se o n�mero de alavancas ativadas atende � condi��o
        if (alavancasAtivadas >= condicao)
        {
            if (!eventoParaBarreira)
            {
                // Caso seja um port�o, abre o port�o
                portaAberta = true;
                animatorPort�o.SetBool("estado", portaAberta); // Ativa a anima��o do port�o
            }
            else
            {
                // Caso seja uma barreira, destr�i a barreira
                Destroy(gameObject); // Destroi o objeto da barreira
            }
        }
    }

    /// Detecta quando o jogador entra na �rea da porta.
    private void OnTriggerEnter2D(Collider2D outro)
    {
        // Verifica se o jogador colidiu com a porta e ela est� aberta
        if (outro.CompareTag("Player") && portaAberta)
        {
            // Faz o bot�o de transi��o aparecer
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(true); // Ativa o bot�o
            }
        }
    }

    /// Detecta quando o jogador sai da �rea da porta.
    private void OnTriggerExit2D(Collider2D outro)
    {
        // Verifica se o jogador saiu da porta e ela est� aberta
        if (outro.CompareTag("Player") && portaAberta)
        {
            // Faz o bot�o de transi��o desaparecer
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(false); // Desativa o bot�o
            }
        }
    }

    /// Carrega a pr�xima cena quando o bot�o de transi��o for acionado.
    public void CarregarProximaCena(string _mapa2)
    {
        // Verifica se o nome da cena foi fornecido corretamente
        if (!string.IsNullOrEmpty(_mapa2))
        {
            // Carrega a cena especificada
            SceneManager.LoadScene(_mapa2);
        }
    }
}