using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.UIElements;

public class CustomRenderPipeline : RenderPipeline {

    readonly CameraRenderer cameraRenderer;
    readonly RenderGraph renderGraph = new("Custom RP Render Graph");
    readonly CustomRenderPipelineSettings customRenderPipelineSettings;

    public CustomRenderPipeline(CustomRenderPipelineSettings customRenderPipelineSettings)
	{
        this.customRenderPipelineSettings = customRenderPipelineSettings;
		GraphicsSettings.useScriptableRenderPipelineBatching =
			customRenderPipelineSettings.useSRPBatcher;
        cameraRenderer = new CameraRenderer(customRenderPipelineSettings.copyBufferShader);
	}

    protected override void Render(ScriptableRenderContext context, Camera[] cameras)
    {
        // implementing this function is only needed because Render is an abstract function in RenderPipeline
        // but we probably want to use the dynamic array camera list, so we write actual rendering in that instead
    }

    // called every frame by Unity to render the scene
    protected override void Render(ScriptableRenderContext context, List<Camera> cameras)
    {
        for (int i = 0; i < cameras.Count; i++)
        {
            cameraRenderer.Render(renderGraph, context, cameras[i], customRenderPipelineSettings);
        }
        renderGraph.EndFrame();
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        cameraRenderer.Dispose();
        renderGraph.Cleanup();
    }
}