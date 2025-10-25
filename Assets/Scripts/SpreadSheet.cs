using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SpreadSheet : MonoBehaviour
{
    public static SpreadSheet inst;

    void OnEnable()
    {
        // ensures there is only ever one spreadsheet
        if (inst == null) {
            inst = this;
        } else {
            Destroy(gameObject);
        }
    }

    void OnDisable()
    {
        // when disabled (destoryed or scene change) removes this reference from static value
        inst = null;    
    }



    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
