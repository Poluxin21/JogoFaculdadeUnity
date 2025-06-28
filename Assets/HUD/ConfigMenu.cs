using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class ConfigMenu : MonoBehaviour
{
    // Referência ao Audio Mixer
    public AudioMixer audioMixer;

    // Método para ajustar o volume
    public void SetVolume(float _volume)
    {
        // Define o valor do parâmetro "volume" no Audio Mixer
        audioMixer.SetFloat("volume", _volume);
    }

    // Método para ajustar a qualidade gráfica
    public void SetQuality(int _qualityIndex)
    {
        // Define o nível de qualidade gráfica
        QualitySettings.SetQualityLevel(_qualityIndex);
    }

    // Método para alternar entre tela cheia e janela
    public void SetFullScreen(bool _isFullScreen)
    {
        // Define o modo de tela cheia
        Screen.fullScreen = _isFullScreen;
    }

    // Método para sair do jogo
    public void QuitGame()
    {
        // Sai do jogo (funciona apenas em builds)
        Application.Quit();
    }
}
