using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class CanvasMenuManager : MonoBehaviour
{
    [Header("Pain�is do Canvas")]
    [SerializeField] private GameObject statusPlayerPanel;
    [SerializeField] private FadeUI menuConfigPanel;
    [SerializeField] private FadeUI deathPanel;
    [SerializeField] private FadeUI menuPausePanel;

    [Header("Bot�es")]
    [SerializeField] private Button pauseButton;
    [SerializeField] private Button retomarButton;
    [SerializeField] private Button menuPrincipalButton;
    [SerializeField] private Button menuConfigButton;
    [SerializeField] private Button retornaButton;
    [SerializeField] private Button respawnButton;

    [Header("Configura��es")]
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

        // Inicializar pain�is
        InitializePanels();

        // Configurar bot�es
        SetupButtons();
    }

    private void Start()
    {
        // StatusPlayerPanel sempre ativo
        if (statusPlayerPanel != null)
        {
            statusPlayerPanel.SetActive(true);
        }

        // Outros pain�is come�am desativados
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
        // Verificar se todos os pain�is est�o atribu�dos
        if (statusPlayerPanel == null) Debug.LogWarning("StatusPlayerPanel n�o atribu�do!");
        if (menuConfigPanel == null) Debug.LogWarning("MenuConfigPanel n�o atribu�do!");
        if (deathPanel == null) Debug.LogWarning("DeathPanel n�o atribu�do!");
        if (menuPausePanel == null) Debug.LogWarning("MenuPausePanel n�o atribu�do!");

        // Garantir que os pain�is de menu t�m CanvasGroup
        ValidateCanvasGroup(menuConfigPanel);
        ValidateCanvasGroup(deathPanel);
        ValidateCanvasGroup(menuPausePanel);
    }

    private void ValidateCanvasGroup(FadeUI fadeUI)
    {
        if (fadeUI != null && fadeUI.GetComponent<CanvasGroup>() == null)
        {
            Debug.LogError($"FadeUI em {fadeUI.name} n�o tem CanvasGroup!");
        }
    }

    private void SetupButtons()
    {
        // Bot�o de Pause
        if (pauseButton != null)
            pauseButton.onClick.AddListener(PauseGame);

        // Bot�es do Menu de Pause
        if (retomarButton != null)
            retomarButton.onClick.AddListener(ResumeGame);

        if (menuPrincipalButton != null)
            menuPrincipalButton.onClick.AddListener(GoToMainMenu);

        if (menuConfigButton != null)
            menuConfigButton.onClick.AddListener(OpenConfigMenu);

        // Bot�o de Retorno do Config
        if (retornaButton != null)
            retornaButton.onClick.AddListener(ReturnFromConfig);

        // Bot�o de Respawn
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

    // M�todos p�blicos para controle dos menus
    public void PauseGame()
    {
        if (isInDeathMenu) return; // N�o pode pausar se estiver morto

        isPaused = true;
        Time.timeScale = 0f;

        // Esconder bot�o de pause
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        ShowPanel(menuPausePanel);
    }

    public void ResumeGame()
    {
        isPaused = false;
        isInConfigMenu = false;
        Time.timeScale = 1f;

        // Mostrar bot�o de pause
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
        isPaused = false; // N�o � pause, � morte

        // Esconder bot�o de pause
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(false);

        ShowPanel(deathPanel);
    }

    public void RespawnPlayer()
    {
        isInDeathMenu = false;

        // Mostrar bot�o de pause novamente
        if (pauseButton != null)
            pauseButton.gameObject.SetActive(true);

        HideAllMenuPanels();

        // Reiniciar a cena atual
        SceneManager.LoadScene(SceneManager.GetActiveScene().name);
    }

    // M�todo para ser chamado pelo PlayerController quando morrer
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

    // M�todos para controle externo
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

    // M�todo para for�ar o fechamento de todos os menus (�til para cutscenes)
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