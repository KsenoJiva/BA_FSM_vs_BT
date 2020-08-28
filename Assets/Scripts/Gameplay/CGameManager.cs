using UnityEngine;

/// <summary>
/// Modes for the game
/// </summary>
public enum GameMode
{
    FSM,        // vs. Finite State Machine
    BT,         // vs. Behaviour Tree
    TEST,       // FSM vs. BT
    COUNT       // count of total modes for exception handling
}

/// <summary>
/// Saves important Data
/// </summary>
public class CGameManager : MonoBehaviour
{
    #region Properties

    /// <summary>
    /// The currently set AI model
    /// </summary>
    public GameMode CurrentAI { get; private set; }

    #endregion

    #region Public Methods

    /// <summary>
    /// Sets the current AI Model
    /// </summary>
    /// <param name="_model"> The model which will be set </param>
    public void SetAIModel (GameMode _model)
    {
        CurrentAI = _model;
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        DontDestroyOnLoad(this);

        CGameManager[] arr_gameManagers = FindObjectsOfType<CGameManager>();

        // Prevent multiple instances on return to menu
        if (arr_gameManagers.Length > 1)
        {
            Destroy(arr_gameManagers[1].gameObject);
        }
    }

    private void OnLevelWasLoaded(int _level)
    {
        // If game scene is loaded
        if (_level == 1)
        { 
            if(CurrentAI == GameMode.TEST)
                FindObjectOfType<CAIController>().ActivateTurn = true;
            else
                FindObjectOfType<CPlayerControls>().ActivateTurn = true;
        }
    }

    #endregion
}
