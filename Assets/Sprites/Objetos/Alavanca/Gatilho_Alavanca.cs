using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Gatilho_Alavancas : MonoBehaviour
{
    [Header("Referências da Alavanca")]
    public Animator animator; // Referência ao Animator da alavanca

    [Header("Configurações de Evento")]
    public TipoEvento eventoTipo; // Tipo de evento associado à alavanca
    public Gerenciamento_Eventos ativaEvento; // Referência ao objeto que gerencia o evento

    [Header("Audios das alavancas")]
    // Referência ao Fonte de Áudio que tocará o audio
    public AudioSource fonteAudio;

    public enum TipoEvento
    {
        Portão,
        Barreira
    }

    [Header("Controle de Estado do Jogador")]
    private bool jogadorContato = false; // Variável para verificar se o jogador está em contato com a alavanca
    private bool nãoAtivaNovamente = false; // Impede que a alavanca seja ativada novamente após sua primeira ativação

    [Header("Configurações de Interação")]
    public string botaoInteracao = "Attack"; // Variável para o nome do botão de interação (valor padrão: "Attack")

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
            nãoAtivaNovamente = true; // Impede que a alavanca seja ativada novamente
            // Atualiza a animação da alavanca
            fonteAudio.Play();
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
        // Chama o método botao() apenas se a alavanca ainda não tiver sido ativada
        if (!nãoAtivaNovamente)
        {
            botao();
        }
    }
}