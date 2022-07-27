using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class MainSceneController : MonoBehaviour
{
    [SerializeField] private Button continueButton;


    void Start()
    {
        if (PlayerPrefs.GetString("savefile") != "")
        {
            continueButton.interactable = true;
        }
        else
        {
            continueButton.interactable = false;
        }
    }

    public void LoadScene1()
    {
        SceneManager.LoadScene(1);
    }
    public void LoadScene2()
    {
        SceneManager.LoadScene(2);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
