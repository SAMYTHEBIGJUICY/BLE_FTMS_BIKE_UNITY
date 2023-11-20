using SBPScripts;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Runtime.CompilerServices;
using TMPro;
using UnityEngine;
using UnityEngine.UI;



public class WGBLEDemo : MonoBehaviour
{

    public bool isScanningDevices = false;
    public bool isSubscribed = false;

    public GameObject deviceScanResultProto;
    public Button subscribeButton;
    public Button writeButton;
    public InputField writeInput;

    public Toggle toggle;

    Transform scanResultRoot;
    public string selectedDeviceId;
    public string selectedServiceId;
    Dictionary<string, string> characteristicNames = new Dictionary<string, string>();
    public string selectedCharacteristicId;
    Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();
    string lastError;

    string read_characteristic= "{00002ad2-0000-1000-8000-00805f9b34fb}";
    string write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}";
    float last_write_time = 0.0f;
    int sended_resistance = 0;
    public TextMeshProUGUI resistance_show;
    public string output;
    public float ResistanceRatio;
    public float gradientslope;
    public int ResistancetoSent;
    public string savedbike;

    private string toggleKey = "ToggleState";
    private UInt16 flags;

    void Start()
    {

        scanResultRoot = deviceScanResultProto.transform.parent;
        deviceScanResultProto.transform.SetParent(null);

    }


    float speed = 0;
    float average_speed = 0;
    float rpm = 0;
    float average_rpm = 0f;
    UInt16 distance = 0;
    Int16 resistance = 0;
    Int16 power = 0;
    Int16 average_power = 0;
    UInt16 expended_energy = 0;


    // Update is called once per frame
    private void Update()
    {


            BleApi.ScanStatus status;
        if (isScanningDevices)
        {
                BleApi.DeviceUpdate res = new BleApi.DeviceUpdate();
            do
            {
                status = BleApi.PollDevice(ref res, false);

                if (status == BleApi.ScanStatus.AVAILABLE)
                {
                    if (!devices.ContainsKey(res.id))
                    {
                        devices[res.id] = new Dictionary<string, string>()
                    {
                        { "name", "" },
                        { "isConnectable", "False" }
                    };
                    }

                    if (res.nameUpdated && !string.IsNullOrEmpty(res.name)) // Check if the name is not blank
                    {
                        devices[res.id]["name"] = res.name;

                        // Check if a device with the same name has already been added
                        bool isNameUnique = devices.Values.Count(d => d["name"] == res.name) == 1;

                        if (isNameUnique)
                        {
                            // Check if a device with the same res.id has already been instantiated
                            GameObject existingDevice = scanResultRoot.Find(res.id)?.gameObject;
                            if (existingDevice == null)
                            {
                                // Instantiate a new device if it doesn't already exist
                                GameObject g = Instantiate(deviceScanResultProto, scanResultRoot);
                                g.name = res.id;
                                g.transform.GetChild(0).GetComponent<TextMeshProUGUI>().text = devices[res.id]["name"];
                                g.transform.GetChild(1).GetComponent<TextMeshProUGUI>().text = res.id;
                            }
                        }
                    }

                    if (res.isConnectableUpdated)
                        devices[res.id]["isConnectable"] = res.isConnectable.ToString();
                }



                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningDevices = false;

                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }


        if (isSubscribed)
        {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {

                //subcribeText.text = BitConverter.ToString(res.buf, 0, res.size);
                int index = 0;
                //  output = "Connection Loading... \n If no data appears after a few seconds please press 'Connect to Trainer' again";

                output = "";
                flags = BitConverter.ToUInt16(res.buf, index);
                index += 2;

                if (res.deviceId == selectedDeviceId)
                {

                    // Test the first bit more data
                    if ((flags & 1) == 0) // if zero intaneous speed is displayed
                    {
                        float value = (float)BitConverter.ToUInt16(res.buf, index);
                        speed = (value * 1.0f) / 100.0f;
                        output += "Speed: " + speed + "\n";
                        index += 2;
                    }
                    else
                    {
                        output += "Speed: " + speed + "\n";
                    }

                    if ((flags & 2) > 0)
                    {
                        //??
                        average_speed = BitConverter.ToUInt16(res.buf, index);
                        //output += "Average Speed: " + average_speed + "\n";
                        index += 2;
                    }
                    else if ((flags & 2) == 0)
                    {
                        //output += "Average Speed: " + average_speed + "\n";
                    }

                    if ((flags & 4) > 0)
                    {
                        rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;
                        output += "RPM: (rev/min): " + rpm + "\n";
                        index += 2;
                    }
                    else
                    {
                        output += "RPM: (rev/min): " + rpm + "\n";
                    }

                    if ((flags & 8) > 0)
                    {
                        average_rpm = (BitConverter.ToUInt16(res.buf, index) * 1.0f) / 2.0f;
                        //output += "Average RPM: " + average_rpm + "\n";
                        index += 2;
                    }
                    else
                    {
                        //output += "Average RPM: " + average_rpm + "\n";
                    }

                    if ((flags & 16) > 0)
                    {
                        distance = BitConverter.ToUInt16(res.buf, index); // ?????s
                                                                          //output += "Distance (meter): " + distance + "\n";
                        index += 2;
                    }
                    else
                    {
                        //output += "Distance (meter): " + distance + "\n";
                    }

                    if ((flags & 32) > 0)
                    {
                        resistance = BitConverter.ToInt16(res.buf, index);
                        //output += "Resistance: " + resistance + "\n";
                        index += 2;
                    }
                    else
                    {
                        // output += "Resistance: " + resistance + "\n";
                    }

                    if ((flags & 64) > 0)
                    {
                        power = BitConverter.ToInt16(res.buf, index);
                        output += "Power (Watt): " + power + "\n";
                        index += 2;
                    }
                    else
                    {
                        output += "Power (Watt): " + power + "\n";
                    }

                    if ((flags & 128) > 0)
                    {
                        average_power = BitConverter.ToInt16(res.buf, index);
                        // output += "AveragePower: " + average_power + "\n";
                        index += 2;
                    }
                    else
                    {
                        //  output += "AveragePower: " + average_power + "\n";
                    }

                    if ((flags & 256) > 0)
                    {
                        expended_energy = BitConverter.ToUInt16(res.buf, index);
                        // output += "ExpendedEnergy: " + expended_energy + "\n";
                        index += 2;
                    }
                    else
                    {
                        // output += "ExpendedEnergy: " + expended_energy + "\n";
                    }

                    output = "Connection Established\n\n" + output;

                    // subcribeText.text = Encoding.ASCII.GetString(res.buf, 0, res.size);
                    //Debug.Log("power" + power);

                }
            }
        }
        {
            // log potential errors
            BleApi.ErrorMessage res = new BleApi.ErrorMessage();
            BleApi.GetError(out res);
            if (lastError != res.msg)
            {
                //Debug.LogError(res.msg);

                lastError = res.msg;
            }
        }





      

    }

    public void StopDeviceScanOnly()
    {
        isScanningDevices = false;
        BleApi.StopDeviceScan();

    }

    public void RefreshDeviceScan()

    {
        isScanningDevices = false;
        BleApi.StopDeviceScan();
        BleApi.Quit();

    }

    private void OnApplicationQuit()
    {
        BleApi.Quit();
    }

    public float GetPower()
    {
        return power;
    }


    public float GetSpeed()
    {
        return speed;
    }

    public float GetRPM()
    {
        return rpm;
    }

    public string GetOuput()
    {
        return output;
    }

    public bool GetScanning()
    {
        return isScanningDevices;
    }

    public string Geterror()
    {
        return lastError;
    }

    public bool GetSubscribed()
    {
        return isSubscribed;
    }

    public string GetDeviceID()
    {
        return selectedDeviceId;
    }


    public void StartStopDeviceScan()
    {
        if (!isScanningDevices)
        {
            // start new scan
            for (int i = scanResultRoot.childCount - 1; i >= 0; i--)
                Destroy(scanResultRoot.GetChild(i).gameObject);
            BleApi.StartDeviceScan();
            isScanningDevices = true;

        }
        else
        {
            // stop scan
            isScanningDevices = false;
            BleApi.StopDeviceScan();

        }
    }

    public void SelectDevice(GameObject data)
    {
        for (int i = 0; i < scanResultRoot.transform.childCount; i++)
        {
            var child = scanResultRoot.transform.GetChild(i).gameObject;
            child.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color = child == data ? Color.red :
                deviceScanResultProto.transform.GetChild(0).GetComponent<TextMeshProUGUI>().color;
        }
        selectedDeviceId = data.name;


        //serviceScanButton.interactable = true;
        subscribeButton.interactable = true;
    }



    public void Subscribe()
    {
        // no error code available in non-blocking mode
        BleApi.SubscribeCharacteristic(selectedDeviceId, selectedServiceId = "{00001826-0000-1000-8000-00805f9b34fb}", read_characteristic = "{00002ad2-0000-1000-8000-00805f9b34fb}", false);
        if (!isSubscribed)

        {
            isSubscribed = true;



        }


    }

    public void SaveBike()
    {
        bool isToggleOn;

        isToggleOn = toggle.isOn;

        if(isToggleOn ==true) 
        {

            ES3.Save<string>("selectedDeviceId", selectedDeviceId);

        }


    }

    


    //public void AutoConnectBikenonco()
    //{

    //    savedbike = ES3.Load<string>("selectedDeviceId", defaultValue: null);
    //    bool savedState = PlayerPrefs.GetInt(toggleKey) == 1;

    //    if (savedbike != null && savedState == true)
    //    {

    //        StartStopDeviceScan();
    //        selectedDeviceId = ES3.Load<string>("selectedDeviceId");

    //        if (devices.ContainsKey(selectedDeviceId))
    //        {

    //            Subscribe();
    //            Debug.Log("it works!");
    //        }
    //        else
    //        {
    //            Debug.Log("not found bro");

    //        }
    //    }
    //}


    private byte[] Convert16(string strText)
    {
        strText = strText.Replace(" ", "");
        byte[] bText = new byte[strText.Length / 2];
        for (int i = 0; i < strText.Length / 2; i++)
        {
            bText[i] = Convert.ToByte(Convert.ToInt32(strText.Substring(i * 2, 2), 16));
        }
        return bText;
    }

    public void Write(string msg)
    {

        byte[] payload22 = Convert16(msg);
        BleApi.BLEData data = new BleApi.BLEData();
        data.buf = new byte[512];
        data.size = (short)payload22.Length;
        data.deviceId = selectedDeviceId;
        data.serviceUuid = selectedServiceId;
        data.characteristicUuid = write_characteristic;
        for (int i = 0; i < payload22.Length; i++)
        {
            data.buf[i] = payload22[i];
        }
        BleApi.SendData(in data, false);
    }

    public void SendNewResistance()
        //by samy
    {

       // float TotalResistance = (controlboi.gradientslope*10f) * (ResistanceRatio/20f);
        float TotalResistance = (gradientslope * 10f) * (ResistanceRatio / 20f);
        ResistancetoSent = (int)TotalResistance;

        write_resistance(ResistancetoSent);

        //Debug.Log("ResistancetoSent: " + ResistancetoSent);
        //Debug.Log("TotalResistance: " + TotalResistance);
        //Debug.Log("gradientslope: " + gradientslope);
        //Debug.Log("ResistanceRatio: " + ResistanceRatio);
    }


    public void write_resistance(int val)
    {
        if (Time.time - last_write_time < 1f)
        {
            return;
        }
        else
        {
            last_write_time = Time.time;
        }


        //Debug.Log("write resistance: " + val);

        BleApi.SubscribeCharacteristic(selectedDeviceId, selectedServiceId, write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}", false);
        Write("00");
        byte resistance1 = Convert.ToByte(val % 256);
        byte resistance2 = Convert.ToByte(val / 256);
        byte[] payload = { 0x11, 0x00, 0x00, resistance1, resistance2, 0x00, 0x00 };
        BleApi.BLEData data = new BleApi.BLEData();
        data.buf = new byte[512];
        data.deviceId = selectedDeviceId;
        data.serviceUuid = selectedServiceId;
        data.characteristicUuid = write_characteristic;
        for (int i = 0; i < payload.Length; i++)
        {
            data.buf[i] = payload[i];
        }
        data.size = (short)payload.Length;
        BleApi.SendData(in data, false);
    }

   

}

