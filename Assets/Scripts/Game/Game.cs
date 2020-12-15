using UnityEngine;
using System.Collections;
using UnityEngine.SceneManagement;

public class Game : MonoBehaviour
{
    public static Game instance { get; private set; }
    public int levelNumber { get; private set; }
    public Level level { get; set; }
    public Level[] levels { get; set; }

    public IGLvlEditor lvlEditor;
    private Omino omino;
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
            BroadcastMessage("OnLevelEnd", SendMessageOptions.DontRequireReceiver);
            Invoke("NextLevel", Constants.fadeOutTime);
        }
	}

    private void NextLevel()
    {
        LoadLevel(levelNumber % levels.Length + 1);
    }
	
	public void LoadLevel(int number)
	{
        levelNumber = number;
        if (level)
            Destroy(level.gameObject);
        level = Instantiate(levels[number-1]);
        level.transform.SetParent(instance.transform);

        BroadcastMessage("OnLevelStart", SendMessageOptions.DontRequireReceiver);
	}

    public void Restart() => LoadLevel(levelNumber);

    public void Play()
    {
        playing = true;
        if (level != null)
        {
            omino = level.GetComponentInChildren<Omino>();
            omino.enabled = true;
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
            omino = level.GetComponentInChildren<Omino>();
            omino.enabled = false;
        }
    }
}
