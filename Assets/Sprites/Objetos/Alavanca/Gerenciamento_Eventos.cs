using UnityEngine;
using UnityEngine.SceneManagement;

public class Gerenciamento_Eventos : MonoBehaviour
{
    // Variáveis públicas configuráveis no Inspector
    [Header("Configurações Gerais")]
    public int alavancasAtivadas = 0; // Contador de alavancas ativadas
    public bool eventoParaBarreira = false; // Indica se o evento é para barreiras (true) ou portão (false)
    public Animator animatorPortão; // Referência ao Animator do portão (só usado para portões)

    [Header("Estado da Porta")]
    public bool portaAberta = false; // Estado da porta (aberta ou fechada)

    [Header("Botão de Transição")]
    public GameObject botaoTransicao; // Objeto do botão que aparece/some

    private void Start()
    {
        // Inicializa o botão de transição como desativado
        if (botaoTransicao != null)
        {
            botaoTransicao.SetActive(false);
        }
    }

    
    /// Atualiza o contador de alavancas ativadas.
    public void AtualizaçãoAlavancasAtivadas()
    {
        alavancasAtivadas++; // Incrementa o contador de alavancas ativadas
    }

    
    /// Compara o número de alavancas ativadas com a condição necessária.
    public void ComparacaoAlavancasAtivadas(int condicao, bool eventoBarreira)
    {
        // Define se o evento é para barreiras
        eventoParaBarreira = eventoBarreira;

        // Verifica se o número de alavancas ativadas atende à condição
        if (alavancasAtivadas >= condicao)
        {
            if (!eventoParaBarreira)
            {
                // Caso seja um portão, abre o portão
                portaAberta = true;
                animatorPortão.SetBool("estado", portaAberta); // Ativa a animação do portão
            }
            else
            {
                // Caso seja uma barreira, destrói a barreira
                Destroy(gameObject); // Destroi o objeto da barreira
            }
        }
    }

    /// Detecta quando o jogador entra na área da porta.
    private void OnTriggerEnter2D(Collider2D outro)
    {
        // Verifica se o jogador colidiu com a porta e ela está aberta
        if (outro.CompareTag("Player") && portaAberta)
        {
            // Faz o botão de transição aparecer
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(true); // Ativa o botão
            }
        }
    }

    /// Detecta quando o jogador sai da área da porta.
    private void OnTriggerExit2D(Collider2D outro)
    {
        // Verifica se o jogador saiu da porta e ela está aberta
        if (outro.CompareTag("Player") && portaAberta)
        {
            // Faz o botão de transição desaparecer
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(false); // Desativa o botão
            }
        }
    }

    /// Carrega a próxima cena quando o botão de transição for acionado.
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