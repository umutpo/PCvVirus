Shader "Custom RP/UnlitShader"
{
    Properties
    {
        [HideInInspector] _MainTex ("Texture", 2D) = "white" {}
    }
    SubShader
    {
        HLSLINCLUDE
        #include "../ShaderLibrary/Common.hlsl"
        ENDHLSL 

        Pass
        {
            HLSLPROGRAM
            #pragma target 3.5
			#pragma vertex UnlitPassVertex
			#pragma fragment UnlitPassFragment
            #include "UnlitPass.hlsl"
            ENDHLSL
        }
    }
}
