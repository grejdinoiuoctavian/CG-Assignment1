using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class DataMining : MonoBehaviour
{
    private FirebaseFirestore _db;
    private DocumentReference _docRef;
    private string stringValue;
    
    public enum ActionType
    {
        StoreButtonClicked, GameButtonClicked, ItemButtonClicked
    }

    private String _anonymisedUserId = "on923oinsafni3";
    private int policyCode = 0;
    

    private void Awake()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _docRef = _db.Collection("data-mining").Document();
        SaveUserPlayerPref();
    }

    private void Start()
    {
        _anonymisedUserId = PlayerPrefs.GetString("UserToken","on923oinsafni3");
        Debug.Log(_anonymisedUserId);
    }

    public void SaveUserPlayerPref()
    {
        PlayerPrefs.SetString("UserToken", _anonymisedUserId);
    }

    public void RecordStoreBtnlClick()
    {
        Dictionary<string, object> city = new Dictionary<string, object>
        {
            { "User", _anonymisedUserId },
            { "Action", ActionType.StoreButtonClicked.ToString() },
            { "DateTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
        };
        _docRef.SetAsync(city).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Added {ActionType.StoreButtonClicked.ToString()} action to Firestore Emoji StoreBtn");
        });
    }
    
    public void RecordGameBtnlClick()
    {
        Dictionary<string, object> city = new Dictionary<string, object>
        {
            { "User", _anonymisedUserId },
            { "Action", ActionType.GameButtonClicked.ToString() },
            { "DateTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") }
        };
        _docRef.SetAsync(city).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Added {ActionType.GameButtonClicked.ToString()} action to Firestore Emoji GameBtn");
        });
    }
    
    public void RecordItemBtnlClick(GameObject buttonClicked)
    {
        Dictionary<string, object> city = new Dictionary<string, object>
        {
            { "User", _anonymisedUserId },
            { "Action", ActionType.ItemButtonClicked.ToString() },
            { "DateTime", DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss") },
            { "ItemBought", buttonClicked.transform.parent.GetChild(3).GetComponent<TMP_Text>().text }
        };
        
        _docRef.SetAsync(city).ContinueWithOnMainThread(task =>
        {
            Debug.Log($"Added {ActionType.ItemButtonClicked.ToString()} action to Firestore Emoji ItemBtn");
        });
    }
}
