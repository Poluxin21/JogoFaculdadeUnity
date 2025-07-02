using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class CavaleiroNegro : Enemy
{
    [Header("Configurações de Combate")]
    public float distanciaDeteccao = 10f;
    public float distanciaAtaque = 2f;
    public float distanciaAtaqueLongo = 6f;
    public float tempoEntreAtaques = 2f;
    public float danoAtaque = 2f;
    public float danoAtaqueEspecial = 3f;
    
    [Header("Timing das Hitboxes")]
    public float tempoAtivarHitboxCorte = 0.5f;
    public float tempoAtivarHitboxCruzado = 0.6f;
    public float tempoAtivarHitboxTremor = 0.8f;
    public float duracaoHitbox = 0.3f;
    
    [Header("Padrões de Ataque")]
    [Range(0f, 1f)]
    public float chanceAtaqueCorte = 0.4f;
    [Range(0f, 1f)]
    public float chanceAtaqueCruzado = 0.3f;
    [Range(0f, 1f)]
    public float chanceAtaqueTremor = 0.2f;
    [Range(0f, 1f)]
    public float chanceDelimitarArena = 0.1f;
    
    [Header("Referências")]
    public Transform jogador;
    public Animator animator;
    public GameObject hitboxAtaqueCorte;
    public GameObject hitboxAtaqueCruzado;
    public GameObject hitboxAtaqueTremor;
    public GameObject areaDelimitacao;

    private float tempoUltimoAtaque;
    private bool atacando;
    private bool hitboxAtiva;
    private PadraoAtaque padraoAtual;
    private List<PadraoAtaque> padroesDisponiveis;

    public enum PadraoAtaque
    {
        AtaqueCorte,
        AtaqueCruzado,
        AtaqueTremor,
        DelimitarArena
    }

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

        // Configura as hitboxes
        ConfigurarHitboxes();
        
        // Inicializa os padrões disponíveis
        InicializarPadroes();

        tempoUltimoAtaque = -tempoEntreAtaques;
    }

    void ConfigurarHitboxes()
    {
        // Configura hitbox do ataque de corte
        if (hitboxAtaqueCorte != null)
        {
            hitboxAtaqueCorte.SetActive(false);
            HitboxBoss hb = hitboxAtaqueCorte.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(danoAtaque);
        }

        // Configura hitbox do ataque cruzado
        if (hitboxAtaqueCruzado != null)
        {
            hitboxAtaqueCruzado.SetActive(false);
            HitboxBoss hb = hitboxAtaqueCruzado.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(danoAtaqueEspecial);
        }

        // Configura hitbox do ataque tremor
        if (hitboxAtaqueTremor != null)
        {
            hitboxAtaqueTremor.SetActive(false);
            HitboxBoss hb = hitboxAtaqueTremor.GetComponent<HitboxBoss>();
            if (hb != null)
                hb.SetDano(danoAtaqueEspecial);
        }

        // Configura área de delimitação
        if (areaDelimitacao != null)
        {
            areaDelimitacao.SetActive(false);
        }
    }

    void InicializarPadroes()
    {
        padroesDisponiveis = new List<PadraoAtaque>();
        
        // Adiciona padrões baseado nas chances configuradas
        int quantidadeCorte = Mathf.RoundToInt(chanceAtaqueCorte * 10);
        int quantidadeCruzado = Mathf.RoundToInt(chanceAtaqueCruzado * 10);
        int quantidadeTremor = Mathf.RoundToInt(chanceAtaqueTremor * 10);
        int quantidadeDelimitar = Mathf.RoundToInt(chanceDelimitarArena * 10);

        for (int i = 0; i < quantidadeCorte; i++)
            padroesDisponiveis.Add(PadraoAtaque.AtaqueCorte);
        
        for (int i = 0; i < quantidadeCruzado; i++)
            padroesDisponiveis.Add(PadraoAtaque.AtaqueCruzado);
        
        for (int i = 0; i < quantidadeTremor; i++)
            padroesDisponiveis.Add(PadraoAtaque.AtaqueTremor);
        
        for (int i = 0; i < quantidadeDelimitar; i++)
            padroesDisponiveis.Add(PadraoAtaque.DelimitarArena);
    }

    protected override void Update()
    {
        base.Update();

        if (jogador == null || health <= 0)
            return;

        float distancia = Vector2.Distance(transform.position, jogador.position);

        // Se está atacando, não faz mais nada
        if (atacando)
        {
            rb.linearVelocity = Vector2.zero;
            return;
        }

        // Verifica se pode atacar
        if (Time.time - tempoUltimoAtaque >= tempoEntreAtaques)
        {
            if (distancia <= distanciaAtaque)
            {
                EscolherEExecutarPadrao();
            }
            else if (distancia <= distanciaAtaqueLongo)
            {
                // Ataques de longo alcance (Tremor, Delimitar Arena)
                EscolherEExecutarPadraoLongo();
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
        else if (distancia <= distanciaDeteccao)
        {
            PerseguirJogador();
        }
        else
        {
            Parar();
        }
    }

    void EscolherEExecutarPadrao()
    {
        if (padroesDisponiveis.Count == 0)
            return;

        padraoAtual = padroesDisponiveis[Random.Range(0, padroesDisponiveis.Count)];
        StartCoroutine(ExecutarPadrao(padraoAtual));
    }

    void EscolherEExecutarPadraoLongo()
    {
        // Para ataques de longo alcance, prioriza Tremor e Delimitar Arena
        List<PadraoAtaque> padroesLongos = new List<PadraoAtaque>();
        
        if (chanceAtaqueTremor > 0)
            padroesLongos.Add(PadraoAtaque.AtaqueTremor);
        if (chanceDelimitarArena > 0)
            padroesLongos.Add(PadraoAtaque.DelimitarArena);

        if (padroesLongos.Count > 0)
        {
            padraoAtual = padroesLongos[Random.Range(0, padroesLongos.Count)];
            StartCoroutine(ExecutarPadrao(padraoAtual));
        }
    }

    void PerseguirJogador()
    {
        Vector2 direcao = (jogador.position - transform.position).normalized;
        rb.linearVelocity = direcao * speed;
        
        // Vira o sprite na direção do jogador
        if (direcao.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direcao.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        
        animator.SetBool("Andando", true);
    }

    void Parar()
    {
        rb.linearVelocity = Vector2.zero;
        animator.SetBool("Andando", false);
    }

    IEnumerator ExecutarPadrao(PadraoAtaque padrao)
    {
        atacando = true;
        Parar();
        
        // Vira na direção do jogador antes de atacar
        VirarParaJogador();

        tempoUltimoAtaque = Time.time;

        switch (padrao)
        {
            case PadraoAtaque.AtaqueCorte:
                yield return StartCoroutine(ExecutarAtaqueCorte());
                break;
            case PadraoAtaque.AtaqueCruzado:
                yield return StartCoroutine(ExecutarAtaqueCruzado());
                break;
            case PadraoAtaque.AtaqueTremor:
                yield return StartCoroutine(ExecutarAtaqueTremor());
                break;
            case PadraoAtaque.DelimitarArena:
                yield return StartCoroutine(ExecutarDelimitarArena());
                break;
        }

        atacando = false;
    }

    IEnumerator ExecutarAtaqueCorte()
    {
        animator.SetTrigger("AtaqueCorte");
        
        yield return new WaitForSeconds(tempoAtivarHitboxCorte);
        
        if (VerificarDistanciaAtaque(distanciaAtaque))
        {
            AtivarHitbox(hitboxAtaqueCorte);
            yield return new WaitForSeconds(duracaoHitbox);
            DesativarHitbox(hitboxAtaqueCorte);
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator ExecutarAtaqueCruzado()
    {
        animator.SetTrigger("AtaqueCorteCruzado");
        
        yield return new WaitForSeconds(tempoAtivarHitboxCruzado);
        
        if (VerificarDistanciaAtaque(distanciaAtaque))
        {
            AtivarHitbox(hitboxAtaqueCruzado);
            yield return new WaitForSeconds(duracaoHitbox);
            DesativarHitbox(hitboxAtaqueCruzado);
        }

        yield return new WaitForSeconds(0.7f);
    }

    IEnumerator ExecutarAtaqueTremor()
    {
        animator.SetTrigger("AtaqueTremor");
        
        yield return new WaitForSeconds(tempoAtivarHitboxTremor);
        
        // Ataque tremor não precisa verificar distância específica
        AtivarHitbox(hitboxAtaqueTremor);
        
        // Efeito de tremor na tela (opcional)
        StartCoroutine(EfeitoTremor());
        
        yield return new WaitForSeconds(duracaoHitbox * 2); // Tremor dura mais
        DesativarHitbox(hitboxAtaqueTremor);

        yield return new WaitForSeconds(1f);
    }

    IEnumerator ExecutarDelimitarArena()
    {
        animator.SetTrigger("DelimitarArena");
        
        yield return new WaitForSeconds(0.5f);
        
        if (areaDelimitacao != null)
        {
            areaDelimitacao.SetActive(true);
            yield return new WaitForSeconds(3f); // Mantém a delimitação por 3 segundos
            areaDelimitacao.SetActive(false);
        }

        yield return new WaitForSeconds(0.5f);
    }

    IEnumerator EfeitoTremor()
    {
        // Efeito visual de tremor na câmera (se tiver script de câmera)
        Camera cam = Camera.main;
        if (cam != null)
        {
            Vector3 posicaoOriginal = cam.transform.position;
            float duracao = 0.5f;
            float intensidade = 0.1f;
            
            for (float t = 0; t < duracao; t += Time.deltaTime)
            {
                cam.transform.position = posicaoOriginal + (Vector3)Random.insideUnitCircle * intensidade;
                yield return null;
            }
            
            cam.transform.position = posicaoOriginal;
        }
    }

    void VirarParaJogador()
    {
        if (jogador == null) return;
        
        Vector2 direcao = (jogador.position - transform.position).normalized;
        if (direcao.x > 0)
            transform.localScale = new Vector3(Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
        else if (direcao.x < 0)
            transform.localScale = new Vector3(-Mathf.Abs(transform.localScale.x), transform.localScale.y, transform.localScale.z);
    }

    bool VerificarDistanciaAtaque(float distanciaMaxima)
    {
        if (jogador == null) return false;
        float distanciaAtual = Vector2.Distance(transform.position, jogador.position);
        return distanciaAtual <= distanciaMaxima + 1f; // Margem extra
    }

    void AtivarHitbox(GameObject hitbox)
    {
        if (hitbox != null && !hitboxAtiva)
        {
            hitbox.SetActive(true);
            hitboxAtiva = true;
            Debug.Log($"Hitbox {hitbox.name} ativada!");
        }
    }

    void DesativarHitbox(GameObject hitbox)
    {
        if (hitbox != null && hitboxAtiva)
        {
            hitbox.SetActive(false);
            hitboxAtiva = false;
            Debug.Log($"Hitbox {hitbox.name} desativada!");
        }
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
        if (hitboxAtaqueCorte != null)
            hitboxAtaqueCorte.SetActive(false);
        if (hitboxAtaqueCruzado != null)
            hitboxAtaqueCruzado.SetActive(false);
        if (hitboxAtaqueTremor != null)
            hitboxAtaqueTremor.SetActive(false);
        if (areaDelimitacao != null)
            areaDelimitacao.SetActive(false);
        
        hitboxAtiva = false;
    }

    void Morrer()
    {
        atacando = false;
        rb.linearVelocity = Vector2.zero;
        animator.SetTrigger("Morte");
        
        GetComponent<Collider2D>().enabled = false;
        
        Destroy(gameObject, 2f);
    }

    // Para debug - mostra as áreas de detecção no editor
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, distanciaDeteccao);
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaque);
        
        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, distanciaAtaqueLongo);
    }
}