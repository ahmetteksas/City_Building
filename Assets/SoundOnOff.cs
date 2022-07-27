using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundOnOff : MonoBehaviour
{
    bool isSoundOn = true;
    [SerializeField] GameObject offVisual;
    public void SoundOnOffClick()
    {
        isSoundOn = !isSoundOn;

        if (isSoundOn)
        {
            AudioListener.volume = 1;
            offVisual.SetActive(false);
        }
        else
        {
            AudioListener.volume = 0;
            offVisual.SetActive(true);
        }
    }

    private void Update()
    {
        if (!Application.isFocused)
        {
            AudioListener.volume = 0;
            offVisual.SetActive(true);
        }
        else
        {
            if (isSoundOn)
            {
                AudioListener.volume = 1;
                offVisual.SetActive(false);
            }
        }
    }
}
