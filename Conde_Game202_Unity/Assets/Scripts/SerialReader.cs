using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO.Ports;
using System;
using System.Threading;
using System.Reflection;
using System.Text.RegularExpressions;
using TMPro;
using UnityEngine.UI;

public class SerialReader : MonoBehaviour
{
	//must match microcontroller com port
    public string SerialPort = "/dev/cu.usbmodem14201";

    //must match microcontroller baudrate
    public int baudrate = 9600;

    //
    public TextMeshProUGUI countText;

    //initalize serial com data stream through serialport
    SerialPort stream;

    //create seperate thread for non-blocking reads of sensor data
    Thread SerialThread;

    public string curData = " ";

    //flag to singal when to end the thread of reading sensor data
    bool bSystemsClear = true;

    bool endFlag = false;

    bool editFlag = false;

    public GameObject BlueIndicatorMarley;
    public GameObject RedIndicatorMarley;
    public GameObject YellowIndicatorMarley;
    public GameObject GreenIndicatorMarley;
    public GameObject IndicatorHolder;

    public GameObject winTextObject;

    // counting the score for the player
    private int count;

    public GameObject startButton;
    public GameObject comPortField;
    // Start is called before the first frame update
    public void Start()
    {
    	if(startButton == null) startButton = GameObject.Find("StartButton");
        if(comPortField == null) comPortField = GameObject.Find("ArduinoCOMPort");
        
        if(comPortField.GetComponent<InputField>().text.Length > 1)
        {
        	SerialPort = comPortField.GetComponent<InputField>().text;
        	PlayerPrefs.SetString("COM", SerialPort);
        }
        if(PlayerPrefs.HasKey("COM"))
        {
        	SerialPort = PlayerPrefs.GetString("COM");
        }
        else
        {
        	SerialPort = "/dev/cu.usbmodem14201";
        	PlayerPrefs.SetString("COM", SerialPort);
        }

        comPortField.GetComponent<InputField>().text = SerialPort;



        stream = new SerialPort(SerialPort, baudrate); //Set the port and the baud rate
        stream.ReadTimeout = 10000; //enable timeout to prevent read from blocking code if no data is recieved
        stream.DtrEnable = true;

        //open sepearate thread to read data and update unity from IMUs
        SerialThread = new Thread(new ThreadStart(GetSerial));
        try
        {
            stream.Open();
        }
        catch (Exception e)
        {
            Debug.Log("Could not open serial port: " + e.Message);
            bSystemsClear = false;
        }
        if (bSystemsClear) SerialThread.Start();

        if(BlueIndicatorMarley == null || RedIndicatorMarley == null || YellowIndicatorMarley == null || GreenIndicatorMarley == null || IndicatorHolder == false)
        { 
        	BlueIndicatorMarley = GameObject.Find("Blue");
        	RedIndicatorMarley = GameObject.Find("Red");
        	YellowIndicatorMarley = GameObject.Find("Yellow");
        	GreenIndicatorMarley = GameObject.Find("Green");
        	IndicatorHolder = GameObject.Find("IndicatorHolder");
        }


        count = 0;

        SetCountText();
        winTextObject.SetActive(false);

        if(bSystemsClear) startButton.GetComponent<Button>().interactable = true;
    }

    public GameObject StartMenu;
   	public void StartGame()
   	{
   		if(StartMenu == null) StartMenu = GameObject.Find("Start Menu");
   		StartMenu.SetActive(false);
   		IndicatorHolder.GetComponent<AudioSource>().volume = 0.2f;

   	}

   	public void UpdateComPort()
   	{
   		if(comPortField == null) comPortField = GameObject.Find("ArduinoCOMPort");
        
        if(comPortField.GetComponent<InputField>().text.Length > 1)
        {
        	SerialPort = comPortField.GetComponent<InputField>().text;
        	PlayerPrefs.SetString("COM", SerialPort);
        }
        if(PlayerPrefs.HasKey("COM"))
        {
        	SerialPort = PlayerPrefs.GetString("COM");
        }
        else
        {
        	SerialPort = "/dev/cu.usbmodem14201";
        	PlayerPrefs.SetString("COM", SerialPort);
        }

        Application.LoadLevel (Application.loadedLevel);
   	}


    void SetCountText()
    {
        countText.text = "KittyCrazeMeter: " + ((((float)count)/9f)*100f).ToString() + "%";
        if(count >= 9)
        {
        	winTextObject.SetActive(true);
        }

    }


    bool bGreen = false, bBlue = false, bRed = false, bYellow = false, bTimerEventFlag = false;
    //this thread collects all arduino data with unblocking code
    private void GetSerial()
    {
        while (SerialThread.IsAlive && endFlag == false)
        {	
        	curData = stream.ReadLine();

            //parse the incoming data from the microcontroller
            string[] data = curData.Split(':');
            if (data.Length == 2)
            {
            	string eventstr = Regex.Replace(data[0].Trim(' '), @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));
            	string actionstr = Regex.Replace(data[1].Trim(' '), @"[^\w\.@-]", "", RegexOptions.None, TimeSpan.FromSeconds(1.5));

            	if(eventstr.CompareTo("ButtonEvent") == 0)
            	{
            		if(actionstr.CompareTo("Green")  == 0)
            		{
            			bGreen = true;

            		} 
            		else if (actionstr.CompareTo("Red")  == 0)
            		{
            			bRed = true;
            		}
            		else if (actionstr.CompareTo("Blue")  == 0)
            		{
            			bBlue = true;
            		}
            		else if (actionstr.CompareTo("Yellow")  == 0)
            		{
            			bYellow = true;
            		}

            	}

            	if(eventstr.CompareTo("TimerEvent")  == 0)
            	{
            		//randomize color loactions
            		bTimerEventFlag = true;
            	}
            }
            
        }
        if (endFlag == true)
        {
            stream.Close();
            Debug.Log("End");
            editFlag = true;
        }
    }

    public float catspeed = 300;
    IEnumerator HandleArduinoButtonInput(GameObject Indicator, float delayseconds)
    {
    	//move block in direction of click
    	var force = transform.position - Indicator.transform.position;
    	var magnitude = catspeed + (count-1)*50f;
    	force.Normalize();
    	GetComponent<Rigidbody>().AddForce(-force * magnitude);


        //signal light in force push
    	Light indicatorLight = Indicator.GetComponent("Light") as Light;
    	indicatorLight.range = 2;
    	yield return new WaitForSeconds(delayseconds);
    	indicatorLight.range = 0;


    }

    IEnumerator RotateIndicators(GameObject IndicatorHolder, int targetAngle, float rotatetime)
    {
    	if(targetAngle < 0)
    	{
    		targetAngle = targetAngle + 360;
    	}
    	Debug.Log("Rotating to: " + targetAngle.ToString());

    	Vector3 byAngles = new Vector3(0f, targetAngle, 0f);
    	float inTime = rotatetime;

    	Light Blue = BlueIndicatorMarley.GetComponent("Light") as Light;
    	Light Red = RedIndicatorMarley.GetComponent("Light") as Light;
    	Light Green = GreenIndicatorMarley.GetComponent("Light") as Light;
    	Light Yellow = YellowIndicatorMarley.GetComponent("Light") as Light;



    	var fromAngle = IndicatorHolder.transform.rotation;
        var toAngle = Quaternion.Euler(IndicatorHolder.transform.eulerAngles + byAngles);
        for(var t = 0f; t < 1; t += Time.deltaTime/inTime) {
        	Blue.range = ((inTime - t) / inTime) * 5;
    		Red.range = ((inTime - t) / inTime) * 5;
    		Green.range = ((inTime - t) / inTime) * 5;
    		Yellow.range = ((inTime - t) / inTime) * 5;

             IndicatorHolder.transform.rotation = Quaternion.Lerp(fromAngle, toAngle, t);
             yield return null;
        }

        Blue.range = 0;
    	Red.range = 0;
    	Green.range =0;
    	Yellow.range = 0;

    }


    int [] rotationOptions = {90, 180, 270, -90, -180, -270};

    // Update is called once per frame
    void Update()
    {
        //press escape to end editor playmode and stop seperate thread of reading sensor data
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            endFlag = true;
            Application.Quit(); //end application if build
        }

        if(Input.GetKeyDown(KeyCode.R))
        {
        	UpdateComPort();
        	Debug.Log("Restarting Game");
        }


        float lighttime = 0.50f;
        if(bRed)
        {
        	bRed = false;
        	StartCoroutine(HandleArduinoButtonInput(RedIndicatorMarley, lighttime));
        }

        if(bGreen)
        {
        	bGreen = false;
        	StartCoroutine(HandleArduinoButtonInput(GreenIndicatorMarley, lighttime));
        }

        if(bBlue)
        {
        	bBlue = false;
        	StartCoroutine(HandleArduinoButtonInput(BlueIndicatorMarley, lighttime));
        }

        if(bYellow)
        {
        	bYellow = false;
        	StartCoroutine(HandleArduinoButtonInput(YellowIndicatorMarley, lighttime));
        }

        if(bTimerEventFlag)
        {
        	bTimerEventFlag = false;

        	StartCoroutine(RotateIndicators(IndicatorHolder, rotationOptions[UnityEngine.Random.Range(0,rotationOptions.Length-1)], 1.5f));
        }


        //if in unity editor, press escape to close the thread
        if (editFlag == true)
        {
#if UNITY_EDITOR //if in editor, stop editor on escape
            Type t = null;
            foreach (Assembly a in AppDomain.CurrentDomain.GetAssemblies())
            {
                t = a.GetType("UnityEditor.EditorApplication");
                if (t != null)
                {
                    t.GetProperty("isPlaying").SetValue(null, false, null);
                    break;
                }
            }
#endif
            Application.Quit(); //end application if build
        }

    }

    private void OnTriggerEnter(Collider other)
    {
    	if(other.gameObject.CompareTag("PickUp"))
    	{
    		other.gameObject.SetActive(false);
    		count = count + 1;
    		gameObject.GetComponent<AudioSource>().Play();

    		SetCountText();
    	}
    }



}
