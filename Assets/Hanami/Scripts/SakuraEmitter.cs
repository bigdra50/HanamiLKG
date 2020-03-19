using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using UniRx.Triggers;

public class SakuraEmitter : MonoBehaviour
{

    [SerializeField] private GameObject m_sakura;
    [SerializeField] private GameObject m_rightPalm, m_leftPalm;
    [SerializeField] private float m_lifeTime;
    
    void Start()
    {
        LeapRx.rightOpenStream
            .Subscribe(_ =>
            {
                var sakura = Instantiate(m_sakura, m_rightPalm.transform);
                sakura.transform.parent = null;
                Destroy(sakura, m_lifeTime);
            });
        
        LeapRx.leftOpenStream
            .Subscribe(_ =>
            {
                var sakura = Instantiate(m_sakura, m_leftPalm.transform);
                sakura.transform.parent = null;
                Destroy(sakura, m_lifeTime);
            });
    }
}
