using UnityEngine;

public class GamaManager : MonoBehaviour
{
    private Transform respawnPoint;
    public Banco banco;
    public Transform platformingRespawnPoint; // Transform padrão
    public HeartController heartController;

    public void RespawnPlayer()
    {
        if (banco.interagir)
        {
            respawnPoint = banco.transform;
        }
        else
        {
            respawnPoint = platformingRespawnPoint;
        }

        PlayerController.Instance.Respawn();
        PlayerController.Instance.transform.position = respawnPoint.position;
        heartController.UpdateHeartsHUD();
    }
}
