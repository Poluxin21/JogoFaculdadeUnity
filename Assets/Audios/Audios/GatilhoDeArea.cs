using UnityEngine;

public class GatilhoDeArea : MonoBehaviour
{
    // Refer�ncia ao GerenciadorDeMusica
    public GerenciadorDeMusica gerenciadorDeMusica;

    // Tipo de �rea (usado para determinar a m�sica)
    public AreaTipo tipoDeArea; // Usando um enum
    public float volume = 1f;   // Volume espec�fico para a �rea (padr�o: 1)

    // Enum para tipos de �reas
    public enum AreaTipo
    {
        Floresta,
        Ruinas,
        Templo,
        SalaDoTrono
    }

    void OnTriggerEnter2D(Collider2D outro)
    {
        // Verifica se o objeto que entrou � o jogador
        if (outro.CompareTag("Player") && gerenciadorDeMusica != null)
        {
            // Toca a m�sica correspondente � �rea
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