using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections.Generic;
using UnityEngine.Experimental.Rendering;
using UnityEngine.Profiling;
using UnityEngine.Internal.VR;
using UnityEditor;

/// <summary>
/// Renders custom post-process
/// </summary>
public class CustomRenderPass : ScriptableRenderPass
{
    #region Private Members
    /// <summary>
    /// The tag name of the pass
    /// </summary>
    private const string _tagPrefix = "Custom Post Process";
    /// <summary>
    /// The tag name of the pass
    /// </summary>
    private string _tag = _tagPrefix;
    /// <summary>
    /// The material used to render the post process
    /// </summary>
    private Material _material;
    /// <summary>
    /// The source (screen) texture used to apply the postprocess
    /// </summary>
    private RenderTargetIdentifier _sourceRenderTarget;
    /// <summary>
    /// The temporary texture used to apply the postprocess
    /// </summary>
    private RenderTargetHandle _tmpRenderTarget;
    #endregion

    #region Constructor
    public CustomRenderPass(RenderPassEvent evt, Shader shader)
    {
        UpdateStage(evt);
        UpdateShader(shader);
        _tmpRenderTarget.Init("_CustomPostProcessTmpTexture");
    }
    #endregion

    #region Overriden base class functions
    public override void Configure(CommandBuffer cmd, RenderTextureDescriptor cameraTextureDescriptor)
    {
        cameraTextureDescriptor.enableRandomWrite = true;
        cmd.GetTemporaryRT(_tmpRenderTarget.id, cameraTextureDescriptor);
        ConfigureTarget(_tmpRenderTarget.Identifier());
        ConfigureClear(ClearFlag.All, Color.black);
    }

    public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
    {
        var cmd = CommandBufferPool.Get(_tag);

        Blit(cmd, _sourceRenderTarget, _tmpRenderTarget.id, _material);
        cmd.CopyTexture(_tmpRenderTarget.id, _sourceRenderTarget);

        context.ExecuteCommandBuffer(cmd);
        CommandBufferPool.Release(cmd);
    }

    public override void FrameCleanup(CommandBuffer cmd)
    {
        cmd.ReleaseTemporaryRT(_tmpRenderTarget.id);
    } 
    #endregion

    #region Functions
    /// <summary>
    /// Setup the pass for the current frame
    /// </summary>
    /// <param name="sourceRenderTarget"></param>
    public void Setup(in RenderTargetIdentifier sourceRenderTarget)
    {
        _sourceRenderTarget = sourceRenderTarget;
    }

    /// <summary>
    /// Assigns the desired render stage to the render pass
    /// </summary>
    /// <param name="evt">The desired render pass event (stage)</param>
    private void UpdateStage(RenderPassEvent evt)
    {
        renderPassEvent = evt;
    }

    /// <summary>
    /// Assigns the desired shader to the material used to apply the postprocess
    /// </summary>
    /// <param name="shader"></param>
    private void UpdateShader(Shader shader)
    {
        _material = new Material(shader);
        _tag = _tagPrefix + " : " + shader.name + " <----------";
    } 
    #endregion
}