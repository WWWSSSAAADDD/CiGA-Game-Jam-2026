using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPlane : MonoBehaviour
{
    public static SettingPlane Instance;
    // Start is called before the first frame update
    void Start()
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

    public void OpenSetting()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        group.alpha = 1.0f;
        group.blocksRaycasts = true;
        group.interactable = true;
    }
    public void CloneSetting()
    {
        CanvasGroup group = GetComponent<CanvasGroup>();
        group.alpha = 0f;
        group.blocksRaycasts = false;
        group.interactable = false;
    }
    // Update is called once per frame
    void Update()
    {

    }
}
