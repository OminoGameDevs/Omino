using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelSelect : MonoBehaviour
{
    public Sprite[] levelIcons;
    public Level[] levels;
    public GameObject levelImage;
    public Text levelTitle;

    private int levelIndex;
    private int lastLevelIndex;
    private int maxLevel;
    // Start is called before the first frame update
    void Start()
    {
        levelIndex = 0;
        lastLevelIndex = 0;
    }

    // Update is called once per frame
    void Update()
    {
        maxLevel = levelIcons.Length;
        if (levelImage) {
            if (lastLevelIndex != levelIndex) {
                lastLevelIndex = levelIndex;
                levelImage.GetComponent<Image>().sprite = levelIcons[levelIndex];
                int level = levelIndex + 1;
                levelTitle.text = "LEVEL " + level.ToString();
            }
        }
    }

    public void IncLevel() {
        levelIndex++;
        levelIndex %= maxLevel;
    }

    public void DecLevel() {
        levelIndex--;
        if (levelIndex < 0 ) {
            levelIndex = maxLevel - 1;
        }
    }

    public void LoadLevelByIndex() {
        Game[] game = FindObjectsOfType<Game>();
        if (game.Length < 1) return;
        game[0].LoadLevel(levelIndex + 1);

    }

    public void UpdateLevel() {
        Game[] game = FindObjectsOfType<Game>();
        if (game.Length < 1) return;
        levelIndex = game[0].levelNumber - 1;
        lastLevelIndex = -1; // to force update

    }
}
