using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class curtain : MonoBehaviour
{
    public static curtain Instance;
    public bool isOpen;
    bool isOpen2;
    public string str;
    float t;
    private void Start()
    {


        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }

    }
    private void Update()
    {
        if (isOpen)
        {
            t += Time.deltaTime;
            this.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(0, 1, t / 1);
            this.GetComponent<CanvasGroup>().blocksRaycasts = true;
            this.GetComponent<CanvasGroup>().interactable = true;
            if (this.GetComponent<CanvasGroup>().alpha == 1)
            {
                SceneManager.LoadScene(str);
                this.GetComponent<CanvasGroup>().alpha = 1;
                t = 0;
                isOpen = false;
                isOpen2 = true;

            }
        }
        if (isOpen2)
        {
            t += Time.deltaTime;
            this.GetComponent<CanvasGroup>().alpha = Mathf.Lerp(1, 0, t / 1);
            if (this.GetComponent<CanvasGroup>().alpha == 0)
            {
                this.GetComponent<CanvasGroup>().alpha = 0;
                t = 0;
                this.GetComponent<CanvasGroup>().blocksRaycasts = false;
                this.GetComponent<CanvasGroup>().interactable = false;
                Time.timeScale = 1.0f;
                if (BGMManager.instance.bgm.clip != BGMManager.instance.bgmLib.audioClips[0])
                {
                    BGMManager.instance.BGMplay(BGMManager.instance.bgmLib.audioClips[0]);
                }
                isOpen2 = false;
            }
        }
    }
}
