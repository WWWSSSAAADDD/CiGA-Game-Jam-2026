using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;

public class NoDestroy : MonoBehaviour
{
    static GameObject instance;
    // Start is called before the first frame update
    void Start()
    {
        if (instance == null)
        {
            instance = this.gameObject;
            if (!GameObject.Find("EventSystem"))
            {
                GameObject obj = new GameObject();
                obj.name = "EventSystem";
                obj.AddComponent<EventSystem>();
                obj.AddComponent<StandaloneInputModule>();
            }
            DontDestroyOnLoad(this.gameObject);
        }
        else
        {
            Destroy(this.gameObject);
        }
    }

    // Update is called once per frame
    void Update()
    {

    }
}
