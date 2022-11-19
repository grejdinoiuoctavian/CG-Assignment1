using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ItemButtonManager : MonoBehaviour
{
    private Transform thumbnailPrefab;
    private Transform locked;
    private Transform unlocked;
    private Transform price;
    

    void Start()
    {
        thumbnailPrefab = transform.parent;
        locked = thumbnailPrefab.GetChild(0);
        unlocked = thumbnailPrefab.GetChild(1);
        price = thumbnailPrefab.GetChild(4);
    }
    
    public void handleBuy()
    {
        //handle price
        
        
        //handle button style
        gameObject.GetComponent<Button>().interactable = false;
        transform.GetChild(0).GetComponent<TMP_Text>().text = "OWNED";

        //handle border
        unlocked.GetComponent<RawImage>().enabled = true;
        locked.GetComponent<RawImage>().enabled = false;
        
    }
}
