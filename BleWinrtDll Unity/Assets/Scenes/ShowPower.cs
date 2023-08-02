using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShowPower : MonoBehaviour
{

    public Text UI_Power;

    Demo demo = null;
    // Start is called before the first frame update
    void Start()
    {
        demo = FindObjectOfType<Demo>();    
    }

    // Update is called once per frame
    void Update()
    {
        UI_Power.text = "WATT: "+demo.GetPower().ToString();
    }
}
