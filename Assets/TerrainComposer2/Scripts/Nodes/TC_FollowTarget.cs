using UnityEngine;
using System.Collections;

namespace TerrainComposer2
{
    [ExecuteInEditMode]
    public class TC_FollowTarget : MonoBehaviour
    {
        public Transform target;
        public bool refresh = false;
        
        #if UNITY_EDITOR
        void OnEnable()
        {
            UnityEditor.EditorApplication.update += MyUpdate;
        }

        void OnDisable()
        {
            UnityEditor.EditorApplication.update -= MyUpdate;
        }
        #endif


        void MyUpdate()
        {
            if (target == null) return;

            transform.position = target.position;

            if (refresh)
            {
                TC.repaintNodeWindow = true;
                TC.AutoGenerate();
            }
        }
    }
}
