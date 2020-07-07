using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;
using System;
using System.Reflection;

#if UNITY_EDITOR
using UnityEditor;

[CustomEditor(typeof(CustomPostProcess))]
[DisallowMultipleComponent]
public class CustomPostProcessEditor : Editor
{
    #region Private Members
    /// <summary>
    /// The inspected component
    /// </summary>
    private CustomPostProcess _component;
    /// <summary>
    /// The property for injection Point
    /// </summary>
    private SerializedProperty _injectionPointProperty;
    /// <summary>
    /// The property for shader
    /// </summary>
    private SerializedProperty _shaderProperty;
    #endregion

    #region Overriden base class functions (https://docs.unity3d.com/ScriptReference/Editor.html)
    private void OnEnable()
    {
        _component = (CustomPostProcess)target;
        _injectionPointProperty = serializedObject.FindProperty("injectionPoint");
        _shaderProperty = serializedObject.FindProperty("shader");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(_injectionPointProperty);
        EditorGUILayout.PropertyField(_shaderProperty);

        serializedObject.ApplyModifiedProperties();
    }
    #endregion
}

#endif

public class CustomPostProcess : CustomPassVolume
{
    #region Public Members
    /// <summary>
    /// The shader used for applying the postprocess
    /// </summary>
    public Shader shader;
    #endregion

    #region Private Members
    /// <summary>
    /// The post process pass used to apply the post process
    /// </summary>
    private CustomPostProcessPass _customPass;
    /// <summary>
    /// The reflected link to the private static list of registred custom passes
    /// </summary>
    private static FieldInfo _activePassesFieldInfo;
    /// <summary>
    /// The table of active custom passes
    /// </summary>
    private HashSet<CustomPassVolume> _activePassVolumes;
    #endregion

    #region Properties
    /// <summary>
    /// Accesses/populates the _activePassesFieldInfo member
    /// </summary>
    private FieldInfo ActivePassesFieldInfo
    {
        get
        {
            if (_activePassesFieldInfo == null)
            {
                BindingFlags bindFlags = BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Static;
                _activePassesFieldInfo = typeof(CustomPassVolume).GetField("m_ActivePassVolumes", bindFlags);
            }

            return _activePassesFieldInfo;
        }
    }
    #endregion

    #region Unity functions
    private void OnEnable()
    {
        if (shader == null)
        {
            enabled = false;
            return;
        }

        _customPass = new CustomPostProcessPass(shader);

        customPasses.Add(_customPass);

        AddPass(this);
    }

    private void OnDisable()
    {
        RemovePass(this);
        customPasses.Clear();
        _customPass = null;
    }

    private void OnValidate()
    {
        Reset();
    }

    private void Reset()
    {
        OnDisable();
        OnEnable();
    }
    #endregion

    #region Functions
    /// <summary>
    /// Retrieves the current list of custom passes
    /// </summary>
    private void GetActivePasses()
    {
        _activePassVolumes = (HashSet<CustomPassVolume>)ActivePassesFieldInfo.GetValue(null);
    }

    /// <summary>
    /// Set the local table of custom passes
    /// </summary>
    private void SetActivePasses()
    {
        ActivePassesFieldInfo.SetValue(null, _activePassVolumes);
    }

    /// <summary>
    /// Adds a custom pass volume to the active custom passes table
    /// </summary>
    /// <param name="pass">The custom pass volume to add to the active custom passes table</param>
    private void AddPass(CustomPassVolume pass)
    {
        GetActivePasses();
        _activePassVolumes.Add(pass);
        SetActivePasses();
    }

    /// <summary>
    /// Removes a custom pass volume from the active custom passes table
    /// </summary>
    /// <param name="pass">The custom pass volume to remove from the active custom passes table</param>
    private void RemovePass(CustomPassVolume pass)
    {
        GetActivePasses();
        _activePassVolumes.Remove(pass);
        SetActivePasses();
    } 
    #endregion
}
