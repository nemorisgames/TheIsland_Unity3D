using UnityEngine;
using System.Collections;

namespace TerrainComposer2
{
    [ExecuteInEditMode]
    public class TC_AutoGenerate : MonoBehaviour
    {

        [HideInInspector] public CachedTransform cT = new CachedTransform();
        public bool generateOnEnable = true;
        public bool generateOnDisable = true;
        Transform t;
        // public bool repeat;

        void Start()
        {
            t = transform;
            cT.Copy(t);
        }

        #if !UNITY_EDITOR
        void Update()
        {
            MyUpdate();
        }
        #endif

        void MyUpdate()
        {
            // if (repeat) TC.AutoGenerate();

            if (cT.hasChanged(t))
            {
                // Debug.Log("Auto generate");
                cT.Copy(t);
                TC.AutoGenerate();
                // TC_Generate.instance.Generate(true);
            }
        }

        void OnEnable()
        {
            if (generateOnEnable) TC.AutoGenerate();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update += MyUpdate;
            #endif
        }
         
        void OnDisable()
        {
            if (generateOnDisable) TC.AutoGenerate();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= MyUpdate;
            #endif
        }

        void OnDestroy()
        {
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.update -= MyUpdate;
            #endif
        }

    }
}