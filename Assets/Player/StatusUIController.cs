using UnityEngine;
using UnityEngine.UI;

public class StatusUIController : MonoBehaviour
{
    // Refer�ncias aos cora��es da vida
    [SerializeField] private Image[] heartImages; // Array de imagens dos cora��es

    // Refer�ncias aos cristais de energia
    [SerializeField] private Image[] energyImages; // Array de imagens dos cristais de energia

    // Valores configur�veis no Inspector
    [Header("Configura��es")]
    [SerializeField] private int maxHealth = 8; // M�ximo de cora��es
    [SerializeField] private int currentHealth = 8; // Cora��es atuais

    [SerializeField] private int maxEnergy = 4; // M�ximo de cristais de energia
    [SerializeField] private int currentEnergy = 4; // Cristais de energia atuais

    // Cores para cora��es cheios e vazios
    [SerializeField] private Color fullHeartColor = Color.white; // Cor do cora��o cheio
    [SerializeField] private Color emptyHeartColor = Color.black; // Cor do cora��o vazio

    // Cores para cristais de energia cheios e vazios
    [SerializeField] private Color fullEnergyColor = Color.white; // Cor do cristal cheio
    [SerializeField] private Color emptyEnergyColor = Color.black; // Cor do cristal vazio

    void Start()
    {
        // Inicializa os cora��es e cristais na tela
        UpdateHearts();
        UpdateEnergy();
    }

    // Atualiza os cora��es na tela
    private void UpdateHearts()
    {
        // Validar refer�ncias
        if (heartImages == null || heartImages.Length == 0)
        {
            Debug.LogError("heartImages array is not set or is empty.");
            return;
        }

        // Ajustar o n�mero de cora��es vis�veis
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].color = fullHeartColor; // Define a cor do cora��o cheio
            }
            else
            {
                heartImages[i].color = emptyHeartColor; // Define a cor do cora��o vazio
            }
        }
    }

    // Atualiza os cristais de energia na tela
    private void UpdateEnergy()
    {
        // Validar refer�ncias
        if (energyImages == null || energyImages.Length == 0)
        {
            Debug.LogError("energyImages array is not set or is empty.");
            return;
        }

        // Ajustar o n�mero de cristais vis�veis
        for (int i = 0; i < energyImages.Length; i++)
        {
            if (i < currentEnergy)
            {
                energyImages[i].color = fullEnergyColor; // Define a cor do cristal cheio
            }
            else
            {
                energyImages[i].color = emptyEnergyColor; // Define a cor do cristal vazio
            }
        }
    }

    // M�todo chamado pelo PlayerController ou outro script para atualizar a vida
    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHearts();
    }

    // M�todo chamado pelo PlayerController ou outro script para atualizar a energia
    public void SetEnergy(int newEnergy)
    {
        currentEnergy = Mathf.Clamp(newEnergy, 0, maxEnergy);
        UpdateEnergy();
    }

    // M�todos para permitir atualiza��o din�mica no Inspector
    private void OnValidate()
    {
        // Garante que os valores sejam clamped mesmo quando ajustados no Inspector
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        // Atualiza a UI imediatamente ap�s altera��es no Inspector
        UpdateHearts();
        UpdateEnergy();
    }
}