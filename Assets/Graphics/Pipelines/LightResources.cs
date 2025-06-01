using UnityEngine.Rendering.RenderGraphModule;

public readonly ref struct LightResources
{
    public readonly BufferHandle directionalLightDataBuffer;

    public LightResources(
        BufferHandle directionalLightDataBuffer)
    {
        this.directionalLightDataBuffer = directionalLightDataBuffer;
    }
}
