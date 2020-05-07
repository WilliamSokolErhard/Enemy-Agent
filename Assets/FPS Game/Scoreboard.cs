using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Scoreboard : MonoBehaviour
{
    public static Scoreboard instance;

    public Text text;

    public int KillNeeded = 3;
    // Start is called before the first frame update
    void Start()
    {
        instance = this;
    }

    

    public void deathCounted()
    {
        if (--KillNeeded <= 0)
        {
            text.text = "You Win!";
        }
        else
        {
            text.text = "Kill "+KillNeeded+" Enemies";
        }
    }
}
