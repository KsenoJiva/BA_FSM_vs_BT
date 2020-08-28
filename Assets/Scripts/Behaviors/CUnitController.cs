using System.Collections.Generic;
using UnityEngine.AI;
using UnityEngine;

/// <summary>
/// Controls individual Unit
/// </summary>
public class CUnitController : MonoBehaviour
{
    #region Variables

    #region Member Variables

    private Vector3 m_navPos;
    private bool m_higlighted;
    private CAIController m_aiCtrl;
    private CShotManager m_shotManager;

    #endregion

    #region Exposed Variables

    [SerializeField, Range(1, 100), Tooltip("Hit chance for dealing damage")]
    private int m_hitChance;

    [SerializeField, Range(0.0f, 100.0f), Tooltip("Factor to reduce precision on distant shots (linear)")]
    private float m_distPRecisionReductionMuliplier = 15.0f;

    [SerializeField, Range(0.0f, 20.0f), Tooltip("Distance from which precision will drop")]
    private float m_distPrecisionReductionStart = 5.0f;

    [SerializeField, Range(1.0f, 10.0f), Tooltip("Distance to cover to take effect")]
    private float m_distToEffectiveCover = 2.5f;

    [SerializeField, Range(0, 30), Tooltip("Maximum movement distance")]
    private int m_moveRadius = 10;

    [SerializeField, Range(0, 100), Tooltip("Shot accuracy penalty for half covered target")]
    private int m_halfCoverPenalty = 20;

    [SerializeField, Range(0,100), Tooltip("Shot accuracy penalty for full covered target")]
    private int m_fullCoverPenalty = 40;

    [SerializeField, Range(1, 100), Tooltip("Average Damage with each shot")]
    private float m_avgDamage;

    [SerializeField, Range(1, 100), Tooltip("Damage Fluctuation")]
    private float m_damageRng;

    [SerializeField]
    private GameObject m_shotAnchor;

    [SerializeField]
    private GameObject m_highlightFX;

    #endregion

    #region Public Variables

    [Range(1, 200), Tooltip("Maximum Health")]
    public float m_MaxHealth;

    /// <summary>
    /// Linerenderer for possible path
    /// </summary>
    public LineRenderer m_MoveLine;

    /// <summary>
    /// NavAgent of hte active unit
    /// </summary>
    public NavMeshAgent m_NavAgent;

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Health of this Unit
    /// </summary>
    public float Health { get; private set; }

    /// <summary>
    /// Determines wether this unit can perform an Action
    /// </summary>
    public bool ActionAllowed { get; set; } = true;

    /// <summary>
    /// Has this unit successfully finished all actions
    /// </summary>
    public bool FinishedMove { get; set; }

    /// <summary>
    /// Is this unit highlighted as active
    /// </summary>
    public bool Highlighted
    {
        get { return m_higlighted; }
        set
        {
            m_higlighted = value;
            m_highlightFX.SetActive(Highlighted);
        }
    }

    /// <summary>
    /// Maximum movement distance
    /// </summary>
    public int MoveRadius
    {
        get { return m_moveRadius; }
    }

    #endregion

    #region Methods

    #region Private Methods

    /// <summary>
    /// Reduces Health
    /// </summary>
    /// <param name="_value"> Value by which the Health will be reduced </param>
    private void TakeDamage(float _value)
    {
        Health = Mathf.Max(0, Health - Mathf.Abs(_value));
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Shoot at a given Unit
    /// </summary>
    /// <param name="_target"> Which Unit wil be targeted </param>
    /// <returns></returns>
    public bool Shoot(CUnitController _target)
    {
        //Stop when not allowed
        if(!ActionAllowed)
            return false;

        Vector3 offset = Vector3.zero;
        int coverPenalty  = 0;

        // Apply cover penalty
        switch(CheckCoverOfTarget(_target, transform.position))
        {
            case CCover.ECoverType.NONE:
                coverPenalty = 0;
                break;
            case CCover.ECoverType.HALF:
                coverPenalty = m_halfCoverPenalty;
                break;
            case CCover.ECoverType.FULL:
                coverPenalty = m_fullCoverPenalty;
                break;
        }

        //Apply distance penalty
        int distPenalty = Mathf.RoundToInt(Mathf.Max(0, Vector3.Distance(_target.transform.position, transform.position) - m_distPrecisionReductionStart) * m_distPRecisionReductionMuliplier);

        // Hit check
        if (Random.Range(0, 100) <= (Mathf.Max(0, m_hitChance - coverPenalty - distPenalty)))
        {
            // Calculate damage value
            float damage = Random.Range(m_avgDamage - m_damageRng, m_avgDamage + m_damageRng);

            // Damage target
            _target.TakeDamage(damage);
        }

        // Look at target
        gameObject.transform.rotation.SetLookRotation(_target.gameObject.transform.position - gameObject.transform.position);

        // Offset for missed shot
        offset = new Vector3(Random.Range(-0.5f, 0.5f), Random.Range(-1, 1), Random.Range(-0.5f, 0.5f));

        // Visible Shot
        Vector3 shotTarget = new Vector3(_target.gameObject.transform.position.x, m_shotAnchor.transform.position.y,
                                        _target.gameObject.transform.position.z) + offset;     // Point of Impact for the Shot
        m_shotManager.PlaceShot(m_shotAnchor.transform.position, shotTarget);

        ActionAllowed = false;  // Actions depleted

        return true;
    }

    /// <summary>
    /// Checks for any active Cover of the target
    /// </summary>
    /// <param name="_targetUnit"> Targeted unit to be shot at </param>
    /// <param name="_fromPoint"> Check from this point </param>
    /// <returns> Active covertype which has an effect on the shot </returns>
    public CCover.ECoverType CheckCoverOfTarget(CUnitController _targetUnit, Vector3 _fromPoint)
    {
        //Get close cover
        List<CCover> list_NearCovers = new List<CCover>();

        foreach(CCover cover in m_aiCtrl.Array_Covers)
        {
            // Only near cover at the target will affect the shot
            if(Vector3.Distance(_targetUnit.transform.position, cover.transform.position) <= m_distToEffectiveCover)
                list_NearCovers.Add(cover);
        }

        CCover.ECoverType returnValue = CCover.ECoverType.NONE;

        // Set Bounds
        float minXPosBoundary = float.MinValue;
        float minZPosBoundary = float.MinValue;
        float maxXPosBoundary = float.MaxValue;
        float maxZPosBoundary = float.MaxValue;

        minXPosBoundary = transform.position.x < _targetUnit.transform.position.x ? _fromPoint.x : _targetUnit.transform.position.x;
        minZPosBoundary = transform.position.z < _targetUnit.transform.position.z ? _fromPoint.z : _targetUnit.transform.position.z;
        maxXPosBoundary = transform.position.x > _targetUnit.transform.position.x ? _fromPoint.x : _targetUnit.transform.position.x;
        maxZPosBoundary = transform.position.z > _targetUnit.transform.position.z ? _fromPoint.z : _targetUnit.transform.position.z;

        // Check for flanked
        bool xFlanked;
        bool zFlanked;

        foreach(CCover cover in list_NearCovers)
        {
            // Check for outflanked
            // Flanked when cover position is not in between
            xFlanked = !(cover.transform.position.x > minXPosBoundary && cover.transform.position.x < maxXPosBoundary);
            zFlanked = !(cover.transform.position.z > minZPosBoundary && cover.transform.position.z < maxZPosBoundary);

            // Set active cover; Full cover get chosen over half cover
            if((!xFlanked || !zFlanked) && (int)cover.CoverType >= (int)returnValue)
                returnValue = cover.CoverType;
        }

        return returnValue;
    }

    /// <summary>
    /// Move towards a given Position
    /// </summary>
    /// <param name="_targetPos"> Position which will be moved towards </param>
    public void Move(Vector3 _targetPos)
    {
        //stop if not allowed
        if(!ActionAllowed)
            return;

        //set destination
        m_navPos = new Vector3(_targetPos.x, 0, _targetPos.z);
        m_NavAgent.SetDestination(m_navPos);

        ActionAllowed = false;
    }

    /// <summary>
    /// Returns the next unit which can perform actions
    /// </summary>
    /// <param name="_list"> List of units to search </param>
    /// <returns> First unit with available actions </returns>
    public static CUnitController NextAvailableUnit(List<CUnitController> _list)
    {
        //search through all units and check for finished moves and those that have actions left
        foreach (CUnitController unit in _list)
        {
            if(!unit.FinishedMove && unit.ActionAllowed)
                return unit;
        }

        return null;
    }

    #endregion

    #region Unity Methods

    private void Start()
    {
        m_NavAgent = GetComponent<NavMeshAgent>();
        m_MoveLine = GetComponent<LineRenderer>();
        m_aiCtrl = FindObjectOfType<CAIController>();
        m_shotManager = FindObjectOfType<CShotManager>();

        Health = m_MaxHealth;
        m_navPos = transform.position;
    }

    private void Update()
    {
        //Check for death
        if(Health <= 0)
        {
            if (m_aiCtrl.List_PlayerUnits.Contains(this))
                m_aiCtrl.List_PlayerUnits.Remove(this);

            if (m_aiCtrl.List_AIUnits.Contains(this))
                m_aiCtrl.List_AIUnits.Remove(this);

            Destroy(gameObject);
        }
    }

    private void FixedUpdate()
    {
        // Move if target is not reached yet
        if (Vector3.Distance(m_navPos, gameObject.transform.position) < 1.0f && !ActionAllowed)
        {
            FinishedMove = true;
            m_navPos = gameObject.transform.position;
            m_NavAgent.ResetPath();
        }
        else
            m_NavAgent.SetDestination(m_navPos);
    }

    #endregion

    #endregion
}
