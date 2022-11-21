using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.UI;

using Firebase.Storage;
using Firebase.Extensions;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
//using UnityEditor.VersionControl;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;
using Task = System.Threading.Tasks.Task;

public class FirebaseStorageController : MonoBehaviour
{
    private FirebaseStorage _firebaseInstance;
    [SerializeField] private GameObject ThumbnailPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> instantiatedPrefabs;
    public List<AssetData> DownloadedAssetData;
    private XDocument manifest;

    public GameObject buySpecialEffect;
    
    public enum DownloadType
    {
        Manifest, Thumbnail
    }
    
    public enum GameItemType
    {
        Background,
        Emoji,
        Sfx
    }
    
    public static FirebaseStorageController Instance
    {
        get;
        private set;
    }

    private void Awake()
    {
        //Singleton Pattern
        if (Instance != this && Instance != null)
        {
            Destroy(gameObject);
        }
        else
        {
            Instance = this;
        }
        DontDestroyOnLoad(this); //GameManager
        _firebaseInstance = FirebaseStorage.DefaultInstance;
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += prepScene;
    }

    private void Start()
    {
        //to debug
        //PlayerPrefs.SetInt("PolicyInt", 0);
    }

    private void prepScene(Scene scene, LoadSceneMode mode)
    {
        //makes sure they are ONLY instantiated in the store scene
        if (scene == SceneManager.GetSceneByName("Store_Scene") && mode == LoadSceneMode.Single)
        {
            prepStore();
        }
        //content is ONLY shown in the game scene
        else if (scene == SceneManager.GetSceneByName("Game_Scene") && mode == LoadSceneMode.Single)
        {
            prepGame();
        }
    }

    public void prepGame()
    {
        GameObject bg1 = GameObject.Find("BG1");
        GameObject bg2 = GameObject.Find("BG2");
        GameObject emoji = GameObject.Find("Emoji");
        GameObject sfx = GameObject.Find("SpecialEffect");
        
        foreach (AssetData asset in DownloadedAssetData)
        {
            if (asset.Owned)
            {
                switch (asset.Name)
                {
                    case "Background 1":
                        StartCoroutine(LoadSpriteInGame(bg1, "Content/bg1.png", GameItemType.Background));
                        break;
            
                    case "Background 2":
                        StartCoroutine(LoadSpriteInGame(bg2, "Content/bg2.png", GameItemType.Background));
                        break;
            
                    case "Emoji skin pack":
                        StartCoroutine(LoadSpriteInGame(emoji, "Content/emoji_pack.png", GameItemType.Emoji));
                        break;
            
                    case "Special effects pack":
                        break;
            
                    default:
                        break;
                }
            }
        }
    }

    IEnumerator LoadSpriteInGame(GameObject spriteContainer, string fileName, GameItemType itemType)
    {
        spriteContainer.GetComponent<SpriteRenderer>().enabled = true;

        string fileUrl = Path.Combine(Application.streamingAssetsPath, fileName);
        byte[] imgData = File.ReadAllBytes(fileUrl);
        Texture2D tex = new Texture2D(2, 2);
        print("==== " + fileUrl);
        
        if (fileUrl.Contains("://") || fileUrl.Contains(":///"))
        {
            UnityWebRequest www = UnityWebRequest.Get(fileUrl);
            yield return www.SendWebRequest();
            imgData = www.downloadHandler.data;
        }
        else
        {
            imgData = File.ReadAllBytes(fileUrl);
        }
        tex.LoadImage(imgData);

        switch (itemType)
        {
            case GameItemType.Background:
                SetSprite(spriteContainer, tex);
                break;
            case GameItemType.Emoji:
                StartCoroutine(SetEmoji(spriteContainer, tex));
                break;
            default:
                break;
        }
    }

    private void SetSprite(GameObject spriteContainer, Texture2D tex)
    {
        Sprite sprite = Sprite.Create(tex, new Rect(0.0f, 0.0f, tex.width, tex.height), Vector2.one * 0.5f, 100.0f);
                        
        spriteContainer.GetComponent<SpriteRenderer>().sprite = sprite;
    }

    IEnumerator SetEmoji(GameObject spriteContainer, Texture2D tex)
    {
        while (true)
        {
            var rect = new Rect(72 * Random.Range(0, 16), 72 * Random.Range(0, 5), tex.width / 16, tex.height / 5);
            Sprite sprite = Sprite.Create(tex, rect, Vector2.one * 0.5f);

            spriteContainer.GetComponent<SpriteRenderer>().sprite = sprite;

            yield return new WaitForSeconds(1);
        }
    }
    
    public void prepStore()
    {
        //updates the wallet and the prefabs are instantiated 
        WalletManager.Instance.updateWallet();

        instantiatedPrefabs = new List<GameObject>();
        _thumbnailContainer = GameObject.Find("Thumbnail_Container");
        
        if (DownloadedAssetData == null)
        {
            DownloadFileAsync("gs://emoji-junkie-dlc-store-fb6f2.appspot.com/manifest.xml", DownloadType.Manifest);
        }
        else
        {
            foreach (AssetData asset in DownloadedAssetData)
            {
                DownloadFileAsync(asset.ThumbnailUrl, DownloadType.Thumbnail, asset);
            }
        }
    }

    public void DownloadFileAsync(string url, DownloadType filetype, [Optional] AssetData assetRef){
        StorageReference storageRef =  _firebaseInstance.GetReferenceFromUrl(url);
        
        // Download in memory with a maximum allowed size of 32MB (32 * 1024 * 1024 bytes)
        const long maxAllowedSize = 32 * 1024 * 1024;
        storageRef.GetBytesAsync(maxAllowedSize).ContinueWithOnMainThread(task => {
            if (task.IsFaulted || task.IsCanceled) {
                Debug.LogException(task.Exception);
                // Uh-oh, an error occurred!
            }
            else {
                Debug.Log($"{storageRef.Name} finished downloading!");
                if (filetype == DownloadType.Manifest)
                {
                    //Load manifest
                    StartCoroutine(LoadManifest(task.Result));
                }else if (filetype == DownloadType.Thumbnail)
                {
                    //Load the image into Unity
                    StartCoroutine(LoadImageContainer(task.Result, assetRef));
                }
            }
        });
    }

    IEnumerator LoadManifest(byte[] byteArr)
    {
        manifest = XDocument.Parse(System.Text.Encoding.UTF8.GetString(byteArr));
        DownloadedAssetData = new List<AssetData>();
        //Get the urls inside the manifest file
        //Download each url and display to the user
        foreach (XElement xElement in manifest.Root.Elements())
        {
            string id = xElement.Element("id")?.Value;
            string name = xElement.Element("name")?.Value;
            string thumbnailUrl = xElement.Element("img")?.Element("url")?.Value;
            string priceStr = xElement.Element("price")?.Element("value")?.Value;
            int price = (priceStr != null) ? int.Parse(priceStr) : 0;
            bool owned = false;
            AssetData newAsset = new AssetData(id, name, thumbnailUrl, price, owned);
            DownloadedAssetData.Add(newAsset);
            DownloadFileAsync(newAsset.ThumbnailUrl, DownloadType.Thumbnail, newAsset);
        }
        yield return null;
    }

    IEnumerator LoadImageContainer(byte[] byteArr, AssetData assetRef)
    {
        Texture2D imageTex = new Texture2D(1, 1);
        imageTex.LoadImage(byteArr);
        //Instantiate a new prefab
        GameObject thumbnailPrefab =
            Instantiate(ThumbnailPrefab, _thumbnailContainer.transform.position, 
                Quaternion.identity,_thumbnailContainer.transform);
        thumbnailPrefab.name = "Thumbnail_" + instantiatedPrefabs.Count;
        //Load the image to that prefab
        thumbnailPrefab.transform.GetChild(2).GetComponent<RawImage>().texture = imageTex;
        thumbnailPrefab.transform.GetChild(3).GetComponent<TMP_Text>().text = assetRef.Name;
        thumbnailPrefab.transform.GetChild(4).GetComponent<TMP_Text>().text = assetRef.Price.ToString();

        instantiatedPrefabs.Add(thumbnailPrefab);
        
        //loaded for each item loaded into the scene (4 times)
        checkIfItemsAreAffordable(assetRef);
        checkIfItemsAreOwned(assetRef);
        yield return null;
    }

    public void checkIfItemsAreAffordable(AssetData assetRef)
    {
        if (!assetRef.Owned)
        {
            //goes through store items, makes ref to the below
            foreach (GameObject nameLabel in GameObject.FindGameObjectsWithTag("ItemName"))
            {   
                // and finds the one matching the current assetRef
                if (nameLabel.GetComponent<TMP_Text>().text == assetRef.Name)
                {
                    //ref parent container
                    Transform itemContainer = nameLabel.transform.parent;
                    //ref price object
                    Transform priceObject = itemContainer.GetChild(4);
                    //ref text of button
                    TMP_Text btnTextComponent = itemContainer.GetChild(6).GetChild(0).GetComponent<TMP_Text>();
                    //parses to integer so it can compare to emojicoin count to see if its larger and also check if the text of the button is set to "BUY"
                    //the text of the button turns red only if the price is higher than the wallet value and if the item is locked
                    if (int.Parse(priceObject.GetComponent<TMP_Text>().text) > WalletManager.Instance.emojicoins)
                    {
                        btnTextComponent.color = Color.red;
                    }
                    else
                    {
                        btnTextComponent.color = Color.black;
                    }
                }
            }
        }
    }

    public void checkIfItemsAreOwned(AssetData assetRef)
    {
        // checks if items are owned and then styles appropriately
        if (assetRef.Owned)
        {
            // goes through all name labels
            foreach (GameObject nameLabel in GameObject.FindGameObjectsWithTag("ItemName"))
            {   
                // and finds the one matching the current assetRef
                if (nameLabel.GetComponent<TMP_Text>().text == assetRef.Name)
                {
                    //ref the elements
                    Transform itemContainer = nameLabel.transform.parent;
                    
                    Transform unlocked = itemContainer.GetChild(1);
                    Transform locked = itemContainer.GetChild(0);
                    Transform itemButton = itemContainer.GetChild(6);

                    //style the container
                    locked.GetComponent<RawImage>().enabled = false;
                    
                    unlocked.GetComponent<RawImage>().enabled = true;
                    
                    itemButton.GetComponent<Button>().interactable = false;
                    itemButton.GetChild(0).GetComponent<TMP_Text>().text = "OWNED";
                }
            }

        }
    }

    public void downloadContent(string itemName, Transform progressBar, Transform progressFill)
    {
        // set url to the url of the content relevant to the button clicked
        int itemID = -1; //default -1 so it doesnt download itemID 0 by accident if a name does not match
        switch (itemName)
        {
            case "Background 1":
                itemID = 0;
                break;
            
            case "Background 2":
                itemID = 1;
                break;
            
            case "Emoji skin pack":
                itemID = 2;
                break;
            
            case "Special effects pack":
                itemID = 3;
                break;
            
            default:
                break;
        }
        string contentUrl = manifest.Root.Elements()?.ElementAt(itemID)?.Element("content")?.Element("url")?.Value;
        
        print(contentUrl.Substring(54));
        StorageReference storageRef =  _firebaseInstance.GetReferenceFromUrl(contentUrl);   
        
        // Create local filesystem URL
        string localUrl = Application.streamingAssetsPath + "/Content/" + contentUrl.Substring(54);
        //string localUrl = Application.streamingAssetsPath + "/Content/" + itemUrl;

        // Download to the local filesystem
        Task task = storageRef.GetFileAsync(localUrl, new StorageProgress<DownloadState>(state => {
            // called periodically during the download
            /*Debug.Log(String.Format(
                "Progress: {0} of {1} bytes transferred.",
                state.BytesTransferred,
                state.TotalByteCount
            ));*/
            
            //progress bar filling up
            new WaitForEndOfFrame();
            progressFill.GetComponent<RectTransform>().localScale = new Vector3((float)state.BytesTransferred/(float)state.TotalByteCount, progressFill.localScale.y, progressFill.localScale.z);

        }), CancellationToken.None);

        task.ContinueWithOnMainThread(resultTask => {
            if (!task.IsFaulted && !task.IsCanceled) {
                Debug.Log("File downloaded." + Application.streamingAssetsPath);
                //Debug.Log("File downloaded." + Application.streamingAssetsPath);
                
                // find relevant asset and set to owned
                foreach (AssetData asset in DownloadedAssetData)
                {
                    if (itemName == asset.Name)
                    {
                        //set to owned
                        asset.Owned = true;
                        
                        //change wallet balance
                        WalletManager.Instance.emojicoins -= asset.Price;
                    }
                    
                    //update wallet display
                    WalletManager.Instance.updateWallet();
                    
                    checkIfItemsAreAffordable(asset);
                }
                
                // do special effect
                Instantiate(buySpecialEffect, progressBar.position, Quaternion.identity);
                
                // disable progress bar
                progressBar.GetComponent<Image>().enabled = false;
                progressFill.GetComponent<Image>().enabled = false;
            }
            else
            {
                Debug.LogException(task.Exception);
                // Uh-oh, an error occurred!
            }
        });
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= prepScene;
    }
}
