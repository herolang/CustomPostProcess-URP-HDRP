using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.HighDefinition;
using UnityEngine.Rendering;
using UnityEngine.Experimental.Rendering;

public class CustomPostProcessPass : CustomPass
{
    #region Private Members
    /// <summary>
    /// The shader used for applying the postprocess
    /// </summary>
    private Shader _shader;
    /// <summary>
    /// The material used for applying the postprocess
    /// </summary> 
    private Material _material;
    #endregion

    #region Constructor
    public CustomPostProcessPass(Shader shader)
    {
        _shader = shader;
        _material = CoreUtils.CreateEngineMaterial(_shader);
    } 
    #endregion

    #region Overriden base class functions
    // It can be used to configure render targets and their clear state. Also to create temporary render target textures.
    // When empty this render pass will render to the active camera render target.
    // You should never call CommandBuffer.SetRenderTarget. Instead call <c>ConfigureTarget</c> and <c>ConfigureClear</c>.
    // The render pipeline will ensure target setup and clearing happens in an performance manner.
    protected override void Setup(ScriptableRenderContext renderContext, CommandBuffer cmd)
    {
    }

    protected override void Execute(ScriptableRenderContext renderContext, CommandBuffer cmd, HDCamera camera, CullingResults cullingResult)
    {
        SetCameraRenderTarget(cmd);
        CoreUtils.DrawFullScreen(cmd, _material);
    }

    protected override void Cleanup()
    {
    } 
    #endregion
}
