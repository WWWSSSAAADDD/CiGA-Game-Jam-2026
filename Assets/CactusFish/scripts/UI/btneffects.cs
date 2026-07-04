using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class btneffects : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Vector2 m_size;
    private void Start()
    {
        m_size = this.transform.localScale;
    }
    public void OnPointerEnter(PointerEventData eventData)
    {
        Vector2 vector = this.transform.localScale;
        this.transform.localScale = Vector2.Lerp(vector, m_size * 1.1f, 1);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Vector2 vector = this.transform.localScale;
        this.transform.localScale = Vector2.Lerp(vector, m_size, 1);
    }
}
