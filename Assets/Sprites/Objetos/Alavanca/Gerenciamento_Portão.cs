using UnityEngine;

public class Gerenciamento_Port�o : MonoBehaviour
{
    [Header("Configura��es Gerais")]
    public int alavancasAtivadas = 0; // Contador de alavancas ativadas
    public int condicao = 4; // N�mero m�nimo de alavancas necess�rias (configur�vel no Inspector)
    public Animator animatorPort�o; // Refer�ncia ao Animator do port�o

    [Header("Bot�o de Transi��o")]
    public GameObject botaoTransicao; // Objeto do bot�o que aparece/some
    public bool portaAberta = false; // Estado da porta (aberta ou fechada)

    private void Start()
    {
        if (botaoTransicao != null)
        {
            botaoTransicao.SetActive(false); // Desativa o bot�o inicialmente
        }
    }

    public void Atualiza��oAlavancasAtivadas()
    {
        alavancasAtivadas++; // Incrementa o contador de alavancas ativadas
    }

    public void ComparacaoAlavancasAtivadas()
    {
        if (alavancasAtivadas >= condicao)
        {
            portaAberta = true;
            animatorPort�o.SetBool("estado", portaAberta); // Ativa a anima��o do port�o
            Debug.Log("Port�o aberto! Permite acesso ao pr�ximo mapa.");
        }
    }

    private void OnTriggerEnter2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") && portaAberta)
        {
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(true); // Ativa o bot�o
            }
            Debug.Log("Est� na porta.");
        }
    }

    private void OnTriggerExit2D(Collider2D outro)
    {
        if (outro.CompareTag("Player") && portaAberta)
        {
            if (botaoTransicao != null)
            {
                botaoTransicao.SetActive(false); // Desativa o bot�o
            }
            Debug.Log("N�o est� na porta.");
        }
    }
}