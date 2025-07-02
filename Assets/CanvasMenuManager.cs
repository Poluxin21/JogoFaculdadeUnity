using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasMenuManager : MonoBehaviour
{
    [Header("Painéis do Canvas")]
    [SerializeField] private GameObject statusPlayerPanel;
    [SerializeField] private FadeUI menuConfigPanel;
    [SerializeField] private FadeUI deathPanel;
    [SerializeField] private FadeUI menuPausePanel;

    [Header("Botões")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button retomarButton;
    [SerializeField] private Button menuPrincipalButton;
    [SerializeField] private Button menuConfigButton;
    [SerializeField] private Button retornaButton;
    [SerializeField] private Button respawnButton;

    [Header("Configurações")]
    [SerializeField] private float fadeTime = 0.5f;
    [SerializeField] private string menuPrincipalSceneName = "MenuPrincipal";

    // Controle de estado
    private bool isPaused = false;
    private bool isInConfigMenu = false;
    private bool isInDeathMenu = false;
    private FadeUI currentActivePanel = null;

    // Singleton
    public static CanvasMenuManager Instance { get; private set; }

    private void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;

        // Inicializar painéis
        InitializePanels();

        // Configurar botões
        SetupButtons();
    }

    private void Start()
    {
        // StatusPlayerPanel sempre ativo
        if (statusPlayerPanel != null)
        {
            statusPlayerPanel.SetActive(true);
        }

        // Outros painéis começam desativados
        HideAllMenuPanels();
    }

    private void Update()
    {
        // Input para pausar/despausar
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isInConfigMenu)
            {
                ReturnFromConfig();
            }
            else if (isPaused)
            {
                ResumeGame();
            }
            else if (!isInDeathMenu)
            {
                PauseGame();
            }
        }
    }

    private void InitializePanels()
    {
        // Verificar se todos os painéis estão atribuídos
        if (statusPlayerPanel == null) Debug.LogWarning("StatusPlayerPanel não atribuído!");
        if (menuConfigPanel == null) Debug.LogWarning("MenuConfigPanel não atribuído!");
        if (deathPanel == null) Debug.LogWarning("DeathPanel não atribuído!");
        if (menuPausePanel == null) Debug.LogWarning("MenuPausePanel não atribuído!");

        // Garantir que os painéis de menu têm CanvasGroup
        ValidateCanvasGroup(menuConfigPanel);
        ValidateCanvasGroup(deathPanel);
        ValidateCanvasGroup(menuPausePanel);
    }

    private void ValidateCanvasGroup(FadeUI fadeUI)
    {
        if (fadeUI != null && fadeUI.GetComponent<CanvasGroup>() == null)
        {
            Debug.LogError($"FadeUI em {fadeUI.name} não tem CanvasGroup!");
        }
    }

    private void SetupButtons()
    {
        // Botão de Pause
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);

        // Botões do Menu de Pause
        if (retomarButton != null)
            retomarButton.onClick.AddListener(ResumeGame);

        if (menuPrincipalButton != null)
            menuPrincipalButton.onClick.AddListener(GoToMainMenu);

        if (menuConfigButton != null)
            menuConfigButton.onClick.AddListener(OpenConfigMenu);

        // Botão de Retorno do Config
        if (retornaButton != null)
            retornaButton.onClick.AddListener(ReturnFromConfig);

        // Botão de Respawn
        if (respawnButton != null)
            respawnButton.onClick.AddListener(RespawnPlayer);
    }

    private void HideAllMenuPanels()
    {
        HidePanel(menuConfigPanel);
        HidePanel(deathPanel);
        HidePanel(menuPausePanel);

        currentActivePanel = null;
    }

    private void HidePanel(FadeUI panel)
    {
        if (panel != null)
        {
            CanvasGroup canvasGroup = panel.GetComponent<CanvasGroup>();
            if (canvasGroup != null)
            {
                canvasGroup.alpha = 0f;
                canvasGroup.interactable = false;
                canvasGroup.blocksRaycasts = false;
            }
        }
    }

    private void ShowPanel(FadeUI panel)
    {
        if (panel != null)
        {
            // Esconder painel atual se houver
            if (currentActivePanel != null && currentActivePanel != panel)
            {
                currentActivePanel.FadeOut(fadeTime * 0.5f);
            }

            // Mostrar novo painel
            panel.FadeIn(fadeTime);
            currentActivePanel = panel;
        }
    }

    // Métodos públicos para controle dos menus
    public void PauseGame()
    {
        if (isInDeathMenu) return; // Não pode pausar se estiver morto

        isPaused = true;
        Time.timeScale = 0f;

        // Esconder botão de pause
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        ShowPanel(menuPausePanel);
    }

    public void ResumeGame()
    {
        isPaused = false;
        isInConfigMenu = false;
        Time.timeScale = 1f;

        // Mostrar botão de pause
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);

        HideAllMenuPanels();
    }

    public void OpenConfigMenu()
    {
        if (!isPaused) return;

        isInConfigMenu = true;
        ShowPanel(menuConfigPanel);
    }

    public void ReturnFromConfig()
    {
        if (!isPaused) return;

        isInConfigMenu = false;
        ShowPanel(menuPausePanel);
    }

    public void GoToMainMenu()
    {
        Time.timeScale = 1f; // Restaurar tempo normal
        SceneManager.LoadScene(menuPrincipalSceneName);
    }

    public void ShowDeathPanel()
    {
        isInDeathMenu = true;
        isPaused = false; // Não é pause, é morte

        // Esconder botão de pause
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        ShowPanel(deathPanel);
    }

    public void RespawnPlayer()
    {
        isInDeathMenu = false;

        // Mostrar botão de pause novamente
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);

        HideAllMenuPanels();

        // Reiniciar a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // Método para ser chamado pelo PlayerController quando morrer
    public void OnPlayerDeath()
    {
        StartCoroutine(HandlePlayerDeath());
    }

    private IEnumerator HandlePlayerDeath()
    {
        // Aguardar um pouco antes de mostrar o painel de morte
        yield return new WaitForSeconds(1f);
        ShowDeathPanel();
    }

    // Métodos para controle externo
    public bool IsAnyMenuOpen()
    {
        return isPaused || isInDeathMenu;
    }

    public bool IsGamePaused()
    {
        return isPaused;
    }

    public bool IsInDeathMenu()
    {
        return isInDeathMenu;
    }

    // Método para forçar o fechamento de todos os menus (útil para cutscenes)
    public void ForceCloseAllMenus()
    {
        isPaused = false;
        isInConfigMenu = false;
        isInDeathMenu = false;
        Time.timeScale = 1f;

        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);

        HideAllMenuPanels();
    }

    private void OnDestroy()
    {
        // Garantir que o tempo volte ao normal
        Time.timeScale = 1f;
    }
}