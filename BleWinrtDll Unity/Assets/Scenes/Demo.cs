using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class Demo : MonoBehaviour
{
    public bool isScanningDevices = false;
    public bool isScanningServices = false;
    public bool isScanningCharacteristics = false;
    [SerializeField]
    public bool isSubscribed = false;
    public Text deviceScanButtonText;
    public Text deviceScanStatusText;
    public GameObject deviceScanResultProto;
    public Button serviceScanButton;
    public Text serviceScanStatusText;
    public Dropdown serviceDropdown;
    public Button characteristicScanButton;
    public Text characteristicScanStatusText;
    public Dropdown characteristicDropdown;
    public Button subscribeButton;
    public Text subcribeText;
    public Button writeButton;
    public InputField writeInput;
    public Text errorText;

    Transform scanResultRoot;
    public string selectedDeviceId;
    public string selectedServiceId;
    Dictionary<string, string> characteristicNames = new Dictionary<string, string>();
    public string selectedCharacteristicId;
    Dictionary<string, Dictionary<string, string>> devices = new Dictionary<string, Dictionary<string, string>>();
    string lastError;

    string read_characteristic;
    string write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}";
    float last_write_time = 0.0f;
    int sended_resistance = 0;
    public Text resistance_show;
    public string output;


    // Start is called before the first frame update
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
    void Update()
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
                        devices[res.id] = new Dictionary<string, string>() {
                            { "name", "" },
                            { "isConnectable", "False" }
                        };
                    if (res.nameUpdated)
                        devices[res.id]["name"] = res.name;
                    if (res.isConnectableUpdated)
                        devices[res.id]["isConnectable"] = res.isConnectable.ToString();
                    // consider only devices which have a name and which are connectable
                    if (devices[res.id]["name"] != "" && devices[res.id]["isConnectable"] == "True")
                    {
                        // add new device to list
                        GameObject g = Instantiate(deviceScanResultProto, scanResultRoot);
                        g.name = res.id;
                        g.transform.GetChild(0).GetComponent<Text>().text = devices[res.id]["name"];
                        g.transform.GetChild(1).GetComponent<Text>().text = res.id;
                    }
                }
                else if (status == BleApi.ScanStatus.FINISHED)
                {
                    isScanningDevices = false;
                    deviceScanButtonText.text = "Scan devices";
                    deviceScanStatusText.text = "finished";
                }
            } while (status == BleApi.ScanStatus.AVAILABLE);
        }
        //if (isScanningServices)
        //{
        //    BleApi.Service res = new BleApi.Service();
        //    do
        //    {
        //        status = BleApi.PollService(out res, false);
        //        if (status == BleApi.ScanStatus.AVAILABLE)
        //        {
        //            serviceDropdown.AddOptions(new List<string> { res.uuid });
        //            // first option gets selected
        //            if (serviceDropdown.options.Count == 1)
        //                SelectService(serviceDropdown.gameObject);
        //        }
        //        else if (status == BleApi.ScanStatus.FINISHED)
        //        {
        //            isScanningServices = false;
        //            serviceScanButton.interactable = true;
        //            serviceScanStatusText.text = "finished";
        //        }
        //    } while (status == BleApi.ScanStatus.AVAILABLE);
        //}
        //if (isScanningCharacteristics)
        //{
        //    BleApi.Characteristic res = new BleApi.Characteristic();
        //    do
        //    {
        //        status = BleApi.PollCharacteristic(out res, false);
        //        if (status == BleApi.ScanStatus.AVAILABLE)
        //        {
        //            string name = res.userDescription != "no description available" ? res.userDescription : res.uuid;
        //            characteristicNames[name] = res.uuid;
        //            characteristicDropdown.AddOptions(new List<string> { name });
        //            // first option gets selected
        //            if (characteristicDropdown.options.Count == 1)
        //                SelectCharacteristic(characteristicDropdown.gameObject);
        //        }
        //        else if (status == BleApi.ScanStatus.FINISHED)
        //        {
        //            isScanningCharacteristics = false;
        //            characteristicScanButton.interactable = true;
        //            characteristicScanStatusText.text = "finished";
        //        }
        //    } while (status == BleApi.ScanStatus.AVAILABLE);
        //}
        if (isSubscribed)
        {
            BleApi.BLEData res = new BleApi.BLEData();
            while (BleApi.PollData(out res, false))
            {
                //subcribeText.text = BitConverter.ToString(res.buf, 0, res.size);
                int index = 0;
                string output = "";
                UInt16 flags = BitConverter.ToUInt16(res.buf, index);
                index += 2;
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
                    output += "Average Speed: " + average_speed + "\n";
                    index += 2;
                }
                else if ((flags & 2) == 0)
                {
                    output += "Average Speed: " + average_speed + "\n";
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
                    output += "Average RPM: " + average_rpm + "\n";
                    index += 2;
                }
                else
                {
                    output += "Average RPM: " + average_rpm + "\n";
                }

                if ((flags & 16) > 0)
                {
                    distance = BitConverter.ToUInt16(res.buf, index); // ?????s
                    output += "Distance (meter): " + distance + "\n";
                    index += 2;
                }
                else
                {
                    output += "Distance (meter): " + distance + "\n";
                }

                if ((flags & 32) > 0)
                {
                    resistance = BitConverter.ToInt16(res.buf, index);
                    output += "Resistance: " + resistance + "\n";
                    index += 2;
                }
                else
                {
                    output += "Resistance: " + resistance + "\n";
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
                    output += "AveragePower: " + average_power + "\n";
                    index += 2;
                }
                else
                {
                    output += "AveragePower: " + average_power + "\n";
                }

                if ((flags & 256) > 0)
                {
                    expended_energy = BitConverter.ToUInt16(res.buf, index);
                    output += "ExpendedEnergy: " + expended_energy + "\n";
                    index += 2;
                }
                else
                {
                    output += "ExpendedEnergy: " + expended_energy + "\n";
                }

                // subcribeText.text = Encoding.ASCII.GetString(res.buf, 0, res.size);
                subcribeText.text = output;
            }
        }
        {
            // log potential errors
            BleApi.ErrorMessage res = new BleApi.ErrorMessage();
            BleApi.GetError(out res);
            if (lastError != res.msg)
            {
                //Debug.LogError(res.msg);
                errorText.text = res.msg;
                lastError = res.msg;
            }
        }
    }

    private void OnApplicationQuit()
    {
        BleApi.Quit();
    }

    public float GetPower()
    {
        return power;
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
            deviceScanButtonText.text = "Stop scan";
            deviceScanStatusText.text = "scanning";
        }
        else
        {
            // stop scan
            isScanningDevices = false;
            BleApi.StopDeviceScan();
            deviceScanButtonText.text = "Start scan";
            deviceScanStatusText.text = "stopped";
        }
    }

    public void SelectDevice(GameObject data)
    {
        for (int i = 0; i < scanResultRoot.transform.childCount; i++)
        {
            var child = scanResultRoot.transform.GetChild(i).gameObject;
            child.transform.GetChild(0).GetComponent<Text>().color = child == data ? Color.red :
                deviceScanResultProto.transform.GetChild(0).GetComponent<Text>().color;
        }
        selectedDeviceId = data.name;
        //serviceScanButton.interactable = true;
        subscribeButton.interactable = true;
    }

    //public void StartServiceScan()
    //{
       // if (!isScanningServices)
       // {
            // start new scan
           // serviceDropdown.ClearOptions();
            //BleApi.ScanServices(selectedDeviceId);
            //isScanningServices = true;
            //serviceScanStatusText.text = "scanning";
            //serviceScanButton.interactable = false;

        // }
    //}

    //public void SelectService(GameObject data)
    //{
    //    selectedServiceId = serviceDropdown.options[serviceDropdown.value].text;
    //    characteristicScanButton.interactable = true;
    //}
    //public void StartCharacteristicScan()
    //{
    //    if (!isScanningCharacteristics)
    //    {
    //        // start new scan
    //        characteristicDropdown.ClearOptions();
    //        BleApi.ScanCharacteristics(selectedDeviceId, selectedServiceId);
    //        isScanningCharacteristics = true;
    //        characteristicScanStatusText.text = "scanning";
    //        characteristicScanButton.interactable = false;
    //    }
    //}

    //public void SelectCharacteristic(GameObject data)
    //{
    //    string name = characteristicDropdown.options[characteristicDropdown.value].text;
    //    selectedCharacteristicId = characteristicNames[name];
    //    subscribeButton.interactable = true;
    //    writeButton.interactable = true;
    //}

    public void Subscribe()
    {
        // no error code available in non-blocking mode
        BleApi.SubscribeCharacteristic_Read(selectedDeviceId, selectedServiceId = "{00001826-0000-1000-8000-00805f9b34fb}", read_characteristic = "{00002ad2-0000-1000-8000-00805f9b34fb}", false);
        isSubscribed = true;
    }

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

    public void write_resistance(float val)
    {
        write_resistance(Mathf.FloorToInt(val));
    }
    public void write_resistance(int val)
    {
        if (Time.time - last_write_time < 0.1f)
        {
            return;
        }
        else
        {
            last_write_time = Time.time;
        }

        Debug.Log("write resistance: " + val);

        BleApi.SubscribeCharacteristic_Write(selectedDeviceId, selectedServiceId, write_characteristic = "{00002ad9-0000-1000-8000-00805f9b34fb}", false);
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
