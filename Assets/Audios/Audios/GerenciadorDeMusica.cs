using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class GerenciadorDeMusica : MonoBehaviour
{
    // Refer�ncia ao Fonte de �udio que tocar� as m�sicas
    public AudioSource fonteAudio;

    // Lista de m�sicas associadas a cada �rea
    public AudioClip musicaFloresta; // M�sica da Floresta Esquecida
    public AudioClip musicaRuinas;  // M�sica das Ru�nas
    public AudioClip musicaTemplo;  // M�sica do Templo
    public AudioClip musicaSalaDoTrono; // M�sica da Sala do Trono

    // Vari�vel para armazenar a m�sica atual
    private AudioClip musicaAtual;

    void Start()
    {
        // Certifique-se de que o Fonte de �udio esteja configurado corretamente
        if (fonteAudio == null)
        {
            fonteAudio = GetComponent<AudioSource>();
        }

        // Inicia sem m�sica ou com uma m�sica padr�o
        if (musicaAtual == null && musicaFloresta != null)
        {
            TocarMusica(musicaFloresta); // Define a m�sica da floresta como padr�o
        }
    }

    // M�todo para reproduzir uma nova m�sica
    public void TocarMusica(AudioClip novaMusica, float volume = 1)
    {
        // Se a m�sica for diferente da atual, troque-a
        if (novaMusica != musicaAtual)
        {
            musicaAtual = novaMusica;
            fonteAudio.clip = novaMusica;
            fonteAudio.volume = volume;
            fonteAudio.Play();
        }
    }

    // M�todo para parar a m�sica atual
    public void PararMusica()
    {
        fonteAudio.Stop();
    }
}