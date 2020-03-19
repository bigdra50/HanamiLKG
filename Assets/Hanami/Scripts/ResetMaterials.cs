using System.Collections;
using System.Collections.Generic;
using UniRx;
using UniRx.Triggers;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ResetMaterials : MonoBehaviour
{
    [SerializeField] private List<GameObject> m_sakuras;
    [SerializeField] private Material m_mat;
    void Start()
    {

        this.UpdateAsObservable()
            .Select(_ => Input.inputString)
            .Do(_=>print("do1"))
            .Where(key => Input.GetKeyDown(KeyCode.R))
            .Do(_=>print("do2"))
            .Subscribe(_ => ResetScene());

        LeapRx.peaceSignStream
            .Subscribe(_ => ResetScene());
    }

    void ResetBloom()
    {
        print("Reset");
        foreach (var sakura in m_sakuras)
        {
            var rend = sakura.GetComponent<Renderer>();
            var mats = rend.materials;
            mats[1] = m_mat;
            rend.materials = mats;
        }
    }

    void ResetScene()
    {
        var thisScene = SceneManager.GetActiveScene();
        SceneManager.LoadScene(thisScene.name);
    }

}
