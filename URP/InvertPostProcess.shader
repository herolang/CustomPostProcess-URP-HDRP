Shader "Invert"
{
    HLSLINCLUDE

        
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Common.hlsl"
        #include "Packages/com.unity.render-pipelines.core/ShaderLibrary/Color.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/ShaderLibrary/Core.hlsl"
        #include "Packages/com.unity.render-pipelines.universal/Shaders/PostProcessing/Common.hlsl"

        TEXTURE2D_X(_BlitTex);

        float4 _BlitTex_TexelSize;

        half3 Fetch(float2 coords, float2 offset)
        {
            float2 uv = coords + offset;
            return SAMPLE_TEXTURE2D_X(_BlitTex, sampler_LinearClamp, uv).xyz;
        }

        half3 Load(int2 icoords, int idx, int idy)
        {
            #if SHADER_API_GLES
            float2 uv = (icoords + int2(idx, idy)) * _BlitTex_TexelSize.xy;
            return SAMPLE_TEXTURE2D_X(_BlitTex, sampler_LinearClamp, uv).xyz;
            #else
            return LOAD_TEXTURE2D_X(_BlitTex, clamp(icoords + int2(idx, idy), 0, _BlitTex_TexelSize.zw - 1.0)).xyz;
            #endif
        }

        half4 Frag(Varyings input) : SV_Target
        {
            UNITY_SETUP_STEREO_EYE_INDEX_POST_VERTEX(input);

            float2 uv = UnityStereoTransformScreenSpaceTex(input.uv);
            float2 positionNDC = uv;
            int2   positionSS  = uv * _BlitTex_TexelSize.zw;

            half3 color = Load(positionSS, 0, 0).xyz;

            color = 1.0f - color;

            return half4(color, 1.0);
        }

    ENDHLSL

    SubShader
    {
        Tags { "RenderType" = "Opaque" "RenderPipeline" = "UniversalPipeline"}
        LOD 100
        ZTest Always ZWrite Off Cull Off

        Pass
        {
            Name "FinalPost"

            HLSLPROGRAM
                #pragma vertex Vert
                #pragma fragment Frag
            ENDHLSL
        }
    }
}
