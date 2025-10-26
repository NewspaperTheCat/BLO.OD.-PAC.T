using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TransitionController : MonoBehaviour
{
    public Image emailDisplay;
    public List<Sprite> dailyEmails = new List<Sprite>();

    // Start is called before the first frame update
    void Start()
    {
        emailDisplay.sprite = dailyEmails[GameManager.inst.day];
    }

    public void NextDay()
    {
        GameManager.inst.day++;
        SceneNavigator.inst.ToGame();
    }
}
