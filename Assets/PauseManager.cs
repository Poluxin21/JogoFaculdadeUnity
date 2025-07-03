using UnityEngine;

public class PauseManager : MonoBehaviour
{
    public static PauseManager Instance;

    [Header("Pause Settings")]
    [SerializeField] private FadeUI menuPausePanel;
    [SerializeField] private FadeUI pauseButton;
    [SerializeField] private FadeUI statusPanel;
    [SerializeField] private float fadeDuration = 0.8f;
    private bool menuConfigi = false;

    // Vari�vel p�blica para outros scripts verificarem
    public static bool isPaused = false;

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        else
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // Opcional: mant�m entre cenas
        }
    }

    void Start()
    {
        // Verifica se o painel foi atribu�do
        if (menuPausePanel == null)
        {
            Debug.LogError("MenuPausePanel n�o est� atribu�do no PauseManager!");
        }
    }

    void Update()
    {
        // Permite pausar/despausar com TAB (opcional)
        if (Input.GetKeyDown(KeyCode.Tab) && menuConfigi == false)
        {
            if (isPaused)
                ResumeGame();
            else
                PauseGame();
        }
    }

    public void PauseGame()
    {
        isPaused = true;
        Time.timeScale = 0f; // Para o tempo do jogo

        // Mostra o painel de pausa
        if (menuPausePanel != null)
        {
            menuPausePanel.FadeIn(fadeDuration);
            pauseButton.FadeOut(fadeDuration);
            statusPanel.FadeOut(fadeDuration);
        }

        Debug.Log("Jogo pausado");
    }

    public void ResumeGame()
    {
        isPaused = false;
        Time.timeScale = 1f; // Restaura o tempo normal

        // Esconde o painel de pausa
        if (menuPausePanel != null)
        {
            menuPausePanel.FadeOut(fadeDuration);
            pauseButton.FadeIn(fadeDuration);
            statusPanel.FadeIn(fadeDuration);
        }

        Debug.Log("Jogo despausado");
    }

    // M�todo para alternar entre pausado e n�o pausado
    public void TogglePause()
    {
        if (isPaused)
            ResumeGame();
        else
            PauseGame();
    }

    public void MenuConfig()
    {
        if (menuConfigi) menuConfigi = false;
        else menuConfigi = true;
    }
}