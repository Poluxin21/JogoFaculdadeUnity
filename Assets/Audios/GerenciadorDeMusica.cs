using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Rendering;

public class GerenciadorDeMusica : MonoBehaviour
{
    // Referência ao Fonte de Áudio que tocará as músicas
    public AudioSource fonteAudio;

    // Lista de músicas associadas a cada área
    public AudioClip musicaFloresta; // Música da Floresta Esquecida
    public AudioClip musicaRuinas;  // Música das Ruínas
    public AudioClip musicaTemplo;  // Música do Templo
    public AudioClip musicaSalaDoTrono; // Música da Sala do Trono

    // Variável para armazenar a música atual
    private AudioClip musicaAtual;

    void Start()
    {
        // Certifique-se de que o Fonte de Áudio esteja configurado corretamente
        if (fonteAudio == null)
        {
            fonteAudio = GetComponent<AudioSource>();
        }

        // Inicia sem música ou com uma música padrão
        if (musicaAtual == null && musicaFloresta != null)
        {
            TocarMusica(musicaFloresta); // Define a música da floresta como padrão
        }
    }

    // Método para reproduzir uma nova música
    public void TocarMusica(AudioClip novaMusica, float volume = 1)
    {
        // Se a música for diferente da atual, troque-a
        if (novaMusica != musicaAtual)
        {
            musicaAtual = novaMusica;
            fonteAudio.clip = novaMusica;
            fonteAudio.volume = volume;
            fonteAudio.Play();
        }
    }

    // Método para parar a música atual
    public void PararMusica()
    {
        fonteAudio.Stop();
    }
}