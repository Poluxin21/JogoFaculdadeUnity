using UnityEngine;

public class GatilhoDeArea : MonoBehaviour
{
    // Referência ao GerenciadorDeMusica
    public GerenciadorDeMusica gerenciadorDeMusica;

    // Tipo de área (usado para determinar a música)
    public AreaTipo tipoDeArea; // Usando um enum
    public float volume = 1f;   // Volume específico para a área (padrão: 1)

    // Enum para tipos de áreas
    public enum AreaTipo
    {
        Floresta,
        Ruinas,
        Templo,
        SalaDoTrono
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        // Verifica se o objeto que entrou é o jogador
        if (outro.CompareTag("Player") && gerenciadorDeMusica != null)
        {
            // Toca a música correspondente à área
            switch (tipoDeArea)
            {
                case AreaTipo.Floresta:
                    gerenciadorDeMusica.TocarMusica(gerenciadorDeMusica.musicaFloresta, volume);
                    break;
                case AreaTipo.Ruinas:
                    gerenciadorDeMusica.TocarMusica(gerenciadorDeMusica.musicaRuinas, volume);
                    break;
                case AreaTipo.Templo:
                    gerenciadorDeMusica.TocarMusica(gerenciadorDeMusica.musicaTemplo, volume);
                    break;
                case AreaTipo.SalaDoTrono:
                    gerenciadorDeMusica.TocarMusica(gerenciadorDeMusica.musicaSalaDoTrono, volume);
                    break;
            }
        }
    }
}