using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Leap.Unity.Animation.Internal;
using UnityEngine;

public class Bloom : MonoBehaviour
{

    [SerializeField] private Material m_sakuraFlower;
    private Renderer m_rend;
    
    void Start()
    {
        m_rend = GetComponent<Renderer>();
    }

    private void OnParticleCollision(GameObject obj)
    {
        ReplaceMat();
    }

    private void ReplaceMat()
    {
        var mats = m_rend.materials;
        mats[1] = m_sakuraFlower;
        m_rend.materials = mats;
    }
}
