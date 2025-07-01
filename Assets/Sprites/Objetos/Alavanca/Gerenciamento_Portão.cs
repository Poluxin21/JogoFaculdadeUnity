using UnityEngine;

public class Gerenciamento_Portão : MonoBehaviour
{
    [Header("Configurações Gerais")]
    public int alavancasAtivadas = 0; // Contador de alavancas ativadas
    public int condicao = 4; // Número mínimo de alavancas necessárias (configurável no Inspector)
    public Animator animatorPortão; // Referência ao Animator do portão

    [Header("Botão de Transição")]
    public GameObject botaoTransicao; // Objeto do botão que aparece/some
    public bool portaAberta = false; // Estado da porta (aberta ou fechada)

    private void Start()
    {
        if (botaoTransicao != null)
        {
            botaoTransicao.SetActive(false); // Desativa o botão inicialmente
        }
    }

    public void AtualizaçãoAlavancasAtivadas()
    {
        alavancasAtivadas++; // Incrementa o contador de alavancas ativadas
    }

    public void ComparacaoAlavancasAtivadas()
    {
        if (alavancasAtivadas >= condicao)
        {
            portaAberta = true;
            animatorPortão.SetBool("estado", portaAberta); // Ativa a animação do portão
            Debug.Log("Portão aberto! Permite acesso ao próximo mapa.");
        }
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") && portaAberta)
        {
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(true); // Ativa o botão
            }
            Debug.Log("Está na porta.");
        }
    }

    private void OnTriggerExit2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") && portaAberta)
        {
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(false); // Desativa o botão
            }
            Debug.Log("Não está na porta.");
        }
    }
}