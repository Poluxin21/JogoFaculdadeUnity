using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class GolemGuardiao : Enemy
{
    [Header("Configura��es de Combate")]
    public float distanciaDeteccao = 12f;
    public float distanciaAtaqueMarreta = 3f;
    public float distanciaMaximaInvestida = 15f;
    public float tempoEntreSequencias = 3f;

    [Header("Ataque Marreta")]
    public float danoMarreta = 2f;
    public float tempoAtivarHitboxMarreta = 0.8f;
    public float duracaoHitboxMarreta = 0.4f;
    public float velocidadeAvanco = 4f;

    [Header("Ataque Investida")]
    public float danoInvestida = 3f;
    public float velocidadeInvestida = 12f;
    public float tempoPreparacaoInvestida = 1f;
    public float duracaoInvestida = 0.8f;

    [Header("Mec�nica de Imobiliza��o")]
    public float tempoImovel = 2f;
    public int ataquesParaImovel = 3; // Ap�s 3 ataques (2 marreta + 1 investida)

    [Header("Refer�ncias")]
    public Transform jogador;
    public Animator animator;
    public GameObject hitboxMarreta;
    public GameObject hitboxInvestida;
    public GameObject indicadorInvestida; // Linha/seta mostrando dire��o da investida

    [Header("Progresso do Jogador")]
    public int ataquesNormaisSobrevividos = 0; // Para rastrear os 15 ataques

    private float tempoUltimaSequencia;
    private bool executandoSequencia;
    private bool imovel;
    private int contadorAtaquesSequencia;
    //private PadraoAtaque proximoPadrao = 0;
    private Vector2 direcaoInvestida;
    private Vector2 posicaoInicialInvestida;

    public enum PadraoAtaque
    {
        Marreta,
        Investida
    }

    protected override void Start()
    {
        base.Start();

        // Busca o jogador se n�o foi atribu�do
        if (jogador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                jogador = playerObj.transform;
        }

        // Configura as hitboxes
        ConfigurarHitboxes();

        // Inicializa com ataque de marreta
        //proximoPadrao = PadraoAtaque.Marreta;
        contadorAtaquesSequencia = 0;
        tempoUltimaSequencia = -tempoEntreSequencias;
    }

    void ConfigurarHitboxes()
    {
        // Configura hitbox da marreta
        if (hitboxMarreta != null)
        {
            hitboxMarreta.SetActive(false);
            HitboxBoss hb = hitboxMarreta.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(danoMarreta);
        }

        // Configura hitbox da investida
        if (hitboxInvestida != null)
        {
            hitboxInvestida.SetActive(false);
            HitboxBoss hb = hitboxInvestida.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(danoInvestida);
        }

        // Esconde o indicador de investida
        if (indicadorInvestida != null)
        {
            indicadorInvestida.SetActive(false);
        }
    }

    protected override void Update()
    {
        base.Update();

        if (PauseManager.isPaused) return;

        if (PlayerController.Instance.pState.alive == false) return;

        if (jogador == null || health <= 0)
            return;

        // Se est� im�vel, apenas conta o tempo
        if (imovel)
        {
            rb.linearVelocity = Vector2.zero;
            animator.SetBool("Andando", false);
            return;
        }

        // Se est� executando sequ�ncia, n�o faz mais nada
        if (executandoSequencia)
        {
            return;
        }

        float distancia = Vector2.Distance(transform.position, jogador.position);

        // Verifica se pode iniciar nova sequ�ncia
        if (Time.time - tempoUltimaSequencia >= tempoEntreSequencias)
        {
            if (distancia <= distanciaDeteccao)
            {
                IniciarSequenciaAtaques();
            }
        }
        else if (distancia <= distanciaDeteccao)
        {
            PerseguirJogador();
        }
        else
        {
            Parar();
        }
    }

    void PerseguirJogador()
    {
        Vector2 direcao = (jogador.position - transform.position).normalized;
        rb.linearVelocity = direcao * speed;

        VirarParaJogador();
        animator.SetBool("Andando", true);
    }

    void Parar()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Andando", false);
    }

    void IniciarSequenciaAtaques()
    {
        StartCoroutine(ExecutarSequenciaAtaques());
    }

    IEnumerator ExecutarSequenciaAtaques()
    {
        executandoSequencia = true;
        tempoUltimaSequencia = Time.time;
        contadorAtaquesSequencia = 0;

        // Executa 2 ataques de marreta
        for (int i = 0; i < 2; i++)
        {
            yield return StartCoroutine(ExecutarAtaqueMarreta());
            contadorAtaquesSequencia++;

            // Pequena pausa entre ataques de marreta
            yield return new WaitForSeconds(0.5f);
        }

        // Executa 1 ataque de investida
        yield return StartCoroutine(ExecutarAtaqueInvestida());
        contadorAtaquesSequencia++;

        // Ap�s 3 ataques, fica im�vel
        yield return StartCoroutine(FicarImovel());

        executandoSequencia = false;

        // Incrementa contador de ataques normais sobrevividos pelo jogador
        ataquesNormaisSobrevividos++;

        // Verifica se jogador sobreviveu a 15 ataques normais
        if (ataquesNormaisSobrevividos >= 15)
        {
            CompletarFase();
        }
    }

    IEnumerator ExecutarAtaqueMarreta()
    {
        Parar();
        VirarParaJogador();

        // Avan�a em dire��o ao jogador
        Vector2 direcaoAvanco = (jogador.position - transform.position).normalized;
        animator.SetTrigger("AtaqueMarreta");

        // Movimento de avan�o durante a anima��o
        float tempoAvanco = tempoAtivarHitboxMarreta;
        float tempoInicio = Time.time;

        while (Time.time - tempoInicio < tempoAvanco)
        {
            rb.linearVelocity = direcaoAvanco * velocidadeAvanco;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;

        // Ativa hitbox da marreta
        if (VerificarDistanciaAtaque(distanciaAtaqueMarreta))
        {
            AtivarHitbox(hitboxMarreta);
            yield return new WaitForSeconds(duracaoHitboxMarreta);
            DesativarHitbox(hitboxMarreta);
        }

        // Aguarda o resto da anima��o
        yield return new WaitForSeconds(0.3f);

        Debug.Log("Ataque de Marreta executado!");
    }

    IEnumerator ExecutarAtaqueInvestida()
    {
        Parar();

        // Fase de prepara��o - mostra indicador de dire��o
        posicaoInicialInvestida = transform.position;
        direcaoInvestida = (jogador.position - transform.position).normalized;

        VirarParaDirecao(direcaoInvestida);

        // Mostra indicador de investida
        if (indicadorInvestida != null)
        {
            indicadorInvestida.SetActive(true);
            // Rotaciona o indicador na dire��o da investida
            float angulo = Mathf.Atan2(direcaoInvestida.y, direcaoInvestida.x) * Mathf.Rad2Deg;
            indicadorInvestida.transform.rotation = Quaternion.AngleAxis(angulo, Vector3.forward);
        }

        animator.SetTrigger("PreparaInvestida");

        // Tempo de prepara��o (jogador pode se esquivar)
        yield return new WaitForSeconds(tempoPreparacaoInvestida);

        // Esconde indicador
        if (indicadorInvestida != null)
        {
            indicadorInvestida.SetActive(false);
        }

        // Executa a investida
        animator.SetTrigger("Investida");
        AtivarHitbox(hitboxInvestida);

        float tempoInvestida = 0f;
        while (tempoInvestida < duracaoInvestida)
        {
            rb.linearVelocity = direcaoInvestida * velocidadeInvestida;

            // Para se colidir com parede ou limite da arena
            if (VerificouColisaoParede())
            {
                break;
            }

            tempoInvestida += Time.deltaTime;
            yield return null;
        }

        rb.linearVelocity = Vector2.zero;
        DesativarHitbox(hitboxInvestida);

        // Pequena pausa ap�s investida
        yield return new WaitForSeconds(0.5f);

        Debug.Log("Ataque de Investida executado!");
    }

    IEnumerator FicarImovel()
    {
        imovel = true;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Imovel");

        Debug.Log("Golem ficou im�vel por " + tempoImovel + " segundos!");

        yield return new WaitForSeconds(tempoImovel);

        imovel = false;
        animator.SetTrigger("RecuperarMovimento");

        Debug.Log("Golem recuperou movimento!");
    }

    bool VerificouColisaoParede()
    {
        // Raycast para verificar se h� parede � frente
        RaycastHit2D hit = Physics2D.Raycast(transform.position, direcaoInvestida, 1f);
        return hit.collider != null && (hit.collider.CompareTag("Wall") || hit.collider.CompareTag("Boundary"));
    }

    void VirarParaJogador()
    {
        if (jogador == null) return;

        Vector2 direcao = (jogador.position - transform.position).normalized;
        VirarParaDirecao(direcao);
    }

    void VirarParaDirecao(Vector2 direcao)
    {
        if (direcao.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direcao.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    bool VerificarDistanciaAtaque(float distanciaMaxima)
    {
        if (jogador == null) return false;
        float distanciaAtual = Vector2.Distance(transform.position, jogador.position);
        return distanciaAtual <= distanciaMaxima + 1f;
    }

    void AtivarHitbox(GameObject hitbox)
    {
        if (hitbox != null)
        {
            hitbox.SetActive(true);
            Debug.Log($"Hitbox {hitbox.name} ativada!");
        }
    }

    void DesativarHitbox(GameObject hitbox)
    {
        if (hitbox != null)
        {
            hitbox.SetActive(false);
            Debug.Log($"Hitbox {hitbox.name} desativada!");
        }
    }

    void CompletarFase()
    {
        Debug.Log("Jogador sobreviveu a 15 ataques normais! Liberando acesso � segunda fase!");

        // Aqui voc� pode adicionar l�gica para:
        // - Abrir porta para pr�xima �rea
        // - Ativar checkpoint
        // - Mostrar UI de progresso
        // - Etc.

        // Exemplo: desabilitar o boss
        gameObject.SetActive(false);

        // Ou chamar um GameManager
        // GameManager.Instance.LiberarSegundaFase();
    }

    public override void EnemyHit(float dano, Vector2 direcao, float forca)
    {
        base.EnemyHit(dano, direcao, forca);

        if (health <= 0)
        {
            DesativarTodasHitboxes();
            Morrer();
        }
    }

    void DesativarTodasHitboxes()
    {
        if (hitboxMarreta != null)
            hitboxMarreta.SetActive(false);
        if (hitboxInvestida != null)
            hitboxInvestida.SetActive(false);
        if (indicadorInvestida != null)
            indicadorInvestida.SetActive(false);
    }

    void Morrer()
    {
        executandoSequencia = false;
        imovel = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Morte");

        GetComponent<Collider2D>().enabled = false;

        Destroy(gameObject, 3f);
    }

    // Para debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaqueMarreta);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanciaMaximaInvestida);
    }
}
