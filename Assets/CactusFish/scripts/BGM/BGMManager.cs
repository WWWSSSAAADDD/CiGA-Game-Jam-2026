using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BGMManager : MonoBehaviour
{
    public static BGMManager instance;
    public MsfxLib msfxLib;
    public BgmLib bgmLib;
    public AudioSource msfx;
    public AudioSource sfx;
    public AudioSource bgm;
    public Slider bgmSlider;
    public Slider sfxSlider;
    public Toggle toggle;
    bool isOpen;
    // Start is called before the first frame update

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    void Start()
    {
        BGMplay(bgmLib.audioClips[0]);
        msfx.clip = msfxLib.audioClips[0];
        toggle.isOn = isOpen;
    }

    // Update is called once per frame
    void Update()
    {
        bgm.volume = bgmSlider.value;
        msfx.volume = sfxSlider.value;
        sfx.volume = msfx.volume;
        MouseSFX();
        isOpen = toggle.isOn;
    }

    public void BGMplay(AudioClip clip)
    {
        bgm.Stop();
        bgm.clip = clip;
        bgm.Play();
    }

    public void SFXplay(AudioClip clip)
    {
        sfx.Stop();
        sfx.clip = clip;
        sfx.Play();
    }

    void MSFXplay(AudioClip clip)
    {
        msfx.Stop();
        msfx.clip = clip;
        msfx.Play();
    }

    void MouseSFX()
    {
        if (Input.GetMouseButtonDown(0))
        {
            if (toggle.isOn)
            {
                MSFXplay(msfxLib.audioClips[Random.Range(0, msfxLib.audioClips.Count)]);
            }
            else
            {
                msfx.Stop();
                msfx.Play();
            }
        }
    }
}
