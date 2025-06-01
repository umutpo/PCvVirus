using System;
using UnityEngine;

[Serializable]
public class CustomRenderPipelineSettings
{
    public bool useSRPBatcher;
    // used by cameras to copy their contents onto other image buffers
    public Shader copyBufferShader;

    public CameraBufferSettings cameraBuffer = new()
    {
        allowHDR = true,
        renderScale = 1f
    };
}
