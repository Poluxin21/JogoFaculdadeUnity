using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MenuFadeController : MonoBehaviour
{
    // Referência ao componente FadeUI
    public FadeUI fadeUI;

    // Tempo de fade em segundos
    public float fadeTime = 0.8f;

    // Método para iniciar o fade out e carregar uma nova cena
    public void CallFadeAndLoadScene(string sceneToLoad)
    {
        StartCoroutine(FadeAndLoadScene(sceneToLoad));
    }

    // Corrotina para executar o fade out e carregar a cena
    private System.Collections.IEnumerator FadeAndLoadScene(string sceneToLoad)
    {
        // Inicia o fade out
        fadeUI.FadeOut(fadeTime);

        // Espera o tempo de fade
        yield return new WaitForSeconds(fadeTime);

        // Carrega a nova cena
        SceneManager.LoadScene(sceneToLoad);
    }
}