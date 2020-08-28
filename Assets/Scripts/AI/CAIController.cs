using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using System.Linq;

/// <summary>
/// Base Class for specialised AI Classes (FSM and BT)
/// </summary>
public class CGeneralAI : MonoBehaviour
{
    #region Variables

    #region Protected Variables

    protected List<CUnitController> m_list_myUnits;
    protected List<CUnitController> m_list_enemyUnits;

    protected CAIController m_aiController;
    protected CUnitController m_activeUnit;

    protected float m_healthPoolOwn;
    protected float m_healthPoolEnemy;

    protected float m_shotTimer;

    #endregion

    #region Exposed Variables

    [SerializeField, Range(0.0f, 5.0f), Tooltip("Time to wait after shooting for AI")]
    protected float m_timeAfterShot = 3.0f;
    
    [SerializeField, Range(0.0f, 1.0f), Tooltip("Percentage for switching to \"Stay\" state")]
    protected float m_ToStay_HealthPerc = 0.9f;

    [SerializeField, Range(0.0f, 1.0f), Tooltip("Percentage for switching to \"Retreat\" state")]
    protected float m_ToRetreat_HealthPerc = 0.5f;

    [SerializeField, Range(0.0f, 20.0f), Tooltip("Cummulative Distance for switching to  \"Stay\" state")]
    protected float m_ToStay_Distance = 15.0f;

    [SerializeField, Range(0.0f, 15.0f), Tooltip("Closest Proximity Distance of between team units for switching to \"Stay\" state")]
    protected float m_ToStay_CloseProximity_Distance = 5.0f;

    #endregion

    #endregion

    #region Properties

    public bool IsOnTurn { get; set; }

    /// <summary>
    /// The average position of enemies
    /// </summary>
    public Vector3 CummulativeTarget
    {
        get
        {
            Vector3 cummulativeTarget = new Vector3();

            foreach (CUnitController enemy in m_list_enemyUnits)
                cummulativeTarget += enemy.transform.position;

            cummulativeTarget /= m_list_enemyUnits.Count;

            return cummulativeTarget;
        }
    }

    /// <summary>
    /// The average position of own units
    /// </summary>
    public Vector3 CummulativePos
    {
        get
        {
            Vector3 cummulativePos = new Vector3();

            foreach (CUnitController unit in m_list_myUnits)
                cummulativePos += unit.transform.position;

            cummulativePos /= m_list_myUnits.Count;

            return cummulativePos;
        }
    }

    /// <summary>
    /// The closest Enemy to the active unit
    /// </summary>
    public CUnitController ClosestEnemy
    {
        get
        {
            CUnitController toReturn = m_list_enemyUnits.FirstOrDefault();

            if (toReturn == null)
                return null;

            foreach (CUnitController unit in m_list_enemyUnits)
            {
                toReturn = Vector3.Distance(unit.transform.position, m_activeUnit.transform.position)
                    < Vector3.Distance(toReturn.transform.position, m_activeUnit.transform.position)
                    ? unit : toReturn;
            }

            return toReturn;
        }
    }

    #endregion

    #region Public Methods

    #region Virtual Methods

    /// <summary>
    /// Actions for this turn
    /// </summary>
    public virtual void Turn()
    {
        m_healthPoolOwn = 0;
        m_healthPoolEnemy = 0;

        //Calculate Healthpools
        foreach (CUnitController unit in m_list_myUnits)
            m_healthPoolOwn += unit.Health;

        foreach (CUnitController unit in m_list_enemyUnits)
            m_healthPoolOwn += unit.Health;

        m_activeUnit = m_list_myUnits[0];
    }

    #endregion

    #endregion
}

/// <summary>
/// Container for AI relevant information
/// </summary>
public class CAIController : MonoBehaviour
{
    #region Private Enums

    /// <summary>
    /// possible winnercases
    /// </summary>
    private enum EWinner
    {
        NONE,
        PLAYER,
        ENEMY
    }

    #endregion

    #region Variables

    #region Private Variables

    private CGeneralAI m_firstAI = null;
    private CGeneralAI m_secondAI = null;
    private CPlayerControls m_playerCtrl;
    private int m_roundCount = 1;
    private Text m_TextRound;
    private Text m_TextOnTurn;

    #endregion

    #region Exposed Variables

    [SerializeField]
    private GameObject m_roundCountGO;

    [SerializeField]
    private GameObject m_onTurnUIGO;

    [SerializeField]
    private GameObject m_endScreen;

    [SerializeField]
    private GameObject m_winnerMessage;

    [SerializeField]
    private GameObject m_endMessage;

    #endregion

    #endregion

    #region Properties

    /// <summary>
    /// Flag for recognising if turn started against player
    /// </summary>
    public bool PlayerEndTurn { get; set; }
    
    /// <summary>
    /// List of all Units controlled by the AI
    /// </summary>
    public List<CUnitController> List_AIUnits { get; private set; } = new List<CUnitController>();

    /// <summary>
    /// List of all Units controlled by the Player
    /// </summary>
    public List<CUnitController> List_PlayerUnits { get; private set; } = new List<CUnitController>();

    /// <summary>
    /// Contains all cover objects
    /// </summary>
    public CCover[] Array_Covers { get; private set; }

    /// <summary>
    /// Game manager of this application
    /// </summary>
    public CGameManager GameManager { get; private set; }

    /// <summary>
    /// Has the turn just started
    /// </summary>
    public bool ActivateTurn { get; set; }

    #endregion

    #region Methods

    #region Private Methods

    /// <summary>
    /// Sets up endscreen
    /// </summary>
    /// <param name="_winner"> who won </param>
    private void GameOver(EWinner _winner)
    {
        //set text
        m_winnerMessage.GetComponent<Text>().text = _winner == EWinner.PLAYER ? "DU" : "DEIN GEGNER";
        m_endMessage.GetComponent<Text>().text = _winner == EWinner.PLAYER ? "hast gewonnen!" : "hat gewonnen!";

        //open and pause
        m_endScreen.SetActive(true);
        Time.timeScale = 0;
    }

    #endregion

    #region Public Methods

    /// <summary>
    /// Call correct specific AI turn
    /// </summary>
    public void Turn()
    {
        //Change Turn Prompt
        m_TextOnTurn.text = "Der Gegner ist am Zug";

        //When against player
        if (PlayerEndTurn)
            m_firstAI.IsOnTurn = true;
            PlayerEndTurn = false;

        //When in testmode
        if(m_firstAI.IsOnTurn)
            m_firstAI.Turn();
        else if(m_secondAI != null && m_secondAI.IsOnTurn)
            m_secondAI.Turn();
    }

    /// <summary>
    /// Ends the AI turn
    /// </summary>
    public void EndTurn()
    {
        //increase roundcount and update ui
        m_TextRound.text = (++m_roundCount).ToString();

        // when against player
        if (GameManager.CurrentAI == GameMode.FSM || GameManager.CurrentAI == GameMode.BT)
        {
            m_firstAI.IsOnTurn = false;
            m_TextOnTurn.text = "Du bist dran !";
            m_playerCtrl.Turn();
        }
        //when in testmode
        else if (GameManager.CurrentAI == GameMode.TEST)
        {
            m_firstAI.IsOnTurn = !m_firstAI.IsOnTurn;
            m_secondAI.IsOnTurn = !m_secondAI.IsOnTurn;
            m_TextOnTurn.text = m_firstAI.IsOnTurn ? "FSM ist am Zug" : "BT ist am Zug";
            Turn();
        }
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        GameManager = FindObjectOfType<CGameManager>();
        m_playerCtrl = FindObjectOfType<CPlayerControls>();
    }

    private void Start()
    {
        // Set the AIs according to the selected mode
        switch(GameManager.CurrentAI)
        {
            case GameMode.FSM:
                m_firstAI = GetComponent<CFiniteStateMachine>();
                break;
            case GameMode.BT:
                m_firstAI = GetComponent<CBehaviourTree>();
                break;
            case GameMode.TEST:
                m_firstAI = GetComponent<CFiniteStateMachine>();
                m_secondAI = GetComponent<CBehaviourTree>();
                m_firstAI.IsOnTurn = true;
                break;

        }

        CUnitController[] array_AllUnits = FindObjectsOfType<CUnitController>();

        // Fill all Lists
        foreach (CUnitController unit in array_AllUnits)    
        {
            switch(unit.gameObject.tag)
            {
                case "PlayerUnit":
                    List_PlayerUnits.Add(unit);
                    break;
                case "AIUnit":
                    List_AIUnits.Add(unit);
                    break;
                default:
                    continue;
            }
        }

        // Fill the cover Array
        Array_Covers = FindObjectsOfType<CCover>();

        m_TextRound = m_roundCountGO.GetComponent<Text>();
        m_TextOnTurn = m_onTurnUIGO.GetComponent<Text>();
    }

    private void Update()
    {
        // On game start
        if (ActivateTurn)
        {
            Turn();
            ActivateTurn = false;
        }

        //Check for end
        if (List_AIUnits.Count == 0)
            GameOver(EWinner.PLAYER);
        else if (List_PlayerUnits.Count == 0)
            GameOver(EWinner.ENEMY);
    }

    #endregion

    #endregion
}
