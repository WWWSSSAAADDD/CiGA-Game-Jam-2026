using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CrusherAnimation : MonoBehaviour
{
    public GameObject aim;
    public AudioClip aimClip;
    private void OnEnable()
    {
        this.transform.localPosition = Vector3.zero;
        this.transform.GetChild(0).localPosition = new Vector3(0.141f, 0, 0);
        if (BGMManager.instance != null)
        {
            BGMManager.instance.SFXplay(aimClip);
        }
        //GetComponent<Animator>().Play("Crusher");
    }
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void one()
    {
        aim.SetActive(true);
        gameObject.SetActive(false);
    }
}
