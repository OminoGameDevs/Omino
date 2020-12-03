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
    public Level level { get; private set; }
    public Level[] levels;
    public Omino _omino { get; private set; }

//---------------------------------------------------------------------------------------------------------------------------------------------------------

private void Awake()
	{
		instance = this;
        levels = ResourceLoader.GetAll<Level>();
        LoadLevel(1);
	}

	public void Win()
	{
		LoadLevel(levelNumber % levels.Length + 1);
        toggleEnable();
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

    public void toggleEnable()
    {
        _omino = level.GetComponentInChildren<Omino>();
        _omino.enabled = !_omino.enabled;
    }
}
