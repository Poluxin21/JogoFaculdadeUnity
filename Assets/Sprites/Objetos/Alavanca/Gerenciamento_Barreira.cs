using UnityEngine;
using UnityEngine.Tilemaps;

public class Gerenciamento_Barreira : MonoBehaviour
{
    [Header("Configurações Gerais")]
    public int alavancasAtivadas = 0; // Contador de alavancas ativadas
    public int condicao = 2; // Número mínimo de alavancas necessárias (configurável no Inspector)

    [SerializeField] private Color corBarreira = new Color(1f, 0.8f, 0f, 0.5f); // Cor nova da barreira (amarelo transparente)
    [SerializeField] private Light luzBarreira; // Referência à luz da barreira

    private Renderer rendererBarreira; // Renderizador da barreira
    private CompositeCollider2D colliderComposite; // Collider composto da barreira
    private TilemapCollider2D colliderTilemap; // Collider do tilemap da barreira

    private void Start()
    {
        rendererBarreira = GetComponent<Renderer>();
        colliderComposite = GetComponent<CompositeCollider2D>();
        colliderTilemap = GetComponent<TilemapCollider2D>();

        if (rendererBarreira == null && colliderTilemap == null)
        {
            Debug.LogError("Nenhum Renderer ou TilemapCollider2D encontrado neste objeto.");
        }
    }

    public void AtualizaçãoAlavancasAtivadas()
    {
        alavancasAtivadas++; // Incrementa o contador de alavancas ativadas
    }

    public void ComparacaoAlavancasAtivadas()
    {
        if (alavancasAtivadas >= condicao)
        {
            // Altera a cor da barreira
            if (rendererBarreira != null)
            {
                rendererBarreira.material.color = corBarreira;
            }

            // Desativa os colliders
            if (colliderComposite != null)
            {
                colliderComposite.enabled = false;
            }

            if (colliderTilemap != null)
            {
                colliderTilemap.enabled = false;
            }

            // Altera a cor da luz
            if (luzBarreira != null)
            {
                luzBarreira.color = corBarreira;
            }

            Debug.Log("Barreira desativada!");
        }
    }
}