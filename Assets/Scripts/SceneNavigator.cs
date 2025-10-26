using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class SceneNavigator : MonoBehaviour
{
    // Singleton Design Pattern (to a degree)
    // NOT GLOBAL so scenes can have easy unity event button reference in local scene navigators
    public static SceneNavigator inst;
    void OnEnable()
    {
        // ensures there is only ever one Scene Navigator
        if (inst == null)
        {
            inst = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        // when disabled (destoryed or scene change) removes this reference from static value
        inst = null;
    }

    public void ToTitle() { SceneManager.LoadScene("Title"); }
    public void ToEmployees() { SceneManager.LoadScene("Employees"); }
    public void ToGame() { SceneManager.LoadScene("Game"); }
    public void ToTransition() { SceneManager.LoadScene("Transition"); }
    public void QuitOut()
    {
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #else
            Application.Quit();
        #endif
    }
}
