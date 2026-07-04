using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UseCurtain : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }
    public void OpenCurtain(string str)
    {
        curtain.Instance.str = str;
        curtain.Instance.isOpen = true;
    }
}
