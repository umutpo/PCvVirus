using UnityEngine;

public class CameraBufferSettings
{
    public bool allowHDR;

    public bool copyColor, copyDepth;
    
    [Range(CameraRenderer.renderScaleMin, CameraRenderer.renderScaleMax)]
    public float renderScale;

}
