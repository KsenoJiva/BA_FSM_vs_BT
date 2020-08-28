using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Class for managing shot pool
/// </summary>
public class CShotManager : MonoBehaviour
{
    #region Variables

    #region Private Variables

    private List<CShot> m_list_shots = new List<CShot>();

    #endregion

    #region Exposed Variables

    [SerializeField]
    private GameObject m_shotPrefab;

    [SerializeField, Range(0.0f, 5.0f), Tooltip("Time until visible shot disappears")]
    private float m_timeToDestroy;

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Time until Shot disappears
    /// </summary>
    public float TimeToDestroy { get { return m_timeToDestroy; } }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Places a shot and makes it visible
    /// </summary>
    /// <param name="_start"> Origion of shot </param>
    /// <param name="_end"> Target of shot </param>
    public void PlaceShot(Vector3 _start, Vector3 _end)
    {
        CShot shot = null;

        //Reuse existing shots not in use
        if (m_list_shots.Count != 0)
        {
            for (int i = 0; i < m_list_shots.Count; i++)
            {
                if (!m_list_shots[i].Used)
                {
                    shot = m_list_shots[i];
                    break;
                }
            }
        }
        
        //Instantiate new shot when pool is to small
        if(shot == null)
        {
            GameObject go = Instantiate(m_shotPrefab);
            shot = go.GetComponent<CShot>();
            m_list_shots.Add(shot);
        }

        //Setup shot and show
        shot.LineRend.SetPositions(new Vector3[] { _start, _end });
        shot.Used = true;
        shot.gameObject.SetActive(true);
    }

    /// <summary>
    /// Resets shot and makes it invisible
    /// </summary>
    /// <param name="_shot"> Which instance should be removed </param>
    public void RemoveShot(CShot _shot)
    {
        _shot.ResetShot();
        _shot.gameObject.SetActive(false);
    }

    #endregion

    #region Unity Methods

    private void Update()
    {
        //Remove shots after timeout
        foreach(CShot shot in m_list_shots)
        {
            shot.AddTime(Time.deltaTime);

            if(shot.Timer > m_timeToDestroy)
            {
                RemoveShot(shot);
            }
        }
    }

    #endregion

    #endregion
}
