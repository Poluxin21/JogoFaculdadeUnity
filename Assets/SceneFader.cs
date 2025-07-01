using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;
using UnityEngine.SceneManagement;

public class SceneFader : MonoBehaviour
{
    [SerializeField] private float fadeTime = 1f; // Valor padrão

    private Image fadeOutUIImage;

    public enum FadeDirection
    {
        In, Out
    }

    private void Awake()
    {
        fadeOutUIImage = GetComponent<Image>();

        // Garante que a imagem existe
        if (fadeOutUIImage == null)
        {
            Debug.LogError("SceneFader precisa de um componente Image!");
        }
    }

    public IEnumerator Fade(FadeDirection fadeDirection)
    {
        if (fadeOutUIImage == null)
        {
            Debug.LogError("fadeOutUIImage é null no SceneFader!");
            yield break;
        }

        float _alpha = fadeDirection == FadeDirection.Out ? 1 : 0;
        float _fadeEndValue = fadeDirection == FadeDirection.Out ? 0 : 1; // Corrigido aqui

        if (fadeDirection == FadeDirection.Out)
        {
            while (_alpha >= _fadeEndValue)
            {
                SetColorImage(ref _alpha, fadeDirection);
                yield return null;
            }
            fadeOutUIImage.enabled = false;
        }
        else
        {
            fadeOutUIImage.enabled = true;
            while (_alpha <= _fadeEndValue)
            {
                SetColorImage(ref _alpha, fadeDirection);
                yield return null;
            }
        }
    }

    public IEnumerator FadeAndLoadScene(FadeDirection fadeDirection, string levelToLoad)
    {
        if (fadeOutUIImage == null)
        {
            Debug.LogError("fadeOutUIImage é null no FadeAndLoadScene!");
            yield break;
        }

        fadeOutUIImage.enabled = true;
        yield return Fade(fadeDirection);
        SceneManager.LoadScene(levelToLoad);
    }

    void SetColorImage(ref float _alpha, FadeDirection fadeDirection)
    {
        if (fadeOutUIImage != null)
        {
            fadeOutUIImage.color = new Color(fadeOutUIImage.color.r, fadeOutUIImage.color.g, fadeOutUIImage.color.b, _alpha);
            _alpha += Time.deltaTime * (1 / fadeTime) * (fadeDirection == FadeDirection.Out ? -1 : 1);
        }
    }
}