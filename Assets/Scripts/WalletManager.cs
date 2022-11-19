using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class WalletManager : MonoBehaviour
{
    public int emojicoins = 1000;
    private GameObject wallet;
    
    public static WalletManager Instance
    {
        get;
        private set;
    }
    private void Awake()
    {
        //Singleton Pattern
        if (Instance != this && Instance != null)
        {
            Destroy(this);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this); //GameManager
    }
    
    // Start is called before the first frame update
    void Start()
    {
        wallet = GameObject.Find("Wallet");
        updateWallet();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void updateWallet()
    {
        wallet.GetComponent<TMP_Text>().text = "Emoji Coins: " + emojicoins;
    }
}
