using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.EventSystems;
using System.Collections;
using System.IO;

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using LitJson;


public class Cam1 : MonoBehaviour {
    public GameObject C1;
    public GameObject Jumper;
    public GameObject sphere;
    public GameObject originalButton;
    public GameObject[] buttons;
    public string[] coord;
    float deltaTime = 0.0f;

    public float timer;
    public bool ispuse;
    public bool guipuse;

    public static string CurrentFolder = "Assets/Resources/Materials/sgugit/";
    public static string ConfigPath;
    public static List<Panorama> Panoramas = new List<Panorama>();
    public static Panorama SelectedPanorama;
    public static Buttons SelectedButton;
    string CurrentScene = rotateModel.panName;

    // Use this for initialization
    void Start () {
        //PlayerPrefs.SetFloat("PositionX", 0);
        Panoramas = new List<Panorama>();
        Panoramas.Clear();
        //ConfigPath = System.IO.Path.Combine(CurrentFolder, "pano.cfg");
        TextAsset txt = (TextAsset)Resources.Load("Materials/sgugit/pano", typeof(TextAsset));
        //string ExistingConfig;
        //ExistingConfig = File.ReadAllText(txt.text); //Read config file if Exists
        //ExistingConfig = File.Exists(txt) ? File.ReadAllText(txt.ToString()) : ""; //Read config file if Exists
        //if (ExistingConfig != "") { Panoramas = JsonMapper.ToObject<List<Panorama>>(ExistingConfig); } //Config Data to Object List if not empty
        Panoramas = JsonMapper.ToObject<List<Panorama>>(txt.text);
        bool isNotEmpty = Panoramas.Any(); //is Panorama list not empty?
        NewPanorama(CurrentScene);
    }
    public void Pause()
    {
        ispuse = true;
    }

    // Update is called once per frame
    void Update () {
        //deltaTime += (Time.deltaTime - deltaTime) * 0.1f;
        //Time.timeScale = timer;
        if (Input.GetKeyDown(KeyCode.Escape) && ispuse == false)
        {
            ispuse = true;
        }
        else if (Input.GetKeyDown(KeyCode.Escape) && ispuse == true)
        {
            ispuse = false;
        }
        timer = 1f;
    }

    void OnGUI()
    {
        int w = Screen.width, h = Screen.height;
        GUIStyle style = new GUIStyle();
        //if (GUI.Button(new Rect(w-130, 20, 120, 50), "Меню"))
        //{
        //    Application.LoadLevel("mainmenu");
        //}
        if (ispuse == true)
        {
            //Cursor.visible = true;// включаем отображение курсора
            if (GUI.Button(new Rect((float)(Screen.width / 2), (float)(Screen.height / 2) - 150f, 150f, 45f), "Continue"))
            {
                ispuse = false;
                timer = 0;
                //Cursor.visible = false;
            }
            if (GUI.Button(new Rect((float)(Screen.width / 2), (float)(Screen.height / 2) - 100f, 150f, 45f), "Choose Scene"))
            {
                ispuse = false;
                timer = 0;
                SceneManager.LoadScene("Menu");
            }

            //if (GUI.Button(new Rect((float)(Screen.width / 2), (float)(Screen.height / 2), 150f, 45f), "Выход"))
            //{
            //    Application.Quit();// здесь при нажатии на кнопку загружается другая сцена, вы можете изменить название сцены на свое
            //}
        }
    }

    public void NextPano()
    {
        var ClickedButton = EventSystem.current.currentSelectedGameObject; //Забираем выбранный объект - кнопку
        var NewPano = ClickedButton.name; //Имя кнопки
        CleanPreviousPanorama();
        NewPanorama(NewPano);
    }

    public void CleanPreviousPanorama()
    {
        foreach (GameObject button in buttons)
        {
            Destroy(button);
        }
    }
    //Прыгаем в другую сферу по кнопке - кнопка должна называться jump+"имя сферы", на кнопку вешаем эту функцию
    public void NewPanorama(string PanoName)  
    {
        //Material mat = new Material(Shader.Find("Unlit / Pano360Shader"));
        //mat.mainTexture = walltex;
        //sphere.GetComponent<Renderer>().material = mat;
        Texture2D walltex = (Texture2D)Resources.Load("Materials/sgugit/" + PanoName);
        Shader wallshade = Shader.Find("Unlit/Pano360Shader");
        sphere.GetComponent<Renderer>().material.mainTexture = walltex;
        sphere.GetComponent<Renderer>().material.shader = wallshade;

        SelectedPanorama = Panoramas.Find(item => item.Name == PanoName);
        buttons = new GameObject[Panoramas.Count];
        int i = 0;

        foreach (Buttons button in SelectedPanorama.Buttons)
        {
            float RawHorizontalAngle = (float)button.PositionX / SelectedPanorama.Width * 360;
            float CalcHorizontalAngle = RawHorizontalAngle * Mathf.Deg2Rad;

            float RawVerticalAngle;
            int HalfPanoHeight = SelectedPanorama.Height / 2;
            if (button.PositionY >= HalfPanoHeight)
            {
                RawVerticalAngle = (float)(button.PositionY - HalfPanoHeight) / HalfPanoHeight * -90;
            }
            else
            {
                RawVerticalAngle = (float)(button.PositionY / HalfPanoHeight) * 90;
            }
            float CalcVerticalAngle = RawVerticalAngle * Mathf.Deg2Rad;

            float x = (float)(28 * Math.Sin(CalcHorizontalAngle) * Math.Cos(CalcVerticalAngle));
            float z = (float)(28 * Math.Cos(CalcHorizontalAngle) * Math.Cos(CalcVerticalAngle));
            float y = (float)(28 * Math.Sin(CalcVerticalAngle)); //Vertical ofset

            buttons[i] = Instantiate(originalButton, new Vector3(x, y+30, z), Quaternion.identity);
            buttons[i].transform.parent = Jumper.transform; //Setting parent
            buttons[i].transform.LookAt(buttons[i].transform.position - C1.transform.position + buttons[i].transform.position);  //Orient button to camera and inverse Look direcion
            buttons[i].name = button.Link;
            i++;
        }
        //foreach (Buttons button in SelectedPanorama.Buttons)
        //{
        //    float RawAngle = (float)button.PositionX / SelectedPanorama.Width * 360;
        //    float CalcAngle = (float)(Math.PI * RawAngle / 180.0);
        //    Debug.Log(RawAngle);
        //    float x = (float)(27 * Math.Sin(CalcAngle));
        //    float z = (float)(27 * Math.Cos(CalcAngle));
        //    buttons[i] = Instantiate(originalButton, new Vector3(x, 30, z), Quaternion.identity);
        //    buttons[i].transform.parent = Jumper.transform; //Setting parent
        //    buttons[i].transform.LookAt(buttons[i].transform.position - C1.transform.position + buttons[i].transform.position);  //Orient button to camera and inverse Look direcion
        //    buttons[i].name = button.Link;
        //    i++;
        //}
        //SceneManager.UnloadScene("main");
        //SceneManager.LoadScene("Menu");
    }
}

public class Buttons
{
    public string Link { get; set; }
    public int PositionX { get; set; }
    public int PositionY { get; set; }
}

public class Panorama
{
    public string Name { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public List<Buttons> Buttons { get; set; }
}

