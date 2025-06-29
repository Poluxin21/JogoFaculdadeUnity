using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatilho_Alavancas : MonoBehaviour
{
    // Referência ao Animator da alavanca
    public Animator animator;

    // Tipo de evento associado à alavanca
    public TipoEvento eventoTipo;

    // Enumeração para tipos de evento
    public enum TipoEvento
    {
        Portão,
        Barreira
    }

    // Referência ao objeto que gerencia o evento
    public Gerenciamento_Eventos ativaEvento;

    // Variável para verificar se o jogador está em contato com a alavanca
    public bool jogadorContato = false;
    private bool nãoAtivaNovamente = false;

    // Variável para o nome do botão de interação
    public string botaoInteracao = "Attack"; // Valor padrão: "Attack"

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
        // Verifica se o jogador está em contato e pressionou o botão de interação
        if (jogadorContato && Input.GetButtonDown(botaoInteracao))
        {
            nãoAtivaNovamente = true;
            // Atualiza a animação da alavanca
            animator.SetBool("estado", true);

            // Notifica o sistema de eventos
            switch (eventoTipo)
            {
                case TipoEvento.Portão:
                    ativaEvento.AtualizaçãoAlavancasAtivadas();
                    ativaEvento.ComparacaoAlavancasAtivadas(4, false); // 4 alavancas necessárias para o portão
                    break;

                case TipoEvento.Barreira:
                    ativaEvento.AtualizaçãoAlavancasAtivadas();
                    ativaEvento.ComparacaoAlavancasAtivadas(2, true); // 2 alavancas necessárias para a barreira
                    break;
            }
        }
    }

    private void Update()
    {
        if(nãoAtivaNovamente == true)
        {
            botao(); // Chama o método botao() a cada frame
        }
    }
}