using ICSharpCode.NRefactory.Ast;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class CustomRenderFeature : ScriptableRendererFeature
{
    private CustomRenderPass _scriptablePass;
    private bool _initialized;

    public override void Create()
    {
    }

    public void Initialize(RenderPassEvent renderPassEvent, Shader shader)
    {
        _scriptablePass = new CustomRenderPass(renderPassEvent, shader);
        _initialized = true;
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if(_initialized)
        {
            _scriptablePass.Setup(renderer.cameraColorTarget);
            renderer.EnqueuePass(_scriptablePass);
        }
        else
        {
            Debug.LogWarning(this.name + " ( type : " + this.GetType().ToString() + ") was not initialized. Please use Initialize() right after instantiation.");
        }
    }
}


