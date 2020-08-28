using UnityEngine;
using UnityEngine.SceneManagement;

/// <summary>
/// Behavior for in game menu
/// </summary>
public class CGameMenuBehaviour : MonoBehaviour
{
    #region Public Methods

    /// <summary>
    /// Continues the Game
    /// </summary>
    public void Continue()
    {
        gameObject.SetActive(false);
        Time.timeScale = 1;
    }

    /// <summary>
    /// Returns to the Main Menu
    /// </summary>
    public void ReturnToMenu()
    {
        SceneManager.LoadScene(0);
    }

    #endregion
}
