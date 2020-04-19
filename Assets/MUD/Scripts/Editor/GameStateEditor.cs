using GameLogic;
using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(GameStateManager))]
public class GameStateManagerEditor : Editor
{
    GameStateManager gameState;
    void OnSceneGUI()
    {
        /*GameState gameState = (GameState)target;*/
        if (gameState == null)
        {
            return;
        }

        // Draw GUI Info in Screen Space
        Handles.BeginGUI();
        foreach (GameStateData playerData in gameState.playerData)
        {
            GUILayout.Label("Game State: " + gameState.GetGameStateName());
            GUILayout.Label("level: " + playerData.level);
            GUILayout.Label("checkpointInLevel: " + playerData.checkpointInLevel);
            GUILayout.Label("pickUpsCollected: " + playerData.pickUpsCollected);
            GUILayout.Label("health: " + playerData.health);
            GUILayout.Label("timeInLevel: " + playerData.timeInLevel);
            GUILayout.Label("scoreValue: " + playerData.scoreValue);
        }
        if (GUILayout.Button("Reset PlayerState", GUILayout.Width(200)))
        {
            gameState.ResetGameData();
        }
        Handles.EndGUI();

    }

    private void OnEnable()
    {
        gameState = (GameStateManager)target;
    }

}
