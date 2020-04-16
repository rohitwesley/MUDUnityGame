using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using GameLogic;
using TMPro;

public class GameState : MonoBehaviour
{
    [Tooltip("Pick Up UI Text")]
    [SerializeField] private TextMeshProUGUI scoreWidgetText;
    [SerializeField] private TextMeshProUGUI pickupWidgetText;
    [SerializeField] private List<Agents> units;

    public static int pickUpsCount = 0;
    public static int pickUpsTotal = 3;
    void Update()
    {
        foreach (Agents agent in units)
        {
            if(agent.GetAgentName() == "Player")
            {
                scoreWidgetText.text = "Score: " + agent.scoreValue;
                pickupWidgetText.text = "Loot: " + pickUpsCount;
            }
        }

    }

}
