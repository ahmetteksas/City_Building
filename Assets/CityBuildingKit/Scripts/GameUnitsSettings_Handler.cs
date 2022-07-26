using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// Attach this to a gameobject that exists in the initial scene

public class GameUnitsSettings_Handler : MonoBehaviour
{
    [Tooltip("Choose which GameSettings asset to use")]
    public GameUnits_Settings _settings; // drag GameSettings asset here in inspector
    [SerializeField]
    public static GameUnits_Settings s; 
    public static GameUnitsSettings_Handler instance;
    void Awake(){

        if(GameUnitsSettings_Handler.instance==null){
            GameUnitsSettings_Handler.instance=this;
            if(GameUnitsSettings_Handler.s==null){
                GameUnitsSettings_Handler.s=_settings;
            }
            DontDestroyOnLoad(gameObject);
        } else {
            Debug.LogWarning("A previously awakened Settings MonoBehaviour exists!", gameObject);
            Destroy(this.gameObject);
        }

    }
}
