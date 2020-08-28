using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Controls for Player interaction and Units
/// </summary>
public class CPlayerControls : MonoBehaviour
{
    #region Variables

    #region Private Variables

    private Camera m_cam;
    private CUnitController m_activeUnit;
    private CAIController m_aiCtrl;

    /// <summary>
    /// List of all Units controlled by the AI
    /// </summary>
    private List<CUnitController> m_list_enemyUnits;

    /// <summary>
    /// List of all Units controlled by the Player
    /// </summary>
    private List<CUnitController> m_list_myUnits;

    #endregion

    #region Exposed Variables

    [SerializeField, Range(0, 30), Tooltip("Speed of the Camera Movement")]
    private float m_camSpeed = 10;

    [SerializeField, Range(0, 180), Tooltip("Speed of the Camera Rotation")]
    private float m_camRotSpeed = 60;

    [SerializeField]
    private GameObject m_pauseMenu;

    #endregion

    #endregion

    #region Properties

    public bool IsOnTurn { get; private set; }

    public bool ActivateTurn { get; set; }

    #endregion

    #region Methods

    #region Public Methods

    /// <summary>
    /// Actions for this turn beginning
    /// </summary>
    public void Turn()
    {
        IsOnTurn = true;

        //reset Units
        foreach(CUnitController unit in m_list_myUnits)
        {
            unit.FinishedMove = false;
            unit.ActionAllowed = true;
        }
        m_activeUnit = m_list_myUnits[0];
    }

    #endregion

    #region Private Methods

    /// <summary>
    /// Moves the Camera horizontally by key
    /// </summary>
    private void MoveCamera()
    {
        Vector3 movement = Vector3.zero;

        //forward
        if(Input.GetKey(KeyCode.W))
        {
            movement += new Vector3(m_cam.transform.forward.x, 0, m_cam.transform.forward.z) * m_camSpeed * Time.deltaTime;
        }

        //backward
        if (Input.GetKey(KeyCode.S))
        {
            movement += new Vector3(-m_cam.transform.forward.x, 0, -m_cam.transform.forward.z) * m_camSpeed * Time.deltaTime;
        }

        //left
        if (Input.GetKey(KeyCode.A))
        {
            movement += new Vector3(-m_cam.transform.right.x, 0, -m_cam.transform.right.z) * m_camSpeed * Time.deltaTime;
        }

        //right
        if (Input.GetKey(KeyCode.D))
        {
            movement += new Vector3(m_cam.transform.right.x, 0, m_cam.transform.right.z) * m_camSpeed * Time.deltaTime;
        }

        //rotate left
        if (Input.GetKey(KeyCode.Q))
        {
            m_cam.transform.RotateAround(gameObject.transform.position, Vector3.up, -m_camRotSpeed * Time.deltaTime);
        }

        //rotate right
        if (Input.GetKey(KeyCode.E))
        {
            m_cam.transform.RotateAround(gameObject.transform.position, Vector3.up, m_camRotSpeed * Time.deltaTime);
        }

        //apply
        m_cam.transform.position += movement;
    }

    /// <summary>
    /// Processes Input for movement
    /// </summary>
    private void InputMove()
    {
        Vector3 targetPos = m_activeUnit.transform.position;

        //raycast
        Ray r = m_cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(r, out hit);
        
        // Check for valid hit and boundaries
        if ((hit.collider.gameObject.tag == "Ground" || hit.collider.gameObject.tag == "Startzone")
            && Vector3.Distance(m_activeUnit.transform.position, hit.point) <= m_activeUnit.MoveRadius)
        {
            targetPos = hit.point;

            //snap to grid
            targetPos.x = Mathf.RoundToInt(targetPos.x);
            targetPos.z = Mathf.RoundToInt(targetPos.z);

            //apply movement
            m_activeUnit.Move(targetPos);
        }
    }

    /// <summary>
    /// Processes input for shooting
    /// </summary>
    private void InputShoot()
    {
        //raycast
        Ray r = m_cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(r, out hit);

        // Check for vaild hit
        if (hit.collider.gameObject.tag == "AIUnit")
        {
            //Shoot
            m_activeUnit.Shoot(hit.collider.gameObject.GetComponent<CUnitController>());
        }
    }

    /// <summary>
    /// Draws a line along possible paths
    /// </summary>
    private void DisplayPath()
    {
        //stop when no active unit
        if(m_activeUnit == null)
            return;

        UnityEngine.AI.NavMeshPath path = new UnityEngine.AI.NavMeshPath();

        //raycast to mouseposition
        Ray r = m_cam.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        Physics.Raycast(r, out hit);

        // Set Line positions
        m_activeUnit.m_NavAgent.CalculatePath(hit.point, path);
        m_activeUnit.m_MoveLine.positionCount = path.corners.Length;
        m_activeUnit.m_MoveLine.SetPositions(path.corners);

        // Switch color according to reachability
        if (Vector3.Distance(m_activeUnit.transform.position, hit.point) > m_activeUnit.MoveRadius)
        {
            m_activeUnit.m_MoveLine.startColor = Color.red;
            m_activeUnit.m_MoveLine.endColor = Color.red;
        }
        else
        {
            m_activeUnit.m_MoveLine.startColor = Color.green;
            m_activeUnit.m_MoveLine.endColor = Color.green;
        }
    }

    #endregion

    #region Unity Methods

    private void Awake()
    {
        m_cam = Camera.main;        
        m_aiCtrl = FindObjectOfType<CAIController>();
    }

    private void Start ()
    {
        //no player in testmode
        if (m_aiCtrl.GameManager.CurrentAI == GameMode.TEST)
            return;

        m_list_enemyUnits = m_aiCtrl.List_AIUnits;
        m_list_myUnits = m_aiCtrl.List_PlayerUnits;        
    }
	
	private void Update ()
    {
        //on game start
        if (ActivateTurn)
        {
            Turn();
            ActivateTurn = false;
        }

        //Camera
        MoveCamera();

        // Pause
        if(Input.GetKeyDown(KeyCode.Escape))
        {
            m_pauseMenu.SetActive(true);
            Time.timeScale = 0;
        }

        //no interaction in testmode
        if (m_aiCtrl.GameManager.CurrentAI == GameMode.TEST)
            return;
        
        // Choose active unit
        if (m_activeUnit != null && m_activeUnit.FinishedMove)
            m_activeUnit = CUnitController.NextAvailableUnit(m_list_myUnits);

        // Update highlight of units
        foreach (CUnitController unit in m_list_myUnits)
        {
            unit.Highlighted = (unit == m_activeUnit);
            unit.m_MoveLine.positionCount = 0;
        }

        if(!IsOnTurn)
            return;             

        // End turn after all units acted
        if(m_activeUnit == null)
        {
            IsOnTurn = false;
            m_aiCtrl.PlayerEndTurn = true;
            m_aiCtrl.Turn();
        }

        //path display
        DisplayPath();
		
        //interction
        //shoot
        if(Input.GetMouseButtonDown(0))
        {
            InputShoot();
        }
        //move
        if (Input.GetMouseButtonDown(1))
        {
            InputMove();
        }
    }

    #endregion

    #endregion
}
