using UnityEngine;

public class Gerenciamento_Eventos : MonoBehaviour
{
    // Contador de alavancas ativadas
    public int alavancasAtivadas = 0;

    // Indica se o evento � para barreiras (true) ou port�o (false)
    public bool eventoParaBarreira = false;

    // Refer�ncia ao Animator do port�o (s� usado para port�es)
    //public Animator animatorPort�o;

    // Fun��o chamada pelas alavancas para atualizar o contador
    public void Atualiza��oAlavancasAtivadas()
    {
        alavancasAtivadas++;
    }

    // Fun��o para comparar o n�mero de alavancas ativadas com a condi��o
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
                //animatorPort�o.SetBool("estado", true);
                Debug.Log("Port�o aberto! Permite acesso ao pr�ximo mapa.");
            }
            else
            {
                // Caso seja uma barreira, destr�i a barreira
                Destroy(gameObject);
                Debug.Log("Barreira destru�da!");
            }
        }
    }
}