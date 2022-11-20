using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.UI;

using Firebase.Storage;
using Firebase.Extensions;
using JetBrains.Annotations;
using TMPro;
using Unity.VisualScripting;
using UnityEngine.SceneManagement;

public class FirebaseStorageController : MonoBehaviour
{
    private FirebaseStorage _firebaseInstance;
    [SerializeField] private GameObject ThumbnailPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> instantiatedPrefabs;
    public List<AssetData> DownloadedAssetData;
    
    //public 
    
    public enum DownloadType
    {
        Manifest, Thumbnail
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
        SceneManager.sceneLoaded += prepStore;
    }

    private void Start()
    {
        //prepStore();
    }
    
    public void prepStore(Scene scene, LoadSceneMode mode)
    {
        if (scene == SceneManager.GetSceneByName("Store_Scene") && mode == LoadSceneMode.Single)
        {
            WalletManager.Instance.updateWallet();
            
            instantiatedPrefabs = new List<GameObject>();
            _thumbnailContainer = GameObject.Find("Thumbnail_Container");
            DownloadFileAsync("gs://emoji-junkie-dlc-store-fb6f2.appspot.com/manifest.xml", DownloadType.Manifest);
            //Get the urls inside the manifest file
            //Download each url and display to the user
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
        XDocument manifest = XDocument.Parse(System.Text.Encoding.UTF8.GetString(byteArr));
        DownloadedAssetData = new List<AssetData>();
        foreach (XElement xElement in manifest.Root.Elements())
        {
            string id = xElement.Element("id")?.Value;
            string name = xElement.Element("name")?.Value;
            string thumbnailUrl = xElement.Element("img")?.Element("url")?.Value;
            string priceStr = xElement.Element("price")?.Element("value")?.Value;
            int price = (priceStr != null) ? int.Parse(priceStr) : 0;
            AssetData newAsset = new AssetData(id, name, thumbnailUrl, price);
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
        checkIfItemsAreAffordable();
        yield return null;
    }

    public void checkIfItemsAreAffordable()
    {
        //goes through store items, makes ref to the below
        foreach (GameObject storeItem in instantiatedPrefabs)
        {
            //ref price object
            Transform priceObject = storeItem.transform.GetChild(4);
            //ref text of button
            TMP_Text btnTextComponent = storeItem.transform.GetChild(6).GetChild(0).GetComponent<TMP_Text>();
            //parses to integer so it can compare to emojicoin count to see if its larger and also check if the text of the button is set to "BUY"
            //the text of the button turns red only if the price is higher than the wallet value and if the item is locked
            if (int.Parse(priceObject.GetComponent<TMP_Text>().text) > WalletManager.Instance.emojicoins && btnTextComponent.text == "BUY")
            {
                btnTextComponent.color = Color.red;
            }
            else
            {
                btnTextComponent.color = Color.black;
            }
        }
    }

    public void downloadContent(string itemName, Transform progressBar, Transform progressFill)
    {
        // set url to the url of the content relevant to the button clicked
        string url = "gs://emoji-junkie-dlc-store-fb6f2.appspot.com/Content/";
        string itemUrl = "";
        switch (itemName)
        {
            case "Background 1":
                itemUrl = "bg1.png";
                break;
            
            case "Background 2":
                itemUrl = "bg2.png";
                break;
            
            case "Emoji skin pack":
                itemUrl = "emoji_pack.png";
                break;
            
            case "Special effects pack":
                itemUrl = "CFXR2 Shiny Item (Loop).prefab";
                break;
            
            default:
                break;
        }
        StorageReference storageRef =  _firebaseInstance.GetReferenceFromUrl(url + itemUrl);
        
        // Create local filesystem URL
        string localUrl = Application.dataPath + "/Content/" + itemUrl;
        //string localUrl = Application.streamingAssetsPath + "/Content/" + itemUrl;

        // Download to the local filesystem
        Task task = storageRef.GetFileAsync(localUrl,
            new StorageProgress<DownloadState>(state => {
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
                Debug.Log("File downloaded." + Application.dataPath);
                //Debug.Log("File downloaded." + Application.streamingAssetsPath);
                
                // do special effect
                
                
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
        SceneManager.sceneLoaded -= prepStore;
    }
}
