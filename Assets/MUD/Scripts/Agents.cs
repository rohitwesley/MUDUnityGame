
using System.Collections;
using UnityEngine;

namespace GameLogic
{
    /// <summary>
    /// Agent Type ID to recognise type of Agent
    /// </summary>
    public enum UnitType
    {
        Floor,
        Home,
        Player,
        Factory,
        Pawns,
        PickUp
    }
    
    /// <summary>
    /// All agents have there properties in this class
    /// Should be added to the part of the Agent that has the collider 
    /// </summary>
    public class Agents : MonoBehaviour
    {

        public UnitType unitType = UnitType.Home;
        public GameObject unitIcon;

        [Tooltip("Points")]
        [SerializeField]
        public int pointsValue = 10;                 // The points value of the object

        [Tooltip("Score")]
        [SerializeField]
        public int scoreValue = 10;                 // The score value of the object

        [Tooltip("Agent Current Health")]                              // The amount of health the player starts the game with.
        public int currentHealth;
        [Tooltip("Agent Starting Health")]                              // The amount of health the player starts the game with.
        [SerializeField]
        protected int startingHealth = 100;                  // The starting health the player has.

        /// <summary>
        /// Hide Icons on start
        /// </summary>
        private void Start()
        {
            unitIcon.SetActive(false);

            Debug.Log("Agent : " + GetAgentName() + "Initialised ");
        }

        /// <summary>
        /// used to activat Icon coroutine
        /// </summary>
        /// <param name="sec"></param>
        public void ShowIcon(float sec)
        {
            StartCoroutine(ShowIconAnimation(sec));
        }

        /// <summary>
        /// Show Icon for given time using Coroutine
        /// </summary>
        /// <param name="sec">seconds to show icons</param>
        /// <returns>pause</returns>
        IEnumerator ShowIconAnimation(float sec)
        {
    
            yield return new WaitForSeconds(0.1f);
            unitIcon.SetActive(true);
            yield return new WaitForSeconds(sec);
            unitIcon.SetActive(false);
        }
        

        public static UnitType GetAgentFromString(string agentType)
        {
            switch (agentType)
            {
                case "Floor": 
                    return UnitType.Floor;
                case "Home": 
                    return UnitType.Home;
                case "Player": 
                    return UnitType.Player;
                case "Factory": 
                    return UnitType.Factory;
                case "Pawns":
                    return UnitType.Pawns;
                case "PickUp":
                    return UnitType.PickUp;
                default:
                    return UnitType.Floor;
            }
        }

        public string GetAgentName()
        {
            switch (unitType)
            {
                case UnitType.Floor: 
                    return "Floor";
                case UnitType.Home: 
                    return "Home";
                case UnitType.Player: 
                    return "Player";
                case UnitType.Factory: 
                    return "Factory";
                case UnitType.Pawns:
                    return "Pawns";
                case UnitType.PickUp:
                    return "PickUp";
                default:
                    return "Floor";
            }
        }
    
    }

}
