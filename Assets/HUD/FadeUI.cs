using UnityEngine;

public class FadeUI : MonoBehaviour
{
    // Refer�ncia ao componente CanvasGroup
    private CanvasGroup canvasGroup;

    void Awake()
    {
        // Obt�m a refer�ncia ao CanvasGroup anexado ao objeto
        canvasGroup = GetComponent<CanvasGroup>();
    }

    // M�todo para realizar o fade out
    public void FadeOut(float seconds)
    {
        // Inicia a corrotina de fade out
        StartCoroutine(FadeRoutine(seconds, false));
    }

    // M�todo para realizar o fade in
    public void FadeIn(float seconds)
    {
        // Inicia a corrotina de fade in
        StartCoroutine(FadeRoutine(seconds, true));
    }

    // Corrotina principal para fade in/fade out
    private System.Collections.IEnumerator FadeRoutine(float seconds, bool fadeIn)
    {
        // Configura os par�metros iniciais do CanvasGroup
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        float startAlpha = canvasGroup.alpha;
        float targetAlpha = fadeIn ? 1f : 0f;

        // Define o tempo inicial
        float timeElapsed = 0f;

        while (timeElapsed < seconds)
        {
            // Atualiza a opacidade gradualmente
            canvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, timeElapsed / seconds);

            // Incrementa o tempo decorrido
            timeElapsed += Time.unscaledDeltaTime;

            // Aguarda at� o pr�ximo frame
            yield return null;
        }

        // Define a opacidade final
        canvasGroup.alpha = targetAlpha;

        // Se for fade in, habilita a intera��o
        if (fadeIn)
        {
            canvasGroup.interactable = true;
            canvasGroup.blocksRaycasts = true;
        }
    }
}