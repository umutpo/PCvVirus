using UnityEditor.Rendering.LookDev;
using UnityEngine;
using UnityEngine.LightTransport;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using static UnityEditor.SceneView;

public class CameraRenderer
{

    public const float renderScaleMin = 0.1f, renderScaleMax = 2f;

    static readonly CameraSettings defaultCameraSettings = new();
    readonly Material copyBufferMaterial;

    public CameraRenderer(Shader copyBufferShader)
    {
        copyBufferMaterial = CoreUtils.CreateEngineMaterial(copyBufferShader);
    }

    public void Dispose()
    {
        CoreUtils.Destroy(copyBufferMaterial);
    }

    public void Render(RenderGraph renderGraph, ScriptableRenderContext context, Camera camera, CustomRenderPipelineSettings customRenderPipelineSettings)
    {
        CameraBufferSettings bufferSettings = customRenderPipelineSettings.cameraBuffer;

        ProfilingSampler cameraSampler;
        CameraSettings cameraSettings;
        if (camera.TryGetComponent(out CustomRenderPipelineCamera crpCamera))
        {
            cameraSampler = crpCamera.Sampler;
            cameraSettings = crpCamera.Settings;
        }
        else
        {
            cameraSampler = ProfilingSampler.Get(camera.cameraType);
            cameraSettings = defaultCameraSettings;
        }

        float renderScale = cameraSettings.GetRenderScale(
            bufferSettings.renderScale);
        bool useScaledRendering = renderScale < 0.99f || renderScale > 1.01f;

#if UNITY_EDITOR
        // don't do up/downsampling in the editor window
        if (camera.cameraType == CameraType.SceneView)
        {
            ScriptableRenderContext.EmitWorldGeometryForSceneView(camera);
            useScaledRendering = false;
        }
#endif

        if (!camera.TryGetCullingParameters(out ScriptableCullingParameters scriptableCullingParameters))
        {
            return;
        }

        CullingResults cullingResults = context.Cull(
            ref scriptableCullingParameters);

        // size of the image buffers to render to
        Vector2Int bufferSize = default;
        if (useScaledRendering)
        {
            renderScale = Mathf.Clamp(renderScale, renderScaleMin, renderScaleMax);
            bufferSize.x = (int)(camera.pixelWidth * renderScale);
            bufferSize.y = (int)(camera.pixelHeight * renderScale);
        }
        else
        {
            bufferSize.x = camera.pixelWidth;
            bufferSize.y = camera.pixelHeight;
        }
        bufferSettings.allowHDR &= camera.allowHDR;
        RenderGraphRecording(renderGraph, context, cullingResults, camera, cameraSampler, cameraSettings, bufferSettings, bufferSize);
    }
    void RenderGraphRecording(RenderGraph renderGraph, ScriptableRenderContext context, CullingResults cullingResults, Camera camera, ProfilingSampler cameraSampler, CameraSettings cameraSettings, CameraBufferSettings bufferSettings, Vector2Int bufferSize)
    {
        var renderGraphParameters = new RenderGraphParameters
        {
            commandBuffer = CommandBufferPool.Get(),
            currentFrameIndex = Time.frameCount,
            executionName = cameraSampler.name,
            rendererListCulling = true,
            scriptableRenderContext = context
        };
        bool useColorTexture = bufferSettings.copyColor && cameraSettings.copyColor;
        bool useDepthTexture = bufferSettings.copyDepth && cameraSettings.copyDepth;

        renderGraph.BeginRecording(renderGraphParameters);
        using (new RenderGraphProfilingScope(renderGraph, cameraSampler))
        {
            LightResources lightResources = LightingPass.Record(
                renderGraph, cullingResults, cameraSettings.renderingLayerMask, context);
            
            CameraRendererTextures textures = SetupPass.Record(
                renderGraph, useColorTexture, useDepthTexture,
                bufferSettings.allowHDR, bufferSize, camera);
            
            GeometryPass.Record(
                renderGraph, camera, cullingResults,
                cameraSettings.renderingLayerMask,
                textures, lightResources);
            
            var copier = new CameraRendererCopier(
                copyBufferMaterial, camera);
            CopyAttachmentsPass.Record(
                renderGraph, useColorTexture, useDepthTexture,
                copier, textures);
            
            FinalPass.Record(renderGraph, copier, textures);
        }
        renderGraph.EndRecordingAndExecute();


        context.ExecuteCommandBuffer(renderGraphParameters.commandBuffer);
        context.Submit();
        CommandBufferPool.Release(renderGraphParameters.commandBuffer);
    }
}
