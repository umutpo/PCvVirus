using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.Rendering;

partial class LightingPass
{
    [StructLayout(LayoutKind.Sequential)]
    struct DirectionalLightData
    {
        // 4 bytes in an int * vector size * number of vectors
        public const int stride = 4 * 4 * 2;

        public Vector4 color, directionAndMask;

        public DirectionalLightData(
            ref VisibleLight visibleLight, Light light)
        {
            color = visibleLight.finalColor;
            directionAndMask = -visibleLight.localToWorldMatrix.GetColumn(2);
            directionAndMask.w = light.renderingLayerMask;
        }
    }
}
