using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Xml.Linq;

using UnityEngine;
using UnityEngine.UI;

using Firebase.Storage;
using Firebase.Extensions;
using JetBrains.Annotations;
using TMPro;
using UnityEngine.SceneManagement;

public class FirebaseStorageController : MonoBehaviour
{
    private FirebaseStorage _firebaseInstance;
    [SerializeField] private GameObject ThumbnailPrefab;
    private GameObject _thumbnailContainer;
    public List<GameObject> instantiatedPrefabs;
    public List<AssetData> DownloadedAssetData;
    
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
        print("1");
        //prepStore();
    }
    
    public void prepStore(Scene scene, LoadSceneMode mode)
    {
        if (scene == SceneManager.GetSceneByName("Store_Scene") && mode == LoadSceneMode.Single)
        {
            WalletManager.Instance.updateWallet();
            
            instantiatedPrefabs = new List<GameObject>();
            _thumbnailContainer = GameObject.Find("Thumbnail_Container");
            print("2 " + _thumbnailContainer);
            DownloadFileAsync("gs://emoji-junkie-dlc-store-fb6f2.appspot.com/manifest.xml", DownloadType.Manifest);
            //Get the urls inside the manifest file
            //Download each url and display to the user
        }
    }

    public void DownloadFileAsync(string url, DownloadType filetype, [Optional] AssetData assetRef){
        print("3");
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
        print("4");
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
        print("5 " + ThumbnailPrefab);
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
        yield return null;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= prepStore;
    }
}
