using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneTransition : MonoBehaviour
{
    [SerializeField] private string transitionTo;
    [SerializeField] private Transform startPoint;
    [SerializeField] private Vector2 exitDirection;
    [SerializeField] private float exitTime;
    [SerializeField] private float activationDelay = 1f; // Delay antes do trigger ficar ativo

    [SerializeField] public bool liberado = true;

    private bool canTransition = false; // Controla se a transição pode acontecer
    private bool hasTriggered = false; // Previne múltiplas ativações

    private void Start()
    {
        // Verifica se o GameManager existe antes de usar
        if (GameManager.instance != null && transitionTo == GameManager.instance.transitionFromScene)
        {
            if (PlayerController.Instance != null && startPoint != null)
            {
                PlayerController.Instance.transform.position = startPoint.position;
                StartCoroutine(PlayerController.Instance.WalkIntoNewScene(exitDirection, exitTime));
            }
        }

        // Verifica se o UIManager existe antes de usar
        if (UIManager.instance != null && UIManager.instance.sceneFader != null)
        {
            StartCoroutine(UIManager.instance.sceneFader.Fade(SceneFader.FadeDirection.Out));
        }

        // Inicia o delay de ativação
        StartCoroutine(ActivateTransitionAfterDelay());
    }

    private IEnumerator ActivateTransitionAfterDelay()
    {
        yield return new WaitForSeconds(activationDelay);
        canTransition = true;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        // Adiciona verificações de segurança
        if (other.CompareTag("Player") &&
            liberado &&
            canTransition &&
            !hasTriggered &&
            !string.IsNullOrEmpty(transitionTo))
        {
            hasTriggered = true; // Previne múltiplas ativações
            StartCoroutine(ExecuteTransition());
        }
    }

    private IEnumerator ExecuteTransition()
    {
        // Verifica se o GameManager existe
        if (GameManager.instance != null)
        {
            GameManager.instance.transitionFromScene = SceneManager.GetActiveScene().name;
        }

        // Verifica se o PlayerController existe
        if (PlayerController.Instance != null)
        {
            PlayerController.Instance.pState.cutScene = true;
        }

        // Pequeno delay adicional para garantir que tudo está pronto
        yield return new WaitForSeconds(0.1f);

        // Verifica se o UIManager existe antes de fazer a transição
        if (UIManager.instance != null && UIManager.instance.sceneFader != null)
        {
            yield return StartCoroutine(UIManager.instance.sceneFader.FadeAndLoadScene(SceneFader.FadeDirection.In, transitionTo));
        }
        else
        {
            // Fallback caso o UIManager não exista
            SceneManager.LoadScene(transitionTo);
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        // Reset da flag quando o player sai do trigger
        if (other.CompareTag("Player"))
        {
            StartCoroutine(ResetTriggerAfterDelay());
        }
    }

    private IEnumerator ResetTriggerAfterDelay()
    {
        yield return new WaitForSeconds(0.5f);
        hasTriggered = false;
    }

    public void IrParaCena()
    {
        liberado = true;
        canTransition = true;
    }

    // Método para debugar no inspector
    private void OnDrawGizmos()
    {
        if (startPoint != null)
        {
            Gizmos.color = canTransition ? Color.green : Color.red;
            Gizmos.DrawWireSphere(startPoint.position, 0.5f);

            // Desenha a direção de saída
            Gizmos.color = Color.blue;
            Gizmos.DrawRay(startPoint.position, exitDirection);
        }
    }
}