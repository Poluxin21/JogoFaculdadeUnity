using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class RainhaNyarla : Enemy
{
    [Header("Configurações de Combate")]
    public float distanciaDeteccao = 15f;
    public float distanciaAtaqueMagico = 12f;
    public float tempoEntreAtaques = 1.5f;
    public float velocidadeTeletransporte = 8f;

    [Header("Sistema de Barreiras")]
    public int simbolosBarreira1 = 2;
    public int simbolosBarreira2 = 3;
    public int simbolosBarreira3 = 4;
    public float tempoRecuperacaoBarreira = 3f;

    [Header("Configurações de Dano por Barreira")]
    [Header("Primeira Barreira")]
    public float danoBarragem1 = 1f;
    public int quantidadeBarragem1 = 1;

    [Header("Segunda Barreira")]
    public float danoBarragem2 = 1f;
    public int quantidadeBarragem2 = 6;
    public float danoOndas2 = 2f;
    public int quantidadeOndas2 = 2;

    [Header("Terceira Barreira")]
    public float danoBarragem3 = 1f;
    public int quantidadeBarragem3 = 9;
    public float danoOndas3 = 2f;
    public int quantidadeOndas3 = 3;

    [Header("Ataque Supremo")]
    public float danoSupremo = 3f;
    public float duracaoSupremo = 2f;
    public float tempoCarregamentoSupremo = 1.5f;

    [Header("Referências")]
    public Transform jogador;
    public Animator animator;
    public GameObject barreira; // Barreira visual
    public GameObject[] simbolosBarreira; // Array com os símbolos da barreira
    public Transform[] pontosEstrela; // Pontos de formação da Estrela Cadente
    public GameObject projetilBarragem;
    public GameObject projetilOnda;
    public GameObject areaSupremo;

    [Header("Posições de Teletransporte")]
    public Transform[] pontosArena;

    private int barreiraAtual = 1;
    private int simbolosDestruidos = 0;
    private bool barreiraAtiva = false;
    private bool executandoAtaque = false;
    private bool recuperandoBarreira = false;
    private bool faseSupremo = false;
    private float tempoUltimoAtaque;
    private bool facingRight = true; // Controla direção sem afetar símbolos

    private List<Transform> pontosDisponiveis;

    protected override void Start()
    {
        base.Start();

        // Busca o jogador se não foi atribuído
        if (jogador == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
                jogador = playerObj.transform;
        }

        // Inicializa pontos disponíveis para teletransporte
        pontosDisponiveis = new List<Transform>(pontosArena);

        // Ativa a primeira barreira
        AtivarBarreira();

        tempoUltimoAtaque = Time.time;
    }

    protected override void Update()
    {
        base.Update();

        if (jogador == null || health <= 0)
            return;

        // Se está executando ataque ou recuperando barreira, não faz mais nada
        if (executandoAtaque || recuperandoBarreira)
            return;

        float distancia = Vector2.Distance(transform.position, jogador.position);

        // CORREÇÃO: Permite atacar mesmo com barreira ativa (boss ataca enquanto se defende)
        if (Time.time - tempoUltimoAtaque >= tempoEntreAtaques)
        {
            if (distancia <= distanciaAtaqueMagico)
            {
                EscolherAtaque();
            }
        }

        // Controla direção apenas visualmente
        VirarParaJogador();
    }

    void AtivarBarreira()
    {
        barreiraAtiva = true;
        simbolosDestruidos = 0;

        if (barreira != null)
            barreira.SetActive(true);

        // Ativa os símbolos baseado na barreira atual
        int simbolosNecessarios = GetSimbolosNecessarios();

        for (int i = 0; i < simbolosBarreira.Length; i++)
        {
            if (simbolosBarreira[i] != null)
            {
                // Reativa o símbolo se necessário
                if (i < simbolosNecessarios)
                {
                    SimboloBarreira simboloScript = simbolosBarreira[i].GetComponent<SimboloBarreira>();
                    if (simboloScript != null)
                    {
                        simboloScript.ReativarSimbolo();
                    }
                    else
                    {
                        simbolosBarreira[i].SetActive(true);
                    }
                }
                else
                {
                    simbolosBarreira[i].SetActive(false);
                }
            }
        }

        if (animator != null)
            animator.SetBool("BarreiraAtiva", true);
        Debug.Log($"Barreira {barreiraAtual} ativada com {simbolosNecessarios} símbolos!");
    }

    int GetSimbolosNecessarios()
    {
        switch (barreiraAtual)
        {
            case 1: return simbolosBarreira1;
            case 2: return simbolosBarreira2;
            case 3: return simbolosBarreira3;
            default: return simbolosBarreira1;
        }
    }

    public void DestruirSimbolo()
    {
        simbolosDestruidos++;
        Debug.Log($"Símbolo destruído! {simbolosDestruidos}/{GetSimbolosNecessarios()}");

        // Verifica se quebrou a barreira
        if (simbolosDestruidos >= GetSimbolosNecessarios())
        {
            QuebrarBarreira();
        }
    }

    void QuebrarBarreira()
    {
        barreiraAtiva = false;

        if (barreira != null)
            barreira.SetActive(false);

        if (animator != null)
        {
            animator.SetBool("BarreiraAtiva", false);
            animator.SetTrigger("BarreiraQuebrada");
        }

        Debug.Log($"Barreira {barreiraAtual} quebrada!");

        // Inicia recuperação da barreira
        StartCoroutine(RecuperarBarreira());
    }

    IEnumerator RecuperarBarreira()
    {
        recuperandoBarreira = true;

        yield return new WaitForSeconds(tempoRecuperacaoBarreira);

        // Avança para próxima barreira
        barreiraAtual++;

        // Se passou da terceira barreira, entra em fase supremo
        if (barreiraAtual > 3)
        {
            IniciarFaseSupremo();
        }
        else
        {
            AtivarBarreira();
        }

        recuperandoBarreira = false;
    }

    void EscolherAtaque()
    {
        // CORREÇÃO: Verifica se está na fase supremo
        if (faseSupremo)
        {
            // Na fase supremo, só executa ataques supremos
            if (!executandoAtaque)
                StartCoroutine(ExecutarEstrelaSuprema());
            return;
        }

        switch (barreiraAtual)
        {
            case 1:
                StartCoroutine(ExecutarAtaquePrimeiraBarreira());
                break;
            case 2:
                StartCoroutine(ExecutarAtaqueSegundaBarreira());
                break;
            case 3:
                StartCoroutine(ExecutarAtaqueTerceiraBarreira());
                break;
        }
    }

    IEnumerator ExecutarAtaquePrimeiraBarreira()
    {
        executandoAtaque = true;
        tempoUltimoAtaque = Time.time;

        // Barragem simples - 1 projétil
        if (animator != null)
            animator.SetTrigger("AtaqueBarragem");

        yield return new WaitForSeconds(0.3f);

        LancarProjetilBarragem(jogador.position, danoBarragem1);

        yield return new WaitForSeconds(0.5f);

        executandoAtaque = false;
        Debug.Log("Ataque Primeira Barreira executado!");
    }

    IEnumerator ExecutarAtaqueSegundaBarreira()
    {
        executandoAtaque = true;
        tempoUltimoAtaque = Time.time;

        // Escolhe aleatoriamente entre Barragem (6 projéteis) ou Ondas (2 ondas)
        if (Random.Range(0f, 1f) < 0.6f) // 60% chance de barragem
        {
            yield return StartCoroutine(ExecutarBarragem(quantidadeBarragem2, danoBarragem2));
        }
        else
        {
            yield return StartCoroutine(ExecutarOndas(quantidadeOndas2, danoOndas2));
        }

        executandoAtaque = false;
        Debug.Log("Ataque Segunda Barreira executado!");
    }

    IEnumerator ExecutarAtaqueTerceiraBarreira()
    {
        executandoAtaque = true;
        tempoUltimoAtaque = Time.time;

        // Escolhe aleatoriamente entre Barragem (9 projéteis) ou Ondas (3 ondas)
        if (Random.Range(0f, 1f) < 0.5f) // 50% chance de cada
        {
            yield return StartCoroutine(ExecutarBarragem(quantidadeBarragem3, danoBarragem3));
        }
        else
        {
            yield return StartCoroutine(ExecutarOndas(quantidadeOndas3, danoOndas3));
        }

        executandoAtaque = false;
        Debug.Log("Ataque Terceira Barreira executado!");
    }

    IEnumerator ExecutarBarragem(int quantidade, float dano)
    {
        if (animator != null)
            animator.SetTrigger("AtaqueBarragem");

        yield return new WaitForSeconds(0.3f);

        // Lança projéteis em padrão circular ou direcionado
        for (int i = 0; i < quantidade; i++)
        {
            Vector3 direcaoAlvo;

            if (quantidade == 1)
            {
                // Ataque direto ao jogador
                direcaoAlvo = jogador.position;
            }
            else
            {
                // Padrão circular ao redor do jogador
                float angulo = (360f / quantidade) * i;
                Vector2 offset = new Vector2(
                    Mathf.Cos(angulo * Mathf.Deg2Rad) * 2f,
                    Mathf.Sin(angulo * Mathf.Deg2Rad) * 2f
                );
                direcaoAlvo = jogador.position + (Vector3)offset;
            }

            LancarProjetilBarragem(direcaoAlvo, dano);

            yield return new WaitForSeconds(0.1f); // Pequeno delay entre projéteis
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ExecutarOndas(int quantidade, float dano)
    {
        if (animator != null)
            animator.SetTrigger("AtaqueOndas");

        yield return new WaitForSeconds(0.5f);

        for (int i = 0; i < quantidade; i++)
        {
            LancarOnda(dano);
            yield return new WaitForSeconds(0.8f); // Delay entre ondas
        }

        yield return new WaitForSeconds(0.3f);
    }

    void IniciarFaseSupremo()
    {
        faseSupremo = true;
        barreiraAtiva = false;

        if (barreira != null)
            barreira.SetActive(false);

        if (animator != null)
            animator.SetTrigger("FaseSupremo");

        // CORREÇÃO: Inicia imediatamente o ataque supremo
        StartCoroutine(ExecutarEstrelaSuprema());
    }

    IEnumerator ExecutarEstrelaSuprema()
    {
        executandoAtaque = true;

        // Teletransporta para o centro da arena
        if (pontosArena.Length > 0)
        {
            Transform centroArena = pontosArena[pontosArena.Length / 2]; // Assume que o centro está no meio do array
            yield return StartCoroutine(Teletransportar(centroArena.position));
        }

        // Aviso ao jogador
        Debug.Log("ATENÇÃO: Estrela Cadente Suprema carregando!");
        if (animator != null)
            animator.SetTrigger("CarregandoSupremo");

        // CORREÇÃO: Posiciona a área suprema na posição do jogador ANTES do delay
        if (areaSupremo != null && jogador != null)
        {
            areaSupremo.transform.position = jogador.position;
            areaSupremo.SetActive(true); // Mostra indicador visual

            // Configura o dano mas não ativa a hitbox ainda
            HitboxBoss hb = areaSupremo.GetComponent<HitboxBoss>();
            if (hb != null)
            {
                hb.SetDano(danoSupremo);
                hb.enabled = false; // Desativa temporariamente
            }
        }

        // Mostra indicadores nos pontos da estrela
        for (int i = 0; i < pontosEstrela.Length && i < 5; i++) // Máximo 5 pontos para estrela
        {
            if (pontosEstrela[i] != null)
            {
                // Aqui você pode ativar efeitos visuais de aviso
                StartCoroutine(MostrarIndicadorPonto(pontosEstrela[i], tempoCarregamentoSupremo));
            }
        }

        yield return new WaitForSeconds(tempoCarregamentoSupremo);

        // CORREÇÃO: Executa o ataque supremo ativando a hitbox
        if (animator != null)
            animator.SetTrigger("EstrelaSuprema");

        if (areaSupremo != null)
        {
            HitboxBoss hb = areaSupremo.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.enabled = true; // Ativa a hitbox para causar dano
        }

        yield return new WaitForSeconds(duracaoSupremo);

        if (areaSupremo != null)
            areaSupremo.SetActive(false);

        // Volta ao estado normal com chance de repetir supremo
        yield return new WaitForSeconds(2f);

        // 30% chance de usar supremo novamente, senão volta para barreira 3
        if (Random.Range(0f, 1f) < 0.3f && faseSupremo)
        {
            yield return new WaitForSeconds(3f); // Cooldown maior
            StartCoroutine(ExecutarEstrelaSuprema());
        }
        else
        {
            barreiraAtual = 3;
            faseSupremo = false;
            AtivarBarreira();
        }

        executandoAtaque = false;
    }

    IEnumerator MostrarIndicadorPonto(Transform ponto, float duracao)
    {
        // Aqui você pode criar efeitos visuais de aviso
        // Por exemplo, uma luz piscando ou partículas
        Debug.Log($"Ponto da estrela em {ponto.position} será atingido!");

        yield return new WaitForSeconds(duracao);

        // CORREÇÃO: Cria projétil do ponto da estrela em direção ao jogador
        if (jogador != null)
        {
            LancarProjetilDosPontos(ponto.position, jogador.position, danoSupremo);
        }
    }

    void LancarProjetilDosPontos(Vector3 origem, Vector3 alvo, float dano)
    {
        if (projetilBarragem != null)
        {
            // Cria o projétil na origem especificada (ponto da estrela)
            GameObject proj = Instantiate(projetilBarragem, origem, Quaternion.identity);

            // Configura direção do projétil
            Vector2 direcao = (alvo - origem).normalized;
            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
            if (rbProj != null)
            {
                rbProj.linearVelocity = direcao * 8f; // Velocidade do projétil
            }

            // Configura dano
            HitboxBoss hb = proj.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(dano);

            // Destrói após 3 segundos
            Destroy(proj, 3f);

            Debug.Log($"Projétil lançado do ponto {origem} em direção a {alvo} com dano {dano}");
        }
        else
        {
            Debug.LogWarning("ProjetilBarragem não está atribuído!");
        }
    }

    IEnumerator Teletransportar(Vector3 posicaoDestino)
    {
        if (animator != null)
            animator.SetTrigger("Teletransporte");

        // Efeito visual de desaparecimento
        yield return new WaitForSeconds(0.2f);

        // Move para nova posição
        transform.position = posicaoDestino;

        // Efeito visual de aparecimento
        yield return new WaitForSeconds(0.3f);

        Debug.Log("Teletransporte concluído!");
    }

    void LancarProjetilBarragem(Vector3 alvo, float dano)
    {
        if (projetilBarragem != null)
        {
            // CORREÇÃO: Cria o projétil um pouco à frente do boss para evitar colisão
            Vector2 direcaoLancamento = (alvo - transform.position).normalized;
            Vector3 posicaoLancamento = transform.position + (Vector3)direcaoLancamento * 1f;

            GameObject proj = Instantiate(projetilBarragem, posicaoLancamento, Quaternion.identity);

            // Configura direção do projétil
            Vector2 direcao = (alvo - posicaoLancamento).normalized;
            Rigidbody2D rbProj = proj.GetComponent<Rigidbody2D>();
            if (rbProj != null)
            {
                rbProj.linearVelocity = direcao * 8f; // Velocidade do projétil
            }

            // Configura dano
            HitboxBoss hb = proj.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(dano);

            // Destrói após 3 segundos
            Destroy(proj, 3f);

            Debug.Log($"Projétil lançado em direção a {alvo} com dano {dano}");
        }
        else
        {
            Debug.LogWarning("ProjetilBarragem não está atribuído!");
        }
    }

    void LancarOnda(float dano)
    {
        if (projetilOnda != null)
        {
            // Cria onda que se expande em direção ao jogador
            Vector3 posicaoOnda = transform.position;
            Vector2 direcaoOnda = (jogador.position - transform.position).normalized;

            GameObject onda = Instantiate(projetilOnda, posicaoOnda, Quaternion.identity);

            // Configura movimento da onda
            Rigidbody2D rbOnda = onda.GetComponent<Rigidbody2D>();
            if (rbOnda != null)
            {
                rbOnda.linearVelocity = direcaoOnda * 6f;
            }

            // Configura dano
            HitboxBoss hb = onda.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(dano);

            Destroy(onda, 4f);

            Debug.Log($"Onda lançada com dano {dano}");
        }
        else
        {
            Debug.LogWarning("ProjetilOnda não está atribuído!");
        }
    }

    // CORREÇÃO: Método de virar melhorado que não afeta os símbolos
    void VirarParaJogador()
    {
        if (jogador == null) return;

        Vector2 direcao = (jogador.position - transform.position).normalized;

        // Controla apenas o sprite do boss, não os objetos filhos (símbolos)
        if (direcao.x > 0 && !facingRight)
        {
            facingRight = true;
            FlipSprite();
        }
        else if (direcao.x < 0 && facingRight)
        {
            facingRight = false;
            FlipSprite();
        }
    }

    void FlipSprite()
    {
        // CORREÇÃO: Vira apenas o sprite renderer do boss, não o transform inteiro
        SpriteRenderer sr = GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            sr.flipX = !facingRight;
        }

        // Alternativa: se usar transform, usar apenas no sprite principal
        // transform.localScale = new Vector3(facingRight ? 1 : -1, 1, 1);
    }

    public override void EnemyHit(float dano, Vector2 direcao, float forca)
    {
        // Se tem barreira ativa, não recebe dano
        if (barreiraAtiva)
        {
            Debug.Log("Ataque bloqueado pela barreira mágica!");
            return;
        }

        base.EnemyHit(dano, direcao, forca);

        if (health <= 0)
        {
            Morrer();
        }
    }

    void Morrer()
    {
        executandoAtaque = false;
        barreiraAtiva = false;
        faseSupremo = false;

        if (barreira != null)
            barreira.SetActive(false);
        if (areaSupremo != null)
            areaSupremo.SetActive(false);

        foreach (GameObject simbolo in simbolosBarreira)
        {
            if (simbolo != null)
                simbolo.SetActive(false);
        }

        if (animator != null)
            animator.SetTrigger("Morte");

        Collider2D col = GetComponent<Collider2D>();
        if (col != null)
            col.enabled = false;

        Debug.Log("Rainha Nyarla derrotada!");
        Destroy(gameObject, 3f);
    }

    // Para debug
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaqueMagico);

        // Mostra pontos da estrela suprema
        if (pontosEstrela != null)
        {
            Gizmos.color = Color.red;
            foreach (Transform ponto in pontosEstrela)
            {
                if (ponto != null)
                    Gizmos.DrawWireSphere(ponto.position, 1f);
            }
        }
    }
}