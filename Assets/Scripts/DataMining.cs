using System;
using System.Collections;
using System.Collections.Generic;
using Firebase.Extensions;
using Firebase.Firestore;
using UnityEngine;
using UnityEngine.UI;

public class DataMining : MonoBehaviour
{
    private FirebaseFirestore _db;
    private DocumentReference _docRef;
    
    public enum ActionType
    {
        StoreButtonClicked, GameButtonClicked
    }

    private readonly String _anonymisedUserId = "on923oinsafni2";

    private void Awake()
    {
        _db = FirebaseFirestore.DefaultInstance;
        _docRef = _db.Collection("data-mining").Document();
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
}
