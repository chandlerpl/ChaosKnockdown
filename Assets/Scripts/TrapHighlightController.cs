using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;
public class TrapHighlightController : MonoBehaviour
{
    [SerializeField] private Material _basicMaterial;
    [SerializeField] private Material _highlightMaterial;
    [SerializeField] private Outline _outline;
    public List<Renderer> renderers;
    // Start is called before the first frame update

    private void OnEnable()
    {
        HighlightEnabled(true);
    }



    public void HighlightEnabled(bool enable)
    {
        switch (enable)
        {
            case true:
                ApplyMaterial(_highlightMaterial);
                _outline.enabled = true;
                break;

            case false:
                ApplyMaterial(_basicMaterial);
                _outline.enabled = false;
                break;

        }
        
    }


    /// <summary>
    /// applies the correct material to each mesh in the renderers list
    /// </summary>
    /// <param name="material"></param>
    private void ApplyMaterial(Material material)
    {
        for (int i = 0; i < renderers.Count; i++)
        {
            renderers[i].material = material;
        }

    }


    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

#if UNITY_EDITOR
[CustomEditor(typeof(TrapHighlightController)), CanEditMultipleObjects]
public class TrapHighlightController_Editor : Editor
{


    public override void OnInspectorGUI()
    {
        TrapHighlightController targetObject = (TrapHighlightController)target;
        base.OnInspectorGUI();
        
        if (GUILayout.Button("Register renderers"))
        {
            EditorUtility.SetDirty(target);
            targetObject.renderers.Clear();
            foreach (Transform child in targetObject.transform)
            {

                Renderer component = null;
                child.gameObject.TryGetComponent<Renderer>(out component);

                if (component != null)
                {
                    targetObject.renderers.Add(component);
                }
            }
        }


        if (GUILayout.Button("Remove HighlightControl"))
        {
            EditorUtility.SetDirty(target);
            foreach (Transform child in targetObject.transform)
            {
         
                HighlightControl component = null;
                child.gameObject.TryGetComponent<HighlightControl>(out component);

                if (component != null)
                {
                    DestroyImmediate(component);
                }
               
            }
        }

        
        this.serializedObject.ApplyModifiedProperties();
    }


    }
#endif