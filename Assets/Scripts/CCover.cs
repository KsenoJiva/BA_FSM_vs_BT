using UnityEngine;

/// <summary>
/// Info about Cover
/// </summary>
public class CCover : MonoBehaviour
{
    #region Public Enums

    /// <summary>
    /// Types of different Cover
    /// </summary>
    public enum ECoverType
    {
        NONE,
        HALF,
        FULL,
        COUNT
    }

    #endregion

    #region Exposed Variables

    [SerializeField, Tooltip("Type of Cover")]
    private ECoverType m_coverType;

    #endregion

    #region Properties

    /// <summary>
    /// This covers type
    /// </summary>
    public ECoverType CoverType { get { return m_coverType; } }

    #endregion
}
