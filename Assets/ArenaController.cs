using System.Collections.Generic;
using UnityEngine;

public class ArenaController : MonoBehaviour
{
    [Header("Configurações da Arena")]
    public Transform limiteSuperior;
    public Transform limiteInferior;
    public Transform limiteEsquerdo;
    public Transform limiteDireito;
    public GameObject areaDanoTemplate;
    
    private Vector2 tamanhoOriginal;
    private Vector2 posicaoOriginal;
    private List<GameObject> areasRemovidas = new List<GameObject>();
    private bool arenaReduzida = false;
    
    void Start()
    {
        // Salva as configurações originais da arena
        tamanhoOriginal = new Vector2(
            limiteDireito.position.x - limiteEsquerdo.position.x,
            limiteSuperior.position.y - limiteInferior.position.y
        );
        
        posicaoOriginal = transform.position;
    }
    
    public void ReduzirArena(float fatorReducao)
    {
        if (arenaReduzida) return;
        
        Vector2 novoTamanho = tamanhoOriginal * fatorReducao;
        Vector2 reducao = (tamanhoOriginal - novoTamanho) / 2f;
        
        // Cria áreas de dano nas extremidades
        CriarAreaDano(new Vector2(limiteEsquerdo.position.x - reducao.x, posicaoOriginal.y), 
                      new Vector2(reducao.x * 2, tamanhoOriginal.y)); // Esquerda
        
        CriarAreaDano(new Vector2(limiteDireito.position.x + reducao.x, posicaoOriginal.y), 
                      new Vector2(reducao.x * 2, tamanhoOriginal.y)); // Direita
        
        CriarAreaDano(new Vector2(posicaoOriginal.x, limiteInferior.position.y - reducao.y), 
                      new Vector2(novoTamanho.x, reducao.y * 2)); // Baixo
        
        CriarAreaDano(new Vector2(posicaoOriginal.x, limiteSuperior.position.y + reducao.y), 
                      new Vector2(novoTamanho.x, reducao.y * 2)); // Cima
        
        // Atualiza os limites da arena
        limiteEsquerdo.position = new Vector3(limiteEsquerdo.position.x + reducao.x, limiteEsquerdo.position.y, 0);
        limiteDireito.position = new Vector3(limiteDireito.position.x - reducao.x, limiteDireito.position.y, 0);
        limiteInferior.position = new Vector3(limiteInferior.position.x, limiteInferior.position.y + reducao.y, 0);
        limiteSuperior.position = new Vector3(limiteSuperior.position.x, limiteSuperior.position.y - reducao.y, 0);
        
        arenaReduzida = true;
    }
    
    void CriarAreaDano(Vector2 posicao, Vector2 tamanho)
    {
        GameObject areaDano = Instantiate(areaDanoTemplate, posicao, Quaternion.identity);
        areaDano.transform.localScale = new Vector3(tamanho.x, tamanho.y, 1f);
        areasRemovidas.Add(areaDano);
        
        // Inicialmente invisível
        SpriteRenderer sr = areaDano.GetComponent<SpriteRenderer>();
        if (sr != null)
        {
            Color cor = sr.color;
            cor.a = 0.3f;
            sr.color = cor;
        }
    }
    
    public void AtivarDanoAreasRemovidas(float dano)
    {
        foreach (GameObject area in areasRemovidas)
        {
            if (area != null)
            {
                AreaDanoArena areaDanoScript = area.GetComponent<AreaDanoArena>();
                if (areaDanoScript != null)
                {
                    areaDanoScript.AtivarDano(dano);
                }
            }
        }
    }
}