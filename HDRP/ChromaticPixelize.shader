Shader "Chromatic Pixelize"
{
    HLSLINCLUDE

    #pragma vertex Vert

    #pragma target 4.5
    #pragma only_renderers d3d11 ps4 xboxone vulkan metal switch

    #include "Packages/com.unity.render-pipelines.high-definition/Runtime/RenderPipeline/RenderPass/CustomPass/CustomPassCommon.hlsl"

    void PixelizeUV(inout float2 uv, float2 blocksCount, int blockSize)
    {
        uv -= 0.5f;
        uv *= blocksCount;
        uv = floor(uv + 0.5f);
        uv /= blocksCount;
        uv += 0.5f;
    }

#define PI 3.1415926f
#define OFFSET (PI/8)

    float4 frag(Varyings varyings) : SV_Target
    {
        float depth = LoadCameraDepth(varyings.positionCS.xy);
        PositionInputs posInput = GetPositionInput(varyings.positionCS.xy, _ScreenSize.zw, depth, UNITY_MATRIX_I_VP, UNITY_MATRIX_V);

        float speed = 1.0f;

        float2 uvR = posInput.positionNDC.xy;
        float2 uvG = uvR;
        float2 uvB = uvR;

        int blockSize = round(lerp(1, 64, sin(_Time.y) * 0.5f + 0.5f));
        float2 blocksCount = _ScreenSize.xy / blockSize;
        PixelizeUV(uvR, blocksCount, blockSize);

        blockSize = round(lerp(1, 64, sin(_Time.y + OFFSET) * 0.5f + 0.5f));
        blocksCount = _ScreenSize.xy / blockSize;
        PixelizeUV(uvG, blocksCount, blockSize);

        blockSize = round(lerp(1, 64, sin(_Time.y + OFFSET * 2) * 0.5f + 0.5f));
        blocksCount = _ScreenSize.xy / blockSize;
        PixelizeUV(uvB, blocksCount, blockSize);

        float colorR = float4(CustomPassSampleCameraColor(uvR, 0), 1).x;
        float colorG = float4(CustomPassSampleCameraColor(uvG, 0), 1).y;
        float colorB = float4(CustomPassSampleCameraColor(uvB, 0), 1).z;

        return float4(colorR, colorG, colorB, 1.0f);
    }
    ENDHLSL

    SubShader
    {
        Pass
        {
            ZWrite Off
            ZTest Always
            Blend Off
            Cull Off

            HLSLPROGRAM
                #pragma fragment frag
            ENDHLSL
        }
    }
    Fallback Off
}


    // The PositionInputs struct allow you to retrieve a lot of useful information for your fullScreenShader:
    // struct PositionInputs
    // {
    //     float3 positionWS;  // World space position (could be camera-relative)
    //     float2 positionNDC; // Normalized screen coordinates within the viewport    : [0, 1) (with the half-pixel offset)
    //     uint2  positionSS;  // Screen space pixel coordinates                       : [0, NumPixels)
    //     uint2  tileCoord;   // Screen tile coordinates                              : [0, NumTiles)
    //     float  deviceDepth; // Depth from the depth buffer                          : [0, 1] (typically reversed)
    //     float  linearDepth; // View space Z coordinate                              : [Near, Far]
    // };

    // To sample custom buffers, you have access to these functions:
    // But be careful, on most platforms you can't sample to the bound color buffer. It means that you
    // can't use the SampleCustomColor when the pass color buffer is set to custom (and same for camera the buffer).
    // float4 SampleCustomColor(float2 uv);
    // float4 LoadCustomColor(uint2 pixelCoords);
    // float LoadCustomDepth(uint2 pixelCoords);
    // float SampleCustomDepth(float2 uv);

    // There are also a lot of utility function you can use inside Common.hlsl and Color.hlsl,
    // you can check them out in the source code of the core SRP package.