using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class Manager : MonoBehaviour
{
    public bool mode3d = false;
    public Vector2 curPoint;
    public Text TextInfo;

    public Canvas _canvas;
    public Button RepeatButton;
    public Button ChangeModeButton;
    public Button ClearButton;
    public Button LoadButton;
    public Button SaveButton;


    public Image StartPointImage;
    public Image shaibaImage;
    public Dropdown speedDropdown;
    public Text spisokText;
    public GameObject backgroundInterface;
    public GameObject DangerField;

    LineRenderer O1;
    LineRenderer O2;

    int indexAxis = 0;

    public Vector2 x;
    public Vector2 y;

    public bool move = false;
    public bool repeat = false;
    int indexRepeat = 0;

    public float[] speed;
    float cur_speed = 0;

    List<Vector2> spisok = new List<Vector2>();

    const string SavePath = "/savedCoords.txt";

    public Text timerText;
    float timer;

    public GameObject scene;
    public Transform shaibaOBJ;

    LineRenderer shaibaDirection;

    public GameObject shaibaDirectInfo;
    void Start()
    {
        O1 = GameObject.Find("O1").GetComponent<LineRenderer>();
        O2 = GameObject.Find("O2").GetComponent<LineRenderer>();

        shaibaDirection = GameObject.Find("shaibaDirection").GetComponent<LineRenderer>();

        speedDropdown.ClearOptions();
        List<string> drop = new List<string>();
        for (int i = 0; i < speed.Length; i++)
        {
            drop.Add(speed[i].ToString());
        }
        speedDropdown.AddOptions(drop);
        DropdownNewValue();

        ChangeModeButton.onClick.AddListener(() => ChangeMode());
        ChangeMode();
    }
    public void DropdownNewValue()
    {
        cur_speed = speed[speedDropdown.value];
    }
    void Update()
    {
        Vector2 vec = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        if (move)
        {
            timer += Time.deltaTime;
            TimeSpan t = TimeSpan.FromSeconds(timer);
            timerText.text = string.Format("{0:D2}:{1:D2}:{2:D2}", t.Minutes, t.Seconds, t.Milliseconds);

            StartMove();
        }

        if (vec.x < -13)
        {
            return;
        }
        if (!mode3d)
        {
            if (indexAxis >= 2)
            {
                if (Input.GetMouseButtonDown(0) && !move && !repeat)
                {
                    float _one = (0 - vec.x) * (x.y - 0) - (x.x - 0) * (0 - vec.y);
                    float _two = (x.x - vec.x) * (y.y - x.y) - (y.x - x.x) * (x.y - vec.y);
                    float _three = (y.x - vec.x) * (0 - y.y) - (0 - y.x) * (y.y - vec.y);

                    if ((_one > 0 && _two > 0 && _three > 0) || (_one < 0 && _two < 0 && _three < 0))
                    {
                        curPoint = vec;
                        move = true;
                    }
                }
            }

            TextInfo.text = new Vector2(vec.x, vec.y).ToString();

            if (Input.GetMouseButtonDown(0))
            {
                SetAxis(vec);
            }
        }
        else
        {
            Camera.main.transform.LookAt(shaibaOBJ);
        }
    }
    void StartMove()
    {
        Vector2 cur_vec;
        if (!mode3d)
        {
            cur_vec = new Vector2(shaibaImage.GetComponent<RectTransform>().position.x, shaibaImage.GetComponent<RectTransform>().position.y);
        }
        else
        {
            cur_vec = new Vector2(shaibaOBJ.position.x, shaibaOBJ.position.z);
            shaibaDirection.SetPosition(0, shaibaOBJ.position + (Vector3.up * 0.5f));
        }
        if (cur_vec != curPoint)
        {
            if (!mode3d)
            {
                shaibaImage.GetComponent<RectTransform>().position = Vector2.MoveTowards(shaibaImage.GetComponent<RectTransform>().position, curPoint, cur_speed * Time.deltaTime);
            }
            else
            {
                shaibaOBJ.position = Vector3.MoveTowards(shaibaOBJ.position, new Vector3(curPoint.x, 0, curPoint.y), cur_speed * Time.deltaTime);
            }
        }
        else
        {
            if (repeat == false)
            {
                spisok.Add(curPoint);

                curPoint = Vector2.zero;
                RefreshSpisok();
                move = false;
                timer = 0;
            }
            else
            {
                move = false;
                StartRepeat();
            }
        }
    }
    public void ForceStop()
    {
        move = false;
        DangerField.SetActive(true);
        RepeatButton.interactable = true;
        indexRepeat = 0;
        repeat = false;
    }
    void RefreshSpisok()
    {
        spisokText.text = "";
        for (int i = 0; i < spisok.Count; i++)
        {
            if (i > 0)
            {
                spisokText.text += "\n" + spisok[i].ToString();
            }
            else
            {
                spisokText.text += spisok[i].ToString();
            }
        }
        RectTransform rt = spisokText.GetComponent<RectTransform>();
        if (spisok.Count > 8)
        {
            rt.sizeDelta = new Vector2(0, 200 + (15 * spisok.Count - 8));
        }
        else
        {
            rt.sizeDelta = new Vector2(0, 200);
        }
    }
    public void StartRepeat()
    {
        if (spisok.Count <= 0)
        {
            return;
        }
        if (repeat == false)
        {
            repeat = true;
            if (!mode3d)
            {
                shaibaImage.GetComponent<RectTransform>().position = Vector2.zero;
            }
            else
            {
                shaibaOBJ.position = Vector3.zero;
                DangerField.SetActive(false);

            }
            RepeatButton.interactable = false;
            ChangeModeButton.interactable = false;
            ClearButton.interactable = false;
            LoadButton.interactable = false;
            SaveButton.interactable = false;

            timer = 0;
            StartRepeat();
        }
        else
        {
            if (indexRepeat < spisok.Count)
            {               
                curPoint = spisok[indexRepeat];
                if (mode3d)
                {
                    shaibaOBJ.GetComponent<shaibaSuppport>().AnimPlay(indexRepeat);

                    shaibaDirection.SetPosition(1, new Vector3(curPoint.x, 0.5f, curPoint.y));

                    shaibaDirectInfo.SetActive(true);
                    shaibaDirectInfo.GetComponent<TextMesh>().color = Color.cyan;
                    shaibaDirectInfo.transform.position = new Vector3(curPoint.x, 0.5f, curPoint.y);
                    shaibaDirectInfo.GetComponent<TextMesh>().text = string.Format("{0:N1}:{1:N1}", curPoint.x, curPoint.y);
                }
                move = true;
                indexRepeat++;
            }
            else
            {
                if (mode3d)
                {
                    shaibaOBJ.GetComponent<shaibaSuppport>().AnimPlay(indexRepeat);
                    shaibaDirectInfo.SetActive(false);
                }
                RepeatButton.interactable = true;
                ChangeModeButton.interactable = true;
                ClearButton.interactable = true;
                LoadButton.interactable = true;
                SaveButton.interactable = true;

                indexRepeat = 0;
                repeat = false;
            }
        }
    }
    void SetAxis(Vector3 target)
    {
        if (indexAxis > 1)
        {
            return;
        }
        if (indexAxis == 0)
        {
            O1.positionCount = 2;
            O1.SetPosition(0, Vector3.zero);

            var distance = target.magnitude;
            var direction = target / distance;
            int parts = 20;

            O1.SetPosition(1, direction * parts);

            for(int i = 0; i < parts; i++)
            {
                GameObject go = Instantiate(StartPointImage.gameObject, _canvas.transform);
                go.transform.position = direction * i;
                go.SetActive(true);
            }

            indexAxis++;
            x = O1.GetPosition(1);
        }
        else
        {
            O2.positionCount = 2;
            O2.SetPosition(0, Vector2.zero);

            var distance = target.magnitude;
            var direction = target / distance;
            int parts = 20;

            O2.SetPosition(1, direction * parts);

            for (int i = 0; i < parts; i++)
            {
                GameObject go = Instantiate(StartPointImage.gameObject, _canvas.transform);
                go.transform.position = direction * i;
                go.SetActive(true);
            }

            indexAxis++;
            y = O2.GetPosition(1);

            StartPointImage.gameObject.SetActive(true);
            StartPointImage.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
            shaibaImage.gameObject.SetActive(true);
            shaibaImage.GetComponent<RectTransform>().anchoredPosition = Vector2.zero;
        }
    }
    public void WriteCoord()
    {
        string res = "";
        if (spisok.Count <= 0)
        {
            return;
        }
        for (int i = 0; i < spisok.Count; i++)
        {
            if (i > 0)
            {
                res += "\n" + spisok[i].x + " " + spisok[i].y.ToString();
            }
            else
            {
                res += spisok[i].x + " " + spisok[i].y.ToString();
            }
        }
        StreamWriter writer = new StreamWriter(Application.dataPath + SavePath);
        writer.WriteLine(res);
        writer.Close();
    }
    public void LoadCoord()
    {
        ClearCoord();
        StreamReader reader = new StreamReader(Application.dataPath + SavePath);
        while (reader.Peek() >= 0)
        {
            string data = reader.ReadLine();
            string[] vector_data = data.Split(char.Parse(" "));

            float.TryParse(vector_data[0], out float a);
            float.TryParse(vector_data[1], out float b);
            spisok.Add(new Vector2(a, b));
        }
        reader.Close();
        RefreshSpisok();
    }
    public void ClearCoord()
    {
        spisok.Clear();
        RefreshSpisok();
    }
    public void ChangeMode()
    {
        if (!mode3d)
        {
            scene.SetActive(true);
            shaibaOBJ.position = Vector3.zero;
            backgroundInterface.SetActive(false);
            Camera.main.orthographic = false;
            _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            O1.gameObject.SetActive(false);
            O2.gameObject.SetActive(false);
            Camera.main.transform.position = new Vector3(0, 5, -5);
            shaibaDirection.gameObject.SetActive(true);
            mode3d = true;
        }
        else
        {
            scene.SetActive(false);
            backgroundInterface.SetActive(true);
            Camera.main.orthographic = true;
            _canvas.renderMode = RenderMode.ScreenSpaceCamera;
            O1.gameObject.SetActive(true);
            O2.gameObject.SetActive(true);
            Camera.main.transform.position = new Vector3(0, 0, -5);
            Camera.main.transform.eulerAngles = Vector3.zero;
            shaibaDirection.gameObject.SetActive(false);
            DangerField.SetActive(false);
            mode3d = false;
        }
    }
    public void Restart()
    {
        Scene scene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(scene.name);
    }
}
