using UnityEngine;

/// <summary>
/// Info about Shot
/// </summary>
public class CShot : MonoBehaviour
{
    #region Properties

    /// <summary>
    /// Linerender for visuals
    /// </summary>
    public LineRenderer LineRend { get; private set; }

    /// <summary>
    /// Is this hsot in currently in use
    /// </summary>
    public bool Used { get; set; } = false;

    /// <summary>
    /// This shots age
    /// </summary>
    public float Timer { get; private set; } = 0.0f;

    #endregion

    #region Methods

    #region Public Mehtods

    /// <summary>
    /// Inscreases shots timer/age
    /// </summary>
    /// <param name="_toAdd"> Delta to add </param>
    public void AddTime(float _toAdd) { Timer += _toAdd; }

    /// <summary>
    /// Resets values to default for reuse
    /// </summary>
    public void ResetShot() { Timer = 0.0f; Used = false; }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        LineRend = GetComponent<LineRenderer>();
    }

    #endregion

    #endregion
}
