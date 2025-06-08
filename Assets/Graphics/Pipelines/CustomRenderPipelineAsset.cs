using System;
using UnityEngine;
using UnityEngine.Rendering;

[CreateAssetMenu(menuName = "Graphics/Custom RP Asset")]
public class CustomRenderPipelineAsset : RenderPipelineAsset {
    [SerializeField]
    CustomRenderPipelineSettings renderPipelineSettings;
    public override Type pipelineType => typeof(CustomRenderPipeline);

    // used to identify render pipeline in shaders, which is unneeded
    public override string renderPipelineShaderTag => string.Empty;

    protected override RenderPipeline CreatePipeline()
    {
        return new CustomRenderPipeline(renderPipelineSettings);
    }
}
