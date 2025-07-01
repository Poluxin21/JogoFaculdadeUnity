using UnityEngine;

public class Gatilho_Alavancas : MonoBehaviour
{
    [Header("Refer�ncias da Alavanca")]
    public Animator animator; // Refer�ncia ao Animator da alavanca
    public MonoBehaviour gerenciamentoEvento; // Vari�vel gen�rica para referenciar Gerenciamento_Port�o ou Gerenciamento_Barreira

    private bool jogadorContato = false; // Vari�vel para verificar se o jogador est� em contato com a alavanca
    private bool n�oAtivaNovamente = false; // Impede que a alavanca seja ativada novamente ap�s sua primeira ativa��o

    public string botaoInteracao = "Attack"; // Vari�vel para o nome do bot�o de intera��o (valor padr�o: "Attack")

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
        if (!n�oAtivaNovamente && jogadorContato && Input.GetButtonDown(botaoInteracao))
        {
            n�oAtivaNovamente = true; // Impede reativa��o

            // Atualiza a anima��o da alavanca
            animator.SetBool("estado", true);

            // Verifica qual tipo de evento est� sendo gerenciado
            if (gerenciamentoEvento != null)
            {
                // Chama os m�todos comuns entre Gerenciamento_Port�o e Gerenciamento_Barreira
                gerenciamentoEvento.SendMessage("Atualiza��oAlavancasAtivadas");
                gerenciamentoEvento.SendMessage("ComparacaoAlavancasAtivadas");
            }
        }
    }
}