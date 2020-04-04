using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using KModkit;

// thanks Emik for optimization
public class numberWangScript : MonoBehaviour
{

    public KMBombInfo Bomb;
    public KMAudio Audio;
    public KMSelectable[] buttons;
    public TextMesh screenText;
    public TextMesh[] buttonTexts;
    public GameObject board;
    public Material[] pictures;
    public GameObject rotateBoardBack;

    //Logging
    static int moduleIdCounter = 1;
    int moduleId;
    private bool moduleSolved;
    // other crap(tm)
    int rngR, rngG, rngB;
    private List<int> buttonsPressed = new List<int>();
    private List<int> buttonNumbers = new List<int>();
    static readonly private int[] serialWang = new int[] { 23, 18, 2, 31, 22, 7, 34, 289, 90, 35, 40, 43, 61, 8, 52, 404 };
    static readonly private int[] litWang = new int[] { 21, 58, 81, 666, 9, 41, 98, 33, 39, 34, 68, 12, 36, 49, 491, 43, 75, 57, 52, 54, 90, 73, 101, 28 };
    static readonly private int[] batteryWang = new int[] { 22, 36, 89, 40, 11, 5, 31, 50, 360, 12, 420, 70, 7, 45, 23, 121, 1337, 30, 41, 6 };
    static readonly private int[] parallelWang = new int[] { 34, 29, 78, 25, 58, 35, 11, 23, 560, 13, 47, 2016, 89, 10, 97, 24, 18, 144, 31, 6, 1, 35, 87, 60, 6, 53, 19, 48, 57, 16 };
    static readonly private int[] alwaysWang = new int[] { 189, 30, 3, 201, 56, 4, 42, 69, 125, 37 };
    int numbersWanged = 0;
    int test = 0;
    int back = 0;
    private List<bool> buttonsWanged = new List<bool>();
    bool boardRotated = false;


    void Awake()
    {
        moduleId = moduleIdCounter++;

        foreach (KMSelectable button in buttons)
        {
            KMSelectable pressedButton = button;
            button.OnInteract += delegate () { ButtonPress(pressedButton); return false; };
        }
    }

    // Use this for initialization
    void Start()
    {
        screenText.text = "";
        for (int i = 0; i < buttons.Length; i++)
        {
            rngR = UnityEngine.Random.Range(0, 101);
            rngG = UnityEngine.Random.Range(0, 101);
            rngB = UnityEngine.Random.Range(0, 101);
            buttonTexts[i].color = new Color((float)rngR / 100, (float)rngG / 100, (float)rngB / 100, 1f);

        }
        while (numbersWanged == 0)
        {
            for (int i = 0; i < 12; i++)
            {
                test = UnityEngine.Random.Range(0, 4);
                switch (test)
                {
                    case 0:
                        buttonNumbers.Add(UnityEngine.Random.Range(0, 10));
                        break;
                    case 1:
                        buttonNumbers.Add(UnityEngine.Random.Range(10, 100));
                        break;
                    case 2:
                        buttonNumbers.Add(UnityEngine.Random.Range(100, 1000));
                        break;
                    case 3:
                        buttonNumbers.Add(UnityEngine.Random.Range(1000, 10000));
                        break;
                }
                // buttonNumbers.Add(UnityEngine.Random.Range(0,10000));
                if (IsNumberwang(buttonNumbers[i]))
                {
                    numbersWanged++;
                    Debug.LogFormat("[NumberWang #{0}] {1} is NumberWang", moduleId, i);
                }
                buttonsWanged.Add(IsNumberwang(buttonNumbers[i]));
            }
            if (numbersWanged == 0)
            {
                buttonsWanged.Clear();
                buttonNumbers.Clear();
            }
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            buttonTexts[i].text = buttonNumbers[i].ToString();
        }
        Audio.PlaySoundAtTransform("intro", transform);
        Debug.Log(numbersWanged);
    }

    void ButtonPress(KMSelectable pressedButton)
    {
        if (moduleSolved)
        {
            return;
        }
        for (int i = 0; i < buttons.Length; i++)
        {
            if (pressedButton != buttons[i]) continue;

            if (buttonsPressed.Contains(i))
            {
                GetComponent<KMBombModule>().HandleStrike();
                screenText.text = "You've been\nWangerNumbed!";
                StartCoroutine(HandleScreen());
                Debug.LogFormat("[NumberWang #{0}] Strike! Button {1} has been pressed before.", moduleId, i);
            }
            else
            {
                if (!buttonsWanged[i])
                {
                    GetComponent<KMBombModule>().HandleStrike();
                    screenText.text = "You've been\nWangerNumbed!";
                    StartCoroutine(HandleScreen());
                    Debug.LogFormat("[NumberWang #{0}] Strike! Button {1} is not NumberWang.", moduleId, i);
                }
                else
                {
                    buttonsPressed.Add(i);
                    Debug.LogFormat("[NumberWang #{0}] Correct Press, Button {1} is NumberWang", moduleId, i);
                }
            }
        }

        if ((buttonsPressed.Count() + 1) == numbersWanged && boardRotated == false)
        {
            back = UnityEngine.Random.Range(0, 2);
            rotateBoardBack.GetComponent<MeshRenderer>().material = pictures[back];
            screenText.text = "Let's Rotate the Board!";
            StartCoroutine(HandleScreen());
            StartCoroutine(LetsRotateTheBoard());
            Debug.LogFormat("[NumberWang #{0}] Let's Rotate the Board!", moduleId);
            boardRotated = true;
        }
        else if (buttonsPressed.Count() == numbersWanged)
        {
            GetComponent<KMBombModule>().HandlePass();
            screenText.text = "That's NumberWang!";
            moduleSolved = true;
            Debug.LogFormat("[NumberWang #{0}] Module Solved!", moduleId);
        }
    }

    IEnumerator HandleScreen()
    {
        yield return new WaitForSeconds(2f);
        screenText.text = "";
    }

    IEnumerator LetsRotateTheBoard() // thanks eXish
    {
        int rotation = 0;
        {
            while (rotation != 720)
            {
                yield return new WaitForSeconds(0.01f);
                board.transform.localRotation = Quaternion.Euler(0.0f, 0.0f, 0.50f) * board.transform.localRotation;
                rotation++;
            }
        }
    }
    /// <summary>
    /// Returns true if the number is NumberWang.
    /// </summary>
    /// <param name="number">Number to check</param>
    /// <returns>True / False</returns>
    bool IsNumberwang(int number)
    {
        if (alwaysWang.Contains(number)) 
            return true;
        
        if (Bomb.GetSerialNumber().Any(ch => "AEIOU".Contains(ch)) && serialWang.Contains(number))
            return true;

        if ((Bomb.GetOnIndicators().Count() > Bomb.GetOffIndicators().Count()) && litWang.Contains(number)) // thanccs xEish 
            return true;

        if ((Bomb.GetBatteryHolderCount() > 3) && batteryWang.Contains(number))
            return true;

        if ((Bomb.IsPortPresent(Port.Parallel) && Bomb.IsPortPresent(Port.Serial)) && parallelWang.Contains(number))
            return true;

        if (Bomb.GetBatteryCount() > 1)
            if (number % Bomb.GetBatteryCount() == 0) 
                return true; 

        if ((Bomb.GetOffIndicators().Count() - Bomb.GetOnIndicators().Count()) > 1)
            if (number % (Bomb.GetOffIndicators().Count() - Bomb.GetOnIndicators().Count()) == 0) 
                return true; 

        if (Bomb.GetPortCount() > 1)
            if (number % Bomb.GetPortCount() == 0) 
                return true;

        return false;

    }

}

