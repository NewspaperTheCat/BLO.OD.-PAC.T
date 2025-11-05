using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ButtonBehavior : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
{
    public Sprite sprite;
    public Sprite highlightSprite;

    void Start()
    {
        transform.GetComponent<Image>().sprite = highlightSprite;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        transform.GetComponent<Image>().sprite = sprite;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        transform.GetComponent<Image>().sprite = highlightSprite;
    }
}
