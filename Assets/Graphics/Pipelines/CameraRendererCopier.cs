using UnityEngine;
using UnityEngine.Rendering;

public readonly struct CameraRendererCopier
{
    static readonly int
        sourceTextureID = Shader.PropertyToID("_SourceTexture"),
        srcBlendID = Shader.PropertyToID("_CameraSrcBlend"),
        dstBlendID = Shader.PropertyToID("_CameraDstBlend");

    static readonly bool copyTextureSupported =
        SystemInfo.copyTextureSupport > CopyTextureSupport.None;

    public static bool RequiresRenderTargetResetAfterCopy =>
        !copyTextureSupported;

    readonly Material copyBufferMaterial;

    readonly Camera camera;

    public CameraRendererCopier(
        Material copyBufferMaterial,
        Camera camera)
    {
        this.copyBufferMaterial = copyBufferMaterial;
        this.camera = camera;
    }

    public readonly void Copy(
        CommandBuffer buffer,
        RenderTargetIdentifier from,
        RenderTargetIdentifier to,
        bool isDepth)
    {
        if (copyTextureSupported)
        {
            buffer.CopyTexture(from, to);
        }
        else
        {
            CopyByDrawing(buffer, from, to, isDepth);
        }
    }

    public readonly void CopyByDrawing(
        CommandBuffer buffer,
        RenderTargetIdentifier from,
        RenderTargetIdentifier to,
        bool isDepth)
    {
        buffer.SetGlobalTexture(sourceTextureID, from);
        buffer.SetRenderTarget(
            to, RenderBufferLoadAction.DontCare, RenderBufferStoreAction.Store);
        buffer.SetViewport(camera.pixelRect);
        buffer.DrawProcedural(
            Matrix4x4.identity, copyBufferMaterial, isDepth ? 1 : 0,
            MeshTopology.Triangles, 3);
    }

    public readonly void CopyToCameraTarget(
        CommandBuffer buffer,
        RenderTargetIdentifier from)
    {
        buffer.SetGlobalFloat(srcBlendID, (float) BlendMode.One);
        buffer.SetGlobalFloat(dstBlendID, (float) BlendMode.Zero);
        buffer.SetGlobalTexture(sourceTextureID, from);
        buffer.SetRenderTarget(
            BuiltinRenderTextureType.CameraTarget,
            RenderBufferLoadAction.DontCare,
            RenderBufferStoreAction.Store);
        buffer.SetViewport(camera.pixelRect);
        buffer.DrawProcedural(
            Matrix4x4.identity, copyBufferMaterial, 0, MeshTopology.Triangles, 3);
    }
}
