using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : MonoBehaviour
{
    //public SceneFader sceneFader; Esta classe SceneFader n foi criada antes, assim não sendo possivel fazer o codigo do UI gameover

    public static UIManager instance;

    [SerializeField] GameObject deathScreem;

    private void Awake()
    {
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        else
        {
            instance = this;
        }
    }

    public IEnumerator ActivatsathScreenk()
    {
        yield return new WaitForSeconds(0.8f);
        
    }
}
