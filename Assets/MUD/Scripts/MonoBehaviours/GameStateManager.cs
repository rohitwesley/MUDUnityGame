using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLogic;
using UnityEngine.SceneManagement;

public enum GameState
{
    ActiveLevel,
    ResetLevel,
    NextLevel,
    QuitLevel
}

public class GameStateManager : MonoBehaviour
{
    [Tooltip("Player Agents In the Scene")]
    [SerializeField] public List<Agents> playerUnits;
    [SerializeField] public List<GameStateData> playerData;
    [SerializeField] public GameStateData resetPlayerData;
    [SerializeField] public GameStateData levelData;
    [SerializeField] public ScreenManager screenManager;

    [SerializeField] private KeyCode _EscapeKey = KeyCode.Escape;

    public GameState state = GameState.ActiveLevel;

    void Update()
    {
        foreach (Agents player in playerUnits)
        {
            if(player.GetAgentName() == "Player")
            {
                
            }
        }

        CheckGameState();
    }

    private void CheckGameState()
    {
        foreach (GameStateData playerData in playerData)
        {
            if (Time.frameCount % 10 == 0)
                playerData.timeInLevel -= 0.1f;

            if (playerData.timeInLevel <= 0)
            {
                screenManager.CallLoose();
                state = GameState.ResetLevel;
                StartCoroutine(EndLevel(state));
                ResetLevelData();
            }
            if (playerData.checkpointInLevel >= levelData.checkpointInLevel)
            {
                screenManager.CallWin();
                state = GameState.NextLevel;
                StartCoroutine(EndLevel(state));
                ResetLevelData();
            }
        }

        if (Input.GetKeyDown(_EscapeKey))
        {
            screenManager.CallMenu();
            //QuitSequence();
        }
    }

    public void QuitSequence()
    {
        state = GameState.QuitLevel;
        ResetGameData();
        StartCoroutine(EndLevel(state));
    }

    public void UpdateScore(Agents currentPlayer, int score)
    {
        for (int index = 0; index < playerUnits.Count; index++)
        {
            if (playerUnits[index] == currentPlayer)
            {
                playerData[index].scoreValue += score;
            }
        }
    }
    public void UpdatePickups(Agents currentPlayer)
    {
        for (int index = 0; index < playerUnits.Count; index++)
        {
            if (playerUnits[index] == currentPlayer)
            {
                playerData[index].pickUpsCollected++;
            }
        }
    }
    public void UpdateCheckpoints(Agents currentPlayer)
    {
        for (int index = 0; index < playerUnits.Count; index++)
        {
            if (playerUnits[index] == currentPlayer)
            {
                playerData[index].checkpointInLevel++;
            }
        }
    }

    /// <summary>
    /// Reset Game Specific Data
    /// </summary>
    public void ResetGameData()
    {
        foreach (GameStateData playerData in playerData)
        {
            playerData.level = resetPlayerData.level;
            playerData.scoreValue = resetPlayerData.scoreValue;
        }
        ResetLevelData();
    }

    /// <summary>
    /// Reset Level Specific Data
    /// </summary>
    public void ResetLevelData()
    {
        foreach (GameStateData playerData in playerData)
        {
            playerData.level++;
            playerData.health = resetPlayerData.health;
            playerData.checkpointInLevel = resetPlayerData.checkpointInLevel;
            playerData.pickUpsCollected = resetPlayerData.pickUpsCollected;
            playerData.timeInLevel = levelData.timeInLevel;
        }
    }

    /// <summary>
    /// Test for end case
    /// </summary>
    /// <param name="setState"></param>
    public IEnumerator EndLevel(GameState setState)
    {
        yield return new WaitForSeconds(2.0f);
        if (setState == GameState.ResetLevel)
        {
            // Reload the level that is currently loaded.
            string sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
        }
        else if (setState == GameState.NextLevel)
        {
            // TODO Load the next level
            // Start loading the given scene and wait for it to finish.
            string sceneName = SceneManager.GetActiveScene().name;
            SceneManager.LoadScene(SceneManager.GetActiveScene().name);
            //StartCoroutine(LoadSceneAndSetActive(sceneName));
            //SceneManager.LoadScene(menuSceenName);
        }
        else if (setState == GameState.QuitLevel)
        {
            // Quit Game
            Quit();
        }
    }
   
    /// <summary>
    /// TODO Load Scene in background with loading bar
    /// </summary>
    /// <param name="sceneName"></param>
    /// <returns></returns>
    public IEnumerator LoadSceneAndSetActive(string sceneName)
    {
        // Allow the given scene to load over several frames and add it to the already loaded scenes (just the Persistent scene at this point).
        yield return SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Additive);

        // Find the scene that was most recently loaded (the one at the last index of the loaded scenes).
        Scene newlyLoadedScene = SceneManager.GetSceneAt(SceneManager.sceneCount - 1);

        // Set the newly loaded scene as the active scene (this marks it as the one to be unloaded next).
        SceneManager.SetActiveScene(newlyLoadedScene);
    }

    /// <summary>
    /// Quit application
    /// </summary>
    private void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
    Application.Quit();
#endif
    }

    /// <summary>
    /// Game Stae Helper functions
    /// </summary>
    /// <param name="gameStateName"></param>
    /// <returns></returns>
    public static GameState GetGameStateFromString(string gameStateName)
    {
        switch (gameStateName)
        {
            case "ActiveLevel":
                return GameState.ActiveLevel;
            case "ResetLevel":
                return GameState.ResetLevel;
            case "NextLevel":
                return GameState.NextLevel;
            case "QuitLevel":
                return GameState.QuitLevel;
            default:
                return GameState.ActiveLevel;
        }
    }
    public string GetGameStateName()
    {
        switch (state)
        {
            case GameState.ActiveLevel:
                return "ActiveLevel";
            case GameState.ResetLevel:
                return "ResetLevel";
            case GameState.NextLevel:
                return "NextLevel";
            default:
                return "ActiveLevel";
        }
    }


}
