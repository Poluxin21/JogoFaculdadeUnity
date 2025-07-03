using UnityEngine;

public class Banco : MonoBehaviour
{
    public bool interagir;

    

    void OnTriggerStay2D(Collider2D other)
    {
        if (other.CompareTag("Player") && Input.GetButtonDown("Interact"))
        {
            interagir = true;
            // Salvar dados
        }
    }
}