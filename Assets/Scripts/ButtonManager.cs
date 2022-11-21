using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class ButtonManager : MonoBehaviour
{
    [SerializeField]
    public Button Store_btn, Game_btn, Back_ss_btn, Back_gs_btn, Exit_btn, Accept_btn, Decline_btn;

    public GameObject Privacy_scroll_area;
    
    // Start is called before the first frame update
    void Start()
    {
        try
        {
            Store_btn.onClick.AddListener(StartStore);
        }
        catch(System.NullReferenceException)
        {
            Debug.Log("Some buttons are missing from the scene because they are not required");
        }
        
        try
        {
            Game_btn.onClick.AddListener(StartGame);
        }
        catch(System.NullReferenceException)
        {
            Debug.Log("Some buttons are missing from the scene because they are not required");
        }
        
        try
        {
            Back_ss_btn.onClick.AddListener(BackStore);
        }
        catch(System.NullReferenceException)
        {
            Debug.Log("Some buttons are missing from the scene because they are not required");
        }
        
        try
        {
            Back_gs_btn.onClick.AddListener(BackGame);
        }
        catch(System.NullReferenceException)
        {
            Debug.Log("Some buttons are missing from the scene because they are not required");
        }
        
        try
        {
            Exit_btn.onClick.AddListener(ExitGame);
        }
        catch(System.NullReferenceException)
        {
            Debug.Log("Some buttons are missing from the scene because they are not required");
        }
    }
    
    public void StartStore()
    {
        int policyCode = PlayerPrefs.GetInt("PolicyInt", 0);
        if (policyCode == 1)
        {
            SceneManager.LoadScene("Store_Scene");
        }
        else
        {
            OpenPolicy();
        }
        //FirebaseStorageController.Instance.prepStore();
    }
    
    private void StartGame()
    {
        SceneManager.LoadScene("Game_Scene");
    }
    
    private void BackStore()
    {
        SceneManager.LoadScene("Welcome_Scene");
    }
    
    private void BackGame()
    {
        SceneManager.LoadScene("Welcome_Scene");
    }

    public void OpenPolicy()
    {
        GameObject.Find("Policy_gameObject").transform.position = new Vector3(0,0,0);
        //Accept_btn.GetComponent<Button>().enabled = true;
        //Decline_btn.GetComponent<Button>().enabled = true;
    }

    public void AcceptPolicy()
    {
        PlayerPrefs.SetInt("PolicyInt", 1);
        StartStore();
    }

    public void ClosePolicy()
    {
        GameObject.Find("Policy_gameObject").transform.position = new Vector3(0,1000,0);
        //Accept_btn.GetComponent<Button>().enabled = false;
        //Decline_btn.GetComponent<Button>().enabled = false;
    }
    
    private void ExitGame()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
