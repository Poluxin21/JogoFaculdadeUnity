using UnityEngine;
using UnityEngine.UI;

public class StatusUIController : MonoBehaviour
{
    // Referências aos corações da vida
    [SerializeField] private Image[] heartImages; // Array de imagens dos corações

    // Referências aos cristais de energia
    [SerializeField] private Image[] energyImages; // Array de imagens dos cristais de energia

    // Valores configuráveis no Inspector
    [Header("Configurações")]
    [SerializeField] private int maxHealth = 8; // Máximo de corações
    [SerializeField] private int currentHealth = 8; // Corações atuais

    [SerializeField] private int maxEnergy = 4; // Máximo de cristais de energia
    [SerializeField] private int currentEnergy = 4; // Cristais de energia atuais

    // Cores para corações cheios e vazios
    [SerializeField] private Color fullHeartColor = Color.white; // Cor do coração cheio
    [SerializeField] private Color emptyHeartColor = Color.black; // Cor do coração vazio

    // Cores para cristais de energia cheios e vazios
    [SerializeField] private Color fullEnergyColor = Color.white; // Cor do cristal cheio
    [SerializeField] private Color emptyEnergyColor = Color.black; // Cor do cristal vazio

    void Start()
    {
        // Inicializa os corações e cristais na tela
        UpdateHearts();
        UpdateEnergy();
    }

    // Atualiza os corações na tela
    private void UpdateHearts()
    {
        // Validar referências
        if (heartImages == null || heartImages.Length == 0)
        {
            Debug.LogError("heartImages array is not set or is empty.");
            return;
        }

        // Ajustar o número de corações visíveis
        for (int i = 0; i < heartImages.Length; i++)
        {
            if (i < currentHealth)
            {
                heartImages[i].color = fullHeartColor; // Define a cor do coração cheio
            }
            else
            {
                heartImages[i].color = emptyHeartColor; // Define a cor do coração vazio
            }
        }
    }

    // Atualiza os cristais de energia na tela
    private void UpdateEnergy()
    {
        // Validar referências
        if (energyImages == null || energyImages.Length == 0)
        {
            Debug.LogError("energyImages array is not set or is empty.");
            return;
        }

        // Ajustar o número de cristais visíveis
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

    // Método chamado pelo PlayerController ou outro script para atualizar a vida
    public void SetHealth(int newHealth)
    {
        currentHealth = Mathf.Clamp(newHealth, 0, maxHealth);
        UpdateHearts();
    }

    // Método chamado pelo PlayerController ou outro script para atualizar a energia
    public void SetEnergy(int newEnergy)
    {
        currentEnergy = Mathf.Clamp(newEnergy, 0, maxEnergy);
        UpdateEnergy();
    }

    // Métodos para permitir atualização dinâmica no Inspector
    private void OnValidate()
    {
        // Garante que os valores sejam clamped mesmo quando ajustados no Inspector
        currentHealth = Mathf.Clamp(currentHealth, 0, maxHealth);
        currentEnergy = Mathf.Clamp(currentEnergy, 0, maxEnergy);

        // Atualiza a UI imediatamente após alterações no Inspector
        UpdateHearts();
        UpdateEnergy();
    }
}