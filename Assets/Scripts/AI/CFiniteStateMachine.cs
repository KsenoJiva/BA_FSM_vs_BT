using System.Collections.Generic;
using System.Linq;
using UnityEngine;

/// <summary>
/// FSM for controlling AI Units
/// </summary>
public class CFiniteStateMachine : CGeneralAI
{
    #region Private Classes

    #region Abtract Classes

    /// <summary>
    /// Base class for states
    /// </summary>
    private abstract class CState
    {
        #region Protected Variables

        protected CFiniteStateMachine m_fsm;

        #endregion

        #region Methods

        #region Constructor

        public CState(CFiniteStateMachine _fsm)
        {
            m_fsm = _fsm;
        }

        #endregion

        #region Public Methods

        #region Abstract Methods

        /// <summary>
        /// Stateinit
        /// </summary>
        public abstract void Start();

        /// <summary>
        /// Actions for active unit
        /// </summary>
        public abstract void Run();

        /// <summary>
        /// Check transistions to other states
        /// </summary>
        public abstract void Transitions();

        #endregion

        #endregion

        #endregion
    }

    #endregion

    /// <summary>
    /// State to stay in position and attack
    /// </summary>
    private class CStateStay : CState
    {
        #region Methods

        #region Constructor

        public CStateStay(CFiniteStateMachine _fsm) : base(_fsm) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Start() { }

        public override void Run()
        {
            //Stay in position
            if (m_fsm.m_activeUnit == null
                || !m_fsm.m_activeUnit.ActionAllowed)
                return;

            //if(m_fsm.m_currentUnitToRun.CheckCoverOfTarget(m_fsm.m_currentUnitToRun, m_fsm.CummulativeTarget) == CCover.ECoverType.NONE)
            //{
            //    Vector3 dir = (m_fsm.CummulativeTarget - m_fsm.CummulativePos).normalized;
            //    Vector3 move = m_fsm.m_currentUnitToRun.transform.position;
            //    CCover.ECoverType activeCover = CCover.ECoverType.NONE;
            //
            //    //Look for simple cover
            //    foreach (CCover cover in m_fsm.m_aiController.Array_Covers)
            //    {
            //        if (cover.CoverType > activeCover
            //            && Vector3.Distance(cover.transform.position, m_fsm.m_currentUnitToRun.transform.position) <= m_fsm.m_currentUnitToRun.MoveRadius)
            //            move = cover.transform.position + dir * 0.8f;
            //    }
            //
            //    if (move != m_fsm.m_currentUnitToRun.transform.position)
            //    {
            //        // Apply
            //        m_fsm.m_currentUnitToRun.Move(move);
            //
            //        return;
            //    }
            //}

            //Shoot best target

            //Copy to work with
            List<CUnitController> list_tmpTargets = new List<CUnitController>(m_fsm.m_list_enemyUnits);

            //find lowest cover
            CCover.ECoverType lowestCoverOfTargets = m_fsm.m_activeUnit.CheckCoverOfTarget(list_tmpTargets[0], m_fsm.m_activeUnit.transform.position);

            foreach(CUnitController enemy in list_tmpTargets)
            {
                CCover.ECoverType tmpCover = m_fsm.m_activeUnit.CheckCoverOfTarget(enemy, m_fsm.m_activeUnit.transform.position);

                if ((int)tmpCover < (int)lowestCoverOfTargets)
                    lowestCoverOfTargets = tmpCover;
            }

            // just target lowest cover
            list_tmpTargets.RemoveAll(o => m_fsm.m_activeUnit.CheckCoverOfTarget(o, m_fsm.m_activeUnit.transform.position) != lowestCoverOfTargets);

            //Target closest
            CUnitController target = list_tmpTargets.Aggregate((o1, o2)
                                        => Vector3.Distance(m_fsm.m_activeUnit.transform.position, o1.transform.position) 
                                        < Vector3.Distance(m_fsm.m_activeUnit.transform.position, o2.transform.position) 
                                        ? o1 : o2);

            //Shoot
            m_fsm.m_activeUnit.Shoot(target);

            m_fsm.m_shotTimer = m_fsm.m_timeAfterShot;
        }
        
        public override void Transitions()
        {
            //Retreat when low health
            if (m_fsm.m_healthPoolOwn < m_fsm.m_healthPoolEnemy * m_fsm.m_ToRetreat_HealthPerc)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.RETREAT];
            //Push
            else if (Vector3.Distance(m_fsm.CummulativePos, m_fsm.CummulativeTarget) >= m_fsm.m_ToStay_Distance)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.PUSH];
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// State to push as a group towards enemy
    /// </summary>
    private class CStatePush : CState
    {
        #region Methods

        #region Constructor

        public CStatePush(CFiniteStateMachine _fsm) : base(_fsm) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Start() { }

        public override void Run()
        {
            //Find targetpositions
            //Move up in group

            Vector3 dir = (m_fsm.CummulativePos - m_fsm.CummulativeTarget).normalized;

            //Limit to move radius

            Vector3 targetPos = m_fsm.m_currentUnitToRun.transform.position
                + (m_fsm.CummulativeTarget - m_fsm.m_currentUnitToRun.transform.position).normalized
                * Mathf.Min(Vector3.Distance(m_fsm.m_currentUnitToRun.transform.position, m_fsm.CummulativeTarget), m_fsm.m_currentUnitToRun.MoveRadius);

            Vector3 move = targetPos;
            CCover.ECoverType activeCover = CCover.ECoverType.NONE;

            //Look for simple cover
            foreach (CCover cover in m_fsm.m_aiController.Array_Covers)
            {
                if (cover.CoverType > activeCover
                    && Vector3.Distance(cover.transform.position, targetPos) <= 2.5f)
                    move = cover.transform.position + dir * 0.8f;
            }

            // Apply
            m_fsm.m_currentUnitToRun.Move(move);
        }

        public override void Transitions()
        {
            //Retreat
            if (m_fsm.m_healthPoolOwn < m_fsm.m_healthPoolEnemy * m_fsm.m_ToRetreat_HealthPerc)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.RETREAT];
            //Stay when somewhat hurt
            else if (m_fsm.m_healthPoolOwn < m_fsm.m_healthPoolEnemy * m_fsm.m_ToStay_HealthPerc)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.STAY];
            //Shoot
            else if (Vector3.Distance(m_fsm.CummulativePos, m_fsm.CummulativeTarget) <= m_fsm.m_ToStay_Distance)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.STAY];
            //Shoot close proximity
            else if (m_fsm.ClosestEnemy != null && m_fsm.DistanceToClosestEnemyOverall() < m_fsm.m_ToStay_CloseProximity_Distance)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.STAY];
        }

        #endregion

        #endregion

        #endregion
    }

    /* Cancelled
    /// <summary>
    /// State to push through the flanks by splitting up.
    /// </summary>
    private class CStateFlank : CState
    {
        #region Methods

        #region Constructor

        public CStateFlank(CFiniteStateMachine _fsm) : base(_fsm) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Start() { }

        public override void Run()
        {
            //Split and move far
        }

        public override void Transitions()
        {
            //Check transitions
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// State to seek best possible cover
    /// </summary>
    private class CStateCover : CState
    {
        #region Methods

        #region Constructor

        public CStateCover(CFiniteStateMachine _fsm) : base(_fsm) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Start() { }

        public override void Run()
        {
            //best cover
            //Optional: Move back
        }

        public override void Transitions()
        {
            //Check transitions
        }

        #endregion

        #endregion

        #endregion
    }
    */

    /// <summary>
    /// State to retreat back and group up
    /// </summary>
    private class CStateRetreat : CState
    {
        #region Methods

        #region Constructor

        public CStateRetreat(CFiniteStateMachine _fsm) : base(_fsm) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Start() { }

        public override void Run()
        {
            //move back far
            //group up

            Vector3 dir = (m_fsm.CummulativePos - m_fsm.CummulativeTarget).normalized;

            //Limit to move radius

            Vector3 targetPos = m_fsm.m_currentUnitToRun.transform.position
                + (m_fsm.m_currentUnitToRun.transform.position - m_fsm.CummulativeTarget).normalized * m_fsm.m_currentUnitToRun.MoveRadius
                + (m_fsm.CummulativePos - m_fsm.m_currentUnitToRun.transform.position).normalized * m_fsm.m_currentUnitToRun.MoveRadius;

            Vector3 move = targetPos;
            CCover.ECoverType activeCover = CCover.ECoverType.NONE;

            //Look for simple cover
            foreach (CCover cover in m_fsm.m_aiController.Array_Covers)
            {
                if (cover.CoverType > activeCover
                    && Vector3.Distance(cover.transform.position, targetPos) <= 3.5f)
                    move = cover.transform.position + dir * 1.0f;
            }

            // Apply
            m_fsm.m_currentUnitToRun.Move(move);
        }

        public override void Transitions()
        {
            //Push
            if (m_fsm.m_healthPoolOwn > m_fsm.m_healthPoolEnemy)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.PUSH];
            //Shoot when more HP
            else if (m_fsm.m_healthPoolOwn * m_fsm.m_ToStay_HealthPerc > m_fsm.m_healthPoolEnemy)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.STAY];
            //Shoot when close
            else if (Vector3.Distance(m_fsm.CummulativePos, m_fsm.CummulativeTarget) <= m_fsm.m_ToStay_Distance)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.STAY];
            //Shoot close proximity
            else if (m_fsm.ClosestEnemy != null && m_fsm.DistanceToClosestEnemyOverall() < m_fsm.m_ToStay_CloseProximity_Distance)
                m_fsm.m_currentState = m_fsm.m_list_states[(int)EStateIdx.STAY];
        }

        #endregion

        #endregion

        #endregion
    }

    #endregion

    #region Private Enums

    /// <summary>
    /// Access States in statelist with those indices
    /// </summary>
    private enum EStateIdx
    {
        STAY,
        PUSH,
        /* Cancelled
        FLANK,
        COVER,
        */
        RETREAT,
        COUNT
    }

    #endregion
    
    #region Private Variables

    private CState m_currentState;
    private CUnitController m_currentUnitToRun;

    /// <summary>
    /// The different States for the Finite State Machine
    /// </summary>
    private List<CState> m_list_states;

    #endregion
        
    #region Methods

    #region Public Methods

    #region Override Methods

    /// <summary>
    /// Initial Turn start
    /// </summary>
    public override void Turn()
    {
        // execute base actions
        base.Turn();
        m_currentUnitToRun = m_activeUnit;

        // setup units according to selected mode
        if (m_aiController.GameManager.CurrentAI == GameMode.FSM)
        {
            foreach (CUnitController unit in m_aiController.List_AIUnits)
            {
                unit.FinishedMove = false;
                unit.ActionAllowed = true;
            }
        }
        else if(m_aiController.GameManager.CurrentAI == GameMode.TEST)
        {
            foreach (CUnitController unit in m_aiController.List_PlayerUnits)
            {
                unit.FinishedMove = false;
                unit.ActionAllowed = true;
            }
        }

        m_currentState.Start();

        // check transitions
        m_currentState.Transitions();
    }

    #endregion

    /// <summary>
    /// Returns the overall smallest distance between an enemy and own unit
    /// </summary>
    /// <returns> Smallest Distance </returns>
    public float DistanceToClosestEnemyOverall()
    {
        float toReturn = float.MaxValue;
        float tmpDist;

        foreach(CUnitController unit in m_list_myUnits)
        {
            foreach (CUnitController enemy in m_list_enemyUnits)
            {
                tmpDist = Vector3.Distance(enemy.transform.position, unit.transform.position);
                toReturn = tmpDist < toReturn ? tmpDist : toReturn;
            }
        }

        return toReturn;
    }

    #endregion

    #region Unity Methods

    private void Start ()
    {
        m_aiController = GetComponent<CAIController>();

        // Activate according to selected mode
        switch (m_aiController.GameManager.CurrentAI)   
        {
            case GameMode.FSM:
                enabled = true;
                m_list_myUnits = m_aiController.List_AIUnits;
                m_list_enemyUnits = m_aiController.List_PlayerUnits;
                break;
            case GameMode.BT:
                enabled = false;
                break;
            case GameMode.TEST:
                enabled = true;
                m_list_myUnits = m_aiController.List_PlayerUnits;
                m_list_enemyUnits = m_aiController.List_AIUnits;
                break;
        }

        // setup states
        m_list_states = new List<CState>
        {
            new CStateStay(this),       // Do not move
            new CStatePush(this),       // Move towards enemy
            /* Cancelled
            new CStateFlank(this),      // Try to flank the enemy
            new CStateCover(this),      // Seek best cover  
             */
            new CStateRetreat(this),    // Retreat to base
        };

        // set init state
        m_currentState = m_list_states[(int)EStateIdx.PUSH];
	}

    private void Update()
    {
        m_shotTimer -= Time.deltaTime;

        // Choose active unit
        if (m_activeUnit != null && m_activeUnit.FinishedMove && m_shotTimer <= 0.0f)
            m_activeUnit = CUnitController.NextAvailableUnit(m_list_myUnits);

        // Highlight active Unit
        foreach (CUnitController unit in m_list_myUnits)
            unit.Highlighted = (unit == m_activeUnit);

        if(!IsOnTurn)
            return;

        #region On Turn Behavior
                
        // execute state
        m_currentState.Run();

        if (m_currentUnitToRun.FinishedMove)
            m_currentUnitToRun = m_activeUnit;

        #endregion

        //End turn after all actions performed
        if (m_activeUnit == null)
            m_aiController.EndTurn();
    }

    #endregion

    #endregion
}
