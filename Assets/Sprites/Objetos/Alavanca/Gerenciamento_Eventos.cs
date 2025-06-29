using UnityEngine;

public class Gerenciamento_Eventos : MonoBehaviour
{
    // Contador de alavancas ativadas
    public int alavancasAtivadas = 0;

    // Indica se o evento é para barreiras (true) ou portão (false)
    public bool eventoParaBarreira = false;

    // Referência ao Animator do portão (só usado para portões)
    //public Animator animatorPortão;

    // Função chamada pelas alavancas para atualizar o contador
    public void AtualizaçãoAlavancasAtivadas()
    {
        alavancasAtivadas++;
    }

    // Função para comparar o número de alavancas ativadas com a condição
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
                //animatorPortão.SetBool("estado", true);
                Debug.Log("Portão aberto! Permite acesso ao próximo mapa.");
            }
            else
            {
                // Caso seja uma barreira, destrói a barreira
                Destroy(gameObject);
                Debug.Log("Barreira destruída!");
            }
        }
    }
}