using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering;
using System;
using System.Reflection;

[ExecuteInEditMode]
public class CustomPostProcessing : MonoBehaviour
{
    #region Public members
    /// <summary>
    /// The shader used for the post process
    /// </summary>
    public Shader shader;
    /// <summary>
    /// The desired moment to apply the post process
    /// </summary>
    public RenderPassEvent stage = RenderPassEvent.AfterRendering;

    #endregion

    #region Private Members
    /// <summary>
    /// The renderer used to render the current camera
    /// </summary>
    private ScriptableRenderer _renderer;
    /// <summary>
    /// The renderer feature used to apply the post process
    /// </summary>
    private CustomRenderFeature _customRenderFeature;
    /// <summary>
    /// The reflected link to the private list of the renderer features set in the renderer
    /// </summary>
    private static FieldInfo _activeFeaturesFieldInfo;
    /// <summary>
    /// The list of renderer features set in the renderer
    /// </summary>
    private static List<ScriptableRendererFeature> _activeFeaturesList;
    /// <summary>
    /// Tells if the script has been correctly initialized
    /// </summary>
    private bool _initialized = false;

    #endregion

    #region Properties
    /// <summary>
    /// Accesses/populates the _activeFeaturesFieldInfo member
    /// </summary>
    private FieldInfo ActiveFeaturesFieldInfo
    {
        get
        {
            if (_activeFeaturesFieldInfo == null)
            {
                BindingFlags bindFlags = BindingFlags.NonPublic | BindingFlags.GetField | BindingFlags.Instance;
                Type rendererType = typeof(ScriptableRenderer);
                _activeFeaturesFieldInfo = rendererType.GetField("m_RendererFeatures", bindFlags);
            }

            return _activeFeaturesFieldInfo;
        }
    }
    #endregion

    #region Monobehaviour functions
    void OnEnable()
    {
        Initialize();
    }

    void OnDisable()
    {
        Uninitialize();
    }

    private void OnValidate()
    {
        Initialize();
    }
    #endregion

    #region Functions
    /// <summary>
    /// Initializes the script
    /// </summary>
    private void Initialize()
    {
        if (shader == null)
        {
            enabled = false;
            return;
        }

        Uninitialize();

        _renderer = GetComponent<UniversalAdditionalCameraData>().scriptableRenderer;

        _customRenderFeature = ScriptableObject.CreateInstance<CustomRenderFeature>();
        _customRenderFeature.name = "Custom post-process " + this.GetType().ToString() + " on " + gameObject.name;
        _customRenderFeature.Initialize(stage, shader);

        AddFeatureToRenderer(_customRenderFeature);

        _initialized = true;
    }

    /// <summary>
    /// Uninitializes the script
    /// </summary>
    private void Uninitialize()
    {
        if (_initialized)
        {
            RemoveFeatureFromRenderer(_customRenderFeature);

#if UNITY_EDITOR
            DestroyImmediate(_customRenderFeature);
#else
            Destroy(_customRenderFeature);
#endif

            _initialized = false;
        }
    }

    /// <summary>
    /// Retrieves the current list of renderer features set in the renderer
    /// </summary>
    private void GetFeaturesFromRenderer()
    {
        _activeFeaturesList = (List<ScriptableRendererFeature>)ActiveFeaturesFieldInfo.GetValue(_renderer);
    }

    /// <summary>
    /// Set the local list of renderer features to the renderer
    /// </summary>
    private void SetFeaturesToRenderer()
    {
        ActiveFeaturesFieldInfo.SetValue(_renderer, _activeFeaturesList);
    }

    /// <summary>
    /// Adds a renderer feature to the renderer
    /// </summary>
    /// <param name="rendererFeature">The renderer feature to add to the renderer</param>
    private void AddFeatureToRenderer(ScriptableRendererFeature rendererFeature)
    {
        GetFeaturesFromRenderer();
        _activeFeaturesList.Add(rendererFeature);
        SetFeaturesToRenderer();
    }

    /// <summary>
    /// Removes a renderer feature from the renderer
    /// </summary>
    /// <param name="rendererFeature">The renderer feature to remove from the renderer</param>
    private void RemoveFeatureFromRenderer(ScriptableRendererFeature rendererFeature)
    {
        GetFeaturesFromRenderer();
        _activeFeaturesList.Remove(rendererFeature);
        SetFeaturesToRenderer();
    }
    #endregion
}
