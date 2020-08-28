using System.Collections.Generic;
using UnityEngine;
using System.Linq;

/// <summary>
/// Behaviour Tree for controling AI Units
/// </summary>
public class CBehaviourTree : CGeneralAI
{
    #region Private Classes

    #region Abstract Classes

    /// <summary>
    /// Base class for Behaviour Tree Nodes
    /// </summary>
    private abstract class CBTNode
    {
        #region Variables

        #region Protected Variables

        protected CBehaviourTree m_bt;

        #endregion

        #region Public Variables

        /// <summary>
        /// List of all Child Nodes
        /// </summary>
        public List<CBTNode> m_List_ChildNodes = null;

        /// <summary>
        /// This nodes parent Node
        /// </summary>
        public CBTNode m_Parent = null;

        #endregion

        #endregion

        #region Properties

        /// <summary>
        /// Current state of this Node
        /// </summary>
        public EReturn State { get; protected set; }

        #endregion

        #region Methods

        #region Constructor

        public CBTNode(CBehaviourTree _bt, CBTNode _parent)
        {
            m_bt = _bt;
            m_Parent = _parent;
        }

        #endregion

        #region Public Methods

        #region Virtual Methods

        public virtual void Run()
        {
            State = EReturn.RUNNING;
        }

        /// <summary>
        /// Check the States of the Child Nodes and change this Nodes current State accordingly
        /// </summary>
        //public virtual void CheckChilds() { }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Selects Child for execution
    /// </summary>
    private abstract class CBTNodeSelector : CBTNode
    {
        #region Methods

        #region Constructor

        public CBTNodeSelector(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            base.Run();

            SelectChild();
        }

        #endregion

        #endregion

        #region Protected Methods

        #region Abstract Methods

        /// <summary>
        /// Select which child should be executed
        /// </summary>
        protected abstract void SelectChild();

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Base class for leaf nodes
    /// </summary>
    private abstract class CBTNodeBehavior : CBTNode
    {
        #region Methods

        #region Constructor

        public CBTNodeBehavior(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            m_bt.m_activeBehavior = this;
            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    #endregion

    #region Node Classes

    /// <summary>
    /// Executes Childs after another. Cancels on first fail
    /// </summary>
    private class CBTNodeSequencer : CBTNode
    {
        #region Methods

        #region Constructor

        public CBTNodeSequencer(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { m_List_ChildNodes = new List<CBTNode>(); }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            // execute base
            base.Run();

            // run each child
            for(int i = 0; i < m_List_ChildNodes.Count; i++)
            {
                m_List_ChildNodes[i].Run();
                
                // cancel on first fail
                if(m_List_ChildNodes[i].State == EReturn.FAILURE)
                {
                    State = EReturn.FAILURE;
                    return;
                }
            }

            State = EReturn.SUCCESS;
        }

        //public override void CheckChilds()
        //{
        //    foreach(CBTNode child in m_List_ChildNodes)
        //    {
        //        if(child.State == EReturn.FAILURE)
        //        {
        //            State = EReturn.FAILURE;
        //            return;
        //        }
        //
        //        if (child.State == EReturn.RUNNING)
        //            State = EReturn.RUNNING;
        //    }
        //
        //
        //}

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Repeats Child regardless of success or failure
    /// </summary>
    private class CBTNodeRepeater : CBTNode
    {
        #region Private Variables

        private uint m_iterations;

        #endregion

        #region Methods

        #region Constructor

        public CBTNodeRepeater(CBehaviourTree _bt, CBTNode _parent, uint _iterations) : base(_bt, _parent)
        {
            // only one child allowed
            m_List_ChildNodes = new List<CBTNode>(1);

            // set iterations
            m_iterations = _iterations;
        }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            // execute base
            base.Run();
            
            // repeat child
            for(int i = 0; i < m_iterations; i++)
                m_List_ChildNodes[0].Run();

            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Inverts the ReturnType of the Child
    /// </summary>
    private class CBTNodeInverter : CBTNode
    {
        #region Methods

        #region Constructor

        public CBTNodeInverter(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent)
        {
            // allow just one child
            m_List_ChildNodes = new List<CBTNode>(1);
        }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            // execute base
            base.Run();

            // run child
            m_List_ChildNodes[0].Run();

            // invert returntype
            switch (m_List_ChildNodes[0].State)
            {
                case EReturn.SUCCESS:
                    State = EReturn.FAILURE;
                    break;
                case EReturn.FAILURE:
                    State = EReturn.SUCCESS;
                    break;
                default:
                    State = EReturn.RUNNING;
                    break;
            }
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Returns true regardless of Childs returntype
    /// </summary>
    private class CBTNodeTRUE : CBTNode
    {
        #region Methods

        #region Constructor

        public CBTNodeTRUE(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent)
        {
            // allow just one child
            m_List_ChildNodes = new List<CBTNode>(1);
        }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            // execute base
            base.Run();

            // run Child
            m_List_ChildNodes[0].Run();

            // always success
            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Find and set the closest enemy
    /// </summary>
    private class CBTFindCLosestEnemy : CBTNode
    {
        #region Methods

        #region Constructor

        public CBTFindCLosestEnemy(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            base.Run();

            m_bt.m_closestEnemy = m_bt.m_list_enemyUnits[0] != null ? m_bt.m_list_enemyUnits[0] : null;

            //Check distance for closest
            foreach (CUnitController enemy in m_bt.m_list_enemyUnits)
            {
                m_bt.m_closestEnemy = Vector3.Distance(m_bt.m_closestEnemy.transform.position, m_bt.m_activeUnit.transform.position)
                                        < Vector3.Distance(enemy.transform.position, m_bt.m_activeUnit.transform.position)
                                        ? m_bt.m_closestEnemy : enemy;
            }

            if (m_bt.m_closestEnemy != null)
                State = EReturn.SUCCESS;
            else
                State = EReturn.FAILURE;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Find best cover at target position and adjust target position
    /// </summary>
    private class CBTFindCoveredPosition : CBTNode
    {
        #region Methods

        #region Constructor

        public CBTFindCoveredPosition(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            base.Run();

            CCover tmpCover = null;

            try
            {
                foreach (CCover cover in m_bt.m_aiController.Array_Covers)
                {
                    //search only trough cover at targeted location within movement range
                    if (Vector3.Distance(cover.transform.position, m_bt.m_targetPos) > 5.0f
                        || Vector3.Distance(cover.transform.position, m_bt.transform.position) > m_bt.m_activeUnit.MoveRadius)
                        continue;

                    if (tmpCover == null)
                        tmpCover = cover;

                    //prioritize high value cover
                    tmpCover = (int)cover.CoverType > (int)tmpCover.CoverType ? cover : tmpCover;

                    //break on highest covertype
                    if (tmpCover.CoverType >= CCover.ECoverType.COUNT - 1)
                        break;
                }

                //set target at cover opposite to enemy
                if(tmpCover != null)
                    m_bt.m_targetPos = tmpCover.transform.position + (m_bt.CummulativePos - m_bt.CummulativeTarget).normalized * 0.8f;
            }
            catch { State = EReturn.FAILURE; return; }

            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Set target position by cummulative positions and modifier direction
    /// </summary>
    private class CBTTargetPos : CBTNode
    {
        #region Private Variables

        private EMoveMod m_dir;

        #endregion

        #region Methods

        #region Constructor

        public CBTTargetPos(CBehaviourTree _bt, CBTNode _parent, EMoveMod _mod) : base(_bt, _parent) { m_dir = _mod; }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            base.Run();

            // Set Target Pos for each case
            switch(m_dir)
            {
                case EMoveMod.TOWARD:
                    m_bt.m_targetPos = m_bt.m_activeUnit.transform.position + (m_bt.CummulativeTarget - m_bt.CummulativePos).normalized
                        * Mathf.Min(Vector3.Distance(m_bt.m_activeUnit.transform.position, m_bt.CummulativeTarget), m_bt.m_activeUnit.MoveRadius);
                    break;

                case EMoveMod.AWAY:
                    m_bt.m_targetPos = m_bt.m_activeUnit.transform.position + (m_bt.CummulativePos - m_bt.CummulativeTarget).normalized
                        * Mathf.Min(Vector3.Distance(m_bt.m_activeUnit.transform.position, m_bt.CummulativeTarget), m_bt.m_activeUnit.MoveRadius);
                    break;

                case EMoveMod.STAY:
                    m_bt.m_targetPos = m_bt.m_activeUnit.transform.position;
                    break;

                default:
                    State = EReturn.FAILURE;
                    return;
            }

            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Check if covered when staying
    /// Chooses first child when not covered
    /// </summary>
    private class CBTStayCoverSelector : CBTNodeSelector
    {
        #region Methods

        #region Constructor

        public CBTStayCoverSelector(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { m_List_ChildNodes = new List<CBTNode>(2); }

        #endregion

        #region Public Methods

        #region Override Methods

        protected override void SelectChild()
        {
            try
            {
                if (m_bt.m_activeUnit.CheckCoverOfTarget(m_bt.m_activeUnit, m_bt.CummulativeTarget) == CCover.ECoverType.NONE
                    && m_bt.m_aiController.Array_Covers.FirstOrDefault(o => Vector3.Distance(o.transform.position, m_bt.m_activeUnit.transform.position) < m_bt.m_activeUnit.MoveRadius) != null)
                    m_List_ChildNodes.FirstOrDefault()?.Run();
                else
                    m_List_ChildNodes.LastOrDefault()?.Run();
            }
            catch
            {
                State = EReturn.FAILURE;
                return;
            }

            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Chooses 1 of 2 childs based on the HP differences of the two teams.
    /// First child executed when less
    /// </summary>
    private class CBTHealthpoolSelector : CBTNodeSelector
    {
        #region Methods

        #region Constructor

        public CBTHealthpoolSelector(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { m_List_ChildNodes = new List<CBTNode>(2); }

        #endregion

        #region Public Methods

        #region Override Methods

        protected override void SelectChild()
        {
            try
            {
                if (m_bt.m_healthPoolOwn < m_bt.m_healthPoolEnemy * m_bt.m_ToRetreat_HealthPerc)
                    m_List_ChildNodes.FirstOrDefault()?.Run();
                else
                    m_List_ChildNodes.LastOrDefault()?.Run();
            }
            catch
            {
                State = EReturn.FAILURE;
                return;
            }

            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Chooses 1 of 2 childs based on cummulative distance of the two teams.
    /// First child executed when less
    /// </summary>
    private class CBTDistanceSelector : CBTNodeSelector
    {
        #region Methods

        #region Constructor

        public CBTDistanceSelector(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { m_List_ChildNodes = new List<CBTNode>(2); }

        #endregion

        #region Public Methods

        #region Override Methods

        protected override void SelectChild()
        {
            try
            {
                if (Vector3.Distance(m_bt.CummulativePos, m_bt.CummulativeTarget) < m_bt.m_ToStay_Distance)
                    m_List_ChildNodes[0]?.Run();
                else if (Vector3.Distance(m_bt.ClosestEnemy.transform.position, m_bt.m_activeUnit.transform.position) < m_bt.m_ToStay_CloseProximity_Distance)
                    m_List_ChildNodes[1]?.Run();
                else
                    m_List_ChildNodes[2]?.Run();
            }
            catch
            {
                State = EReturn.FAILURE;
                return;
            }

            State = EReturn.SUCCESS;
        }

        #endregion

        #endregion

        #endregion
    }

    #endregion

    #region Acting Classes

    /// <summary>
    /// Stay in position and shoot
    /// </summary>
    private class CBTShoot : CBTNodeBehavior
    {
        #region Methods

        #region Constructor

        public CBTShoot(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            base.Run();

            //Stay and shoot
            if(m_bt.m_activeUnit.Shoot(m_bt.m_closestEnemy))
                m_bt.m_shotTimer = m_bt.m_timeAfterShot;
        }

        #endregion

        #endregion

        #endregion
    }
    
    /* Cancelled
    /// <summary>
    /// Split and push up flanks
    /// </summary>
    private class CFlank : CBTNodeBehavior
    {
        #region Methods

        #region Constructor

        public CFlank(CBehaviourTree _bt) : base(_bt) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Act()
        {
            //Split and move far
        }

        #endregion

        #endregion

        #endregion
    }

    /// <summary>
    /// Seek best possible cover
    /// </summary>
    private class CCover : CBTNodeBehavior
    {
        #region Methods

        #region Constructor

        public CCover(CBehaviourTree _bt) : base(_bt) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Act()
        {
            //best cover
            //optional: backwards
        }

        #endregion

        #endregion

        #endregion
    }
    */

    /// <summary>
    /// Move to targeted Position
    /// </summary>
    private class CBTMove : CBTNodeBehavior
    {
        #region Methods

        #region Constructor

        public CBTMove(CBehaviourTree _bt, CBTNode _parent) : base(_bt, _parent) { }

        #endregion

        #region Public Methods

        #region Override Methods

        public override void Run()
        {
            base.Run();

            m_bt.m_activeUnit.Move(m_bt.m_targetPos);
        }

        #endregion

        #endregion

        #endregion
    }
        
    #endregion

    #endregion

    #region Private Enums

    /// <summary>
    /// Return Types of the Behaviour Tree Nodes
    /// </summary>
    private enum EReturn
    {
        FAILURE,
        RUNNING,
        SUCCESS
    }

    /// <summary>
    /// Modifier selection for target position
    /// </summary>
    private enum EMoveMod
    {
        TOWARD,
        AWAY,
        STAY
    }

    #endregion

    #region Private Variables

    private CBTNodeBehavior m_activeBehavior;
    private CUnitController m_closestEnemy;
    private CBTNode m_rootNode;
    private CCover m_bestCoveredPos;
    private Vector3 m_targetPos;

    #endregion

    #region Methods

    #region Private Methods

    /// <summary>
    /// Builds the tree structure
    /// </summary>
    private void SetupTree()
    {
        //Root
        m_rootNode = new CBTDistanceSelector(this, null);

        //Root Childs
        //CBTNode staySelect = new CBTStayCoverSelector(this, m_rootNode);
        CBTNode shootSequence = new CBTNodeSequencer(this, m_rootNode);
        CBTNode moveSelect = new CBTHealthpoolSelector(this, m_rootNode);
        m_rootNode.m_List_ChildNodes.Add(shootSequence);
        m_rootNode.m_List_ChildNodes.Add(shootSequence);
        m_rootNode.m_List_ChildNodes.Add(moveSelect);

        //Path 0 - Stay Childs
        //CBTNode coverSequence = new CBTNodeSequencer(this, staySelect);
        //staySelect.m_List_ChildNodes.Add(coverSequence);
        //staySelect.m_List_ChildNodes.Add(shootSequence);

        //Path 0.1 - Find Cover Childs
        //coverSequence.m_List_ChildNodes.Add(new CBTTargetPos(this, coverSequence, EMoveMod.STAY));
        //coverSequence.m_List_ChildNodes.Add(new CBTFindCoveredPosition(this, coverSequence));
        //coverSequence.m_List_ChildNodes.Add(new CBTMove(this, coverSequence));

        //Path 1 - Shoot Childs
        shootSequence.m_List_ChildNodes.Add(new CBTFindCLosestEnemy(this, shootSequence));
        shootSequence.m_List_ChildNodes.Add(new CBTShoot(this, shootSequence));

        //Path 2 - Move Selection Childs
        CBTNode retreatSequence = new CBTNodeSequencer(this, moveSelect);
        CBTNode pushSequence = new CBTNodeSequencer(this, moveSelect);
        moveSelect.m_List_ChildNodes.Add(retreatSequence);
        moveSelect.m_List_ChildNodes.Add(pushSequence);

        //Path 2.0 - Retreat Sequence Childs
        retreatSequence.m_List_ChildNodes.Add(new CBTTargetPos(this, retreatSequence, EMoveMod.AWAY));
        retreatSequence.m_List_ChildNodes.Add(new CBTFindCoveredPosition(this, retreatSequence));
        retreatSequence.m_List_ChildNodes.Add(new CBTMove(this, retreatSequence));

        //Path 2.1 - Push Sequence Childs
        pushSequence.m_List_ChildNodes.Add(new CBTTargetPos(this, pushSequence,EMoveMod.TOWARD));
        pushSequence.m_List_ChildNodes.Add(new CBTFindCoveredPosition(this, pushSequence));
        pushSequence.m_List_ChildNodes.Add(new CBTMove(this, pushSequence));
    }

    #endregion

    #region Public Methods

    #region Override Methods

    /// <summary>
    /// Initial turn behavior
    /// </summary>
    public override void Turn()
    {
        // execute base
        base.Turn();

        // setup units
        foreach (CUnitController unit in m_aiController.List_AIUnits)
        {
            unit.FinishedMove = false;
            unit.ActionAllowed = true;
        }
    }

    #endregion

    #endregion

    #region Unity Methods

    private void Start ()
    {
        m_aiController = GetComponent<CAIController>();

        // Activate according to selected mode
        switch (m_aiController.GameManager.CurrentAI)   
        {
            case GameMode.FSM:
                enabled = false;
                break;
            case GameMode.BT:
                enabled = true;
                m_list_myUnits = m_aiController.List_AIUnits;
                m_list_enemyUnits = m_aiController.List_PlayerUnits;
                break;
            case GameMode.TEST:
                enabled = true;
                m_list_myUnits = m_aiController.List_AIUnits;
                m_list_enemyUnits = m_aiController.List_PlayerUnits;
                break;
        }

        // setup behavior trees
        if(m_aiController.GameManager.CurrentAI == GameMode.BT
            || m_aiController.GameManager.CurrentAI == GameMode.TEST)
            SetupTree();
    }

    private void Update()
    {
        m_shotTimer -= Time.deltaTime;

        // Choose active unit
        if (m_activeUnit != null && m_activeUnit.FinishedMove && m_shotTimer <= 0.0f)
            m_activeUnit = CUnitController.NextAvailableUnit(m_list_myUnits);

        foreach (CUnitController unit in m_list_myUnits)
            unit.Highlighted = (unit == m_activeUnit);

        if(!IsOnTurn)
            return;

        #region On Turn Behavior

        //Run through tree
        m_rootNode.Run();

        // execute acting Leaf
        //m_activeBehavior.Act();

        #endregion

        // end turn when all units acted
        if (m_activeUnit == null)
            m_aiController.EndTurn();
    }

    #endregion

    #endregion
}
