using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Holds and controls methods for the main menu
/// </summary>
public class CMenuBehaviour : MonoBehaviour
{
    #region Exposed Variables

    [SerializeField]
    private CGameManager m_gameManager;

    #endregion

    #region Public Methods

    /// <summary>
    /// Starts a new Game with the given AI model
    /// </summary>
    /// <param name="_mode"> The Game mode which will be started </param>
    public void StartGame(int _mode)
    {
        GameMode tmpMode = _mode >= (int)GameMode.COUNT ? GameMode.FSM : (GameMode)_mode;   // Catch exception when given number has no existing game mode
        m_gameManager.SetAIModel(tmpMode);              // Set Game mode
        SceneManager.LoadScene(1);                      // Load Game
    }

    /// <summary>
    /// Quits the Application
    /// </summary>
    public void Quit()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
        return;
#endif
        Application.Quit();
    }

    #endregion
}
