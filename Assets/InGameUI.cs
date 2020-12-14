using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class InGameUI : MonoBehaviour
{
    public Game game;
    public Text levelText;

    private int lastLevel;
    // Start is called before the first frame update
    void Start()
    {
        lastLevel = -1;
    }

    // Update is called once per frame
    void Update()
    {
        if (game && game.levelNumber != lastLevel) {
            levelText.text = "LEVEL " + game.levelNumber.ToString();
        }
    }
}
