using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

public class ConfigMenu : MonoBehaviour
{
    // Refer�ncia ao Audio Mixer
    public AudioMixer audioMixer;

    // M�todo para ajustar o volume
    public void SetVolume(float _volume)
    {
        // Define o valor do par�metro "volume" no Audio Mixer
        audioMixer.SetFloat("volume", _volume);
    }

    // M�todo para ajustar a qualidade gr�fica
    public void SetQuality(int _qualityIndex)
    {
        // Define o n�vel de qualidade gr�fica
        QualitySettings.SetQualityLevel(_qualityIndex);
    }

    // M�todo para alternar entre tela cheia e janela
    public void SetFullScreen(bool _isFullScreen)
    {
        // Define o modo de tela cheia
        Screen.fullScreen = _isFullScreen;
    }

    // M�todo para sair do jogo
    public void QuitGame()
    {
        // Sai do jogo (funciona apenas em builds)
        Application.Quit();
    }
}
