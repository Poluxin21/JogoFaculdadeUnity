using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatilho_Alavancas : MonoBehaviour
{
    // Refer�ncia ao Animator da alavanca
    public Animator animator;

    // Tipo de evento associado � alavanca
    public TipoEvento eventoTipo;

    // Enumera��o para tipos de evento
    public enum TipoEvento
    {
        Port�o,
        Barreira
    }

    // Refer�ncia ao objeto que gerencia o evento
    public Gerenciamento_Eventos ativaEvento;

    // Vari�vel para verificar se o jogador est� em contato com a alavanca
    public bool jogadorContato = false;
    private bool n�oAtivaNovamente = false;

    // Vari�vel para o nome do bot�o de intera��o
    public string botaoInteracao = "Attack"; // Valor padr�o: "Attack"

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
            n�oAtivaNovamente = true;
            // Atualiza a anima��o da alavanca
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
        if(n�oAtivaNovamente == true)
        {
            botao(); // Chama o m�todo botao() a cada frame
        }
    }
}