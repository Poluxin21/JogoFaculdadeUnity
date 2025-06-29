using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatilho_Alavancas : MonoBehaviour
{
    [Header("Refer�ncias da Alavanca")]
    public Animator animator; // Refer�ncia ao Animator da alavanca

    [Header("Configura��es de Evento")]
    public TipoEvento eventoTipo; // Tipo de evento associado � alavanca
    public Gerenciamento_Eventos ativaEvento; // Refer�ncia ao objeto que gerencia o evento

    [Header("Audios das alavancas")]
    // Refer�ncia ao Fonte de �udio que tocar� o audio
    public AudioSource fonteAudio;

    public enum TipoEvento
    {
        Port�o,
        Barreira
    }

    [Header("Controle de Estado do Jogador")]
    private bool jogadorContato = false; // Vari�vel para verificar se o jogador est� em contato com a alavanca
    private bool n�oAtivaNovamente = false; // Impede que a alavanca seja ativada novamente ap�s sua primeira ativa��o

    [Header("Configura��es de Intera��o")]
    public string botaoInteracao = "Attack"; // Vari�vel para o nome do bot�o de intera��o (valor padr�o: "Attack")

    private void OnTriggerEnter2D(Collider2D outro)
    {
        // Verifica se o jogador colidiu com a alavanca
        if (outro.CompareTag("Player"))
        {
            jogadorContato = true; // Define como true ao entrar no gatilho
        }
    }

    private void OnTriggerExit2D(Collider2D outro)
    {
        // Verifica se o jogador saiu do gatilho da alavanca
        if (outro.CompareTag("Player"))
        {
            jogadorContato = false; // Define como false ao sair do gatilho
        }
    }

    private void botao()
    {
        // Verifica se o jogador est� em contato e pressionou o bot�o de intera��o
        if (jogadorContato && Input.GetButtonDown(botaoInteracao))
        {
            n�oAtivaNovamente = true; // Impede que a alavanca seja ativada novamente
            // Atualiza a anima��o da alavanca
            fonteAudio.Play();
            animator.SetBool("estado", true);  


            // Notifica o sistema de eventos
            switch (eventoTipo)
            {
                case TipoEvento.Port�o:
                    ativaEvento.Atualiza��oAlavancasAtivadas();
                    ativaEvento.ComparacaoAlavancasAtivadas(4, false); // 4 alavancas necess�rias para o port�o
                    break;

                case TipoEvento.Barreira:
                    ativaEvento.Atualiza��oAlavancasAtivadas();
                    ativaEvento.ComparacaoAlavancasAtivadas(2, true); // 2 alavancas necess�rias para a barreira
                    break;
            }
        }
    }

    private void Update()
    {
        // Chama o m�todo botao() apenas se a alavanca ainda n�o tiver sido ativada
        if (!n�oAtivaNovamente)
        {
            botao();
        }
    }
}