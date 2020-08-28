using UnityEngine;

/// <summary>
/// Functionality of HP bar billboards
/// </summary>
public class CBehaviorHPBar : MonoBehaviour
{
    #region Private Variables

    private CUnitController m_parentUnit;
    private Transform m_childTransform;
    private Camera m_mainCam;

    #endregion

    #region Unity Methods

    void Start ()
    {
        m_parentUnit = gameObject.GetComponentInParent<CUnitController>();
        //get forwground bar
        m_childTransform = transform.GetChild(1);
        m_mainCam = Camera.main;
	}
	
	void Update ()
    {
        //scale foregrond bar with hp percentage of unit
        m_childTransform.localScale = new Vector3(m_parentUnit.Health / m_parentUnit.m_MaxHealth, m_childTransform.localScale.y, m_childTransform.localScale.z);

        //rotate towards camera; billboard
        transform.rotation = m_mainCam.transform.rotation;
	}

    #endregion
}
