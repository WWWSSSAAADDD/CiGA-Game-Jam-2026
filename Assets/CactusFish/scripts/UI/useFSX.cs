using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class useFSX : MonoBehaviour
{
    public AudioClip clip;


    public void UseFsx()
    {
        BGMManager.instance.SFXplay(clip);
    }
}
