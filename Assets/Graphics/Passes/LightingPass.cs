using Unity.Collections;
using Unity.Jobs;
using Unity.Mathematics;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;

using static Unity.Mathematics.math;

public partial class LightingPass
{
    static readonly ProfilingSampler sampler = new("Lighting");

    const int maxDirectionalLightCount = 4;

    static readonly int
        directionalLightCountId = Shader.PropertyToID("_DirectionalLightCount"),
        directionalLightDataId = Shader.PropertyToID("_DirectionalLightData");

    static readonly DirectionalLightData[] directionalLightData =
        new DirectionalLightData[maxDirectionalLightCount];

    BufferHandle directionalLightDataBuffer;

    CullingResults cullingResults;

    int directionalLightCount;

    NativeArray<float4> lightBounds;

    void Setup(
        CullingResults cullingResults,
        int renderingLayerMask)
    {
        this.cullingResults = cullingResults;
        SetupLights(renderingLayerMask);
    }
    void SetupLights(int renderingLayerMask)
    {
        NativeArray<VisibleLight> visibleLights = cullingResults.visibleLights;

        directionalLightCount = 0;
        for (int i = 0; i < visibleLights.Length; i++)
        {
            VisibleLight visibleLight = visibleLights[i];
            Light light = visibleLight.light;
            if ((light.renderingLayerMask & renderingLayerMask) != 0)
            {
                if (directionalLightCount < maxDirectionalLightCount)
                {
                    directionalLightData[directionalLightCount] = new DirectionalLightData(ref visibleLight, light);
                    directionalLightCount++;
                }
                      
            }
        }
    }

    void Render(RenderGraphContext context)
    {
        CommandBuffer buffer = context.cmd;
        buffer.SetGlobalInt(directionalLightCountId, directionalLightCount);
        buffer.SetBufferData(
            directionalLightDataBuffer, directionalLightData,
            0, 0, directionalLightCount);
        buffer.SetGlobalBuffer(
            directionalLightDataId, directionalLightDataBuffer);

        context.renderContext.ExecuteCommandBuffer(buffer);
        buffer.Clear();
        lightBounds.Dispose();
    }

    public static LightResources Record(
        RenderGraph renderGraph,
        CullingResults cullingResults,
        int renderingLayerMask,
        ScriptableRenderContext context)
    {
        using RenderGraphBuilder builder = renderGraph.AddRenderPass(
            sampler.name, out LightingPass pass, sampler);
        pass.Setup(cullingResults, renderingLayerMask);

        pass.directionalLightDataBuffer =
            builder.WriteBuffer(renderGraph.CreateBuffer(
                new BufferDesc(maxDirectionalLightCount, DirectionalLightData.stride) {
                name = "Directional Light Data"
                }
            ));
        
        builder.SetRenderFunc<LightingPass>(
            static (pass, context) => pass.Render(context));
        builder.AllowPassCulling(false);
        return new LightResources(
            pass.directionalLightDataBuffer);
    }
}
