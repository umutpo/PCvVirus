using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.RenderGraphModule;
using UnityEngine.Rendering.RendererUtils;

public class GeometryPass
{
    static readonly ProfilingSampler
        samplerOpaque = new("Opaque Geometry");

    static readonly ShaderTagId[] shaderTagIDs = {
        new("SRPDefaultUnlit")
    };

    RendererListHandle list;

    void Render(RenderGraphContext context)
    {
        context.cmd.DrawRendererList(list);
        context.renderContext.ExecuteCommandBuffer(context.cmd);
        context.cmd.Clear();
    }

    public static void Record(
        RenderGraph renderGraph,
        Camera camera,
        CullingResults cullingResults,
        uint renderingLayerMask,
        in CameraRendererTextures textures,
        in LightResources lightData)
    {
        ProfilingSampler sampler = samplerOpaque;

        using RenderGraphBuilder builder = renderGraph.AddRenderPass(
            sampler.name, out GeometryPass pass, sampler);

        pass.list = builder.UseRendererList(renderGraph.CreateRendererList(
            new RendererListDesc(shaderTagIDs, cullingResults, camera)
            {
                sortingCriteria = SortingCriteria.CommonOpaque,
                rendererConfiguration = 0,
                renderQueueRange = RenderQueueRange.opaque,
                renderingLayerMask = renderingLayerMask
            }));

        builder.ReadWriteTexture(textures.colorAttachment);
        builder.ReadWriteTexture(textures.depthAttachment);

        builder.ReadBuffer(lightData.directionalLightDataBuffer);
        builder.SetRenderFunc<GeometryPass>(
            static (pass, context) => pass.Render(context));
    }
}