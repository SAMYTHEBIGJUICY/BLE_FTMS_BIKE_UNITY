using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Deno_UI : MonoBehaviour
{
    // Start is called before the first frame update
    private bool connected = false;
    public Demo connector;
    public Text info;
    public Text resistance_show;

    public string write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}";
    //void Start()
    //{
    //    connector = new Demo(this);

    //}

    public void write_resistance(float val)
    {
        //if (connected)
        {
            connector.write_resistance(val);
            resistance_show.text = "Resistance: " + Mathf.FloorToInt(val).ToString();
        }
    }

    //// Update is called once per frame
    //void Update()
    //{
    //   // if (connected)
    //    {
    //        connector.Update();
    //        info.text = connector.output;
    //    }
    //}
    //private void OnApplicationQuit()
    //{
    //    connector.quit();
    //}
}
