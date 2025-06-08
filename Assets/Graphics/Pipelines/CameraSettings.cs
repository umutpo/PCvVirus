using System;
using UnityEngine;
using UnityEngine.Rendering;

[Serializable]
public class CameraSettings
{
    public bool copyColor = true, copyDepth = true;
    public enum RenderScaleMode
    { Inherit, Override }

    public RenderScaleMode renderScaleMode = RenderScaleMode.Inherit;

    [Range(CameraRenderer.renderScaleMin, CameraRenderer.renderScaleMax)]
    public float renderScale = 1f;

    public float GetRenderScale(float renderBufferScale) =>
        renderScaleMode == RenderScaleMode.Inherit ? renderBufferScale : renderScale;

    public RenderingLayerMask renderingLayerMask = -1;
}
