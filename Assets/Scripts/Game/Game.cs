using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public const float fadeOutTime = 2f;
    IGLvlEditor lvlEditor;

    //---------------------------------------------------------------------------------------------------------------------------------------------------------

    public static Game instance { get; private set; }
    public int levelNumber { get; private set; }
    public Level level;
    public Level[] levels;
    public Omino _omino { get; private set; }
    public bool playing;
    public bool testPlaying;

//---------------------------------------------------------------------------------------------------------------------------------------------------------

private void Awake()
	{
		instance = this;
        levels = ResourceLoader.GetAll<Level>();
        LoadLevel(1);
        playing = false;
        testPlaying = false;
    }

	public void Win()
	{
        if (!testPlaying)
        {
            LoadLevel(levelNumber % levels.Length + 1);
            Play();
        }
    }
	
	public void LoadLevel(int number)
	{
        levelNumber = number;
        if (level)
            Destroy(level.gameObject);
        level = Instantiate(levels[number-1]);
        level.transform.SetParent(instance.transform);
	}

    public void Restart() => LoadLevel(levelNumber);

    public void Play()
    {
        playing = true;
        if (level != null)
        {
            _omino = level.GetComponentInChildren<Omino>();
            _omino.enabled = true;
        }
    }

    public void TestPlay()
    {
        Play();
        testPlaying = true;
    }

    public void Pause()
    {
        playing = false;
        testPlaying = false;
        if (level != null)
        {
            _omino = level.GetComponentInChildren<Omino>();
            _omino.enabled = false;
        }
    }
}
