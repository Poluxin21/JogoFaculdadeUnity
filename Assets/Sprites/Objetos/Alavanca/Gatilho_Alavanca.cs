using UnityEngine;

public class Gatilho_Alavancas : MonoBehaviour
{
    [Header("Referências da Alavanca")]
    public Animator animator; // Referência ao Animator da alavanca
    public MonoBehaviour gerenciamentoEvento; // Variável genérica para referenciar Gerenciamento_Portão ou Gerenciamento_Barreira

    private bool jogadorContato = false; // Variável para verificar se o jogador está em contato com a alavanca
    private bool nãoAtivaNovamente = false; // Impede que a alavanca seja ativada novamente após sua primeira ativação

    public string botaoInteracao = "Attack"; // Variável para o nome do botão de interação (valor padrão: "Attack")

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            jogadorContato = true; // Define como true ao entrar no gatilho
        }
    }

    private void OnTriggerExit2D(Collider2D outro)
    {
        if (outro.CompareTag("Player"))
        {
            jogadorContato = false; // Define como false ao sair do gatilho
        }
    }

    private void Update()
    {
        if (!nãoAtivaNovamente && jogadorContato && Input.GetButtonDown(botaoInteracao))
        {
            nãoAtivaNovamente = true; // Impede reativação

            // Atualiza a animação da alavanca
            animator.SetBool("estado", true);

            // Verifica qual tipo de evento está sendo gerenciado
            if (gerenciamentoEvento != null)
            {
                // Chama os métodos comuns entre Gerenciamento_Portão e Gerenciamento_Barreira
                gerenciamentoEvento.SendMessage("AtualizaçãoAlavancasAtivadas");
                gerenciamentoEvento.SendMessage("ComparacaoAlavancasAtivadas");
            }
        }
    }
}