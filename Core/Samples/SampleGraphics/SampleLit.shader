// The library's sample shader (scene system 5A2). Library samples are graphics-free: Unity primitives with materials
// made from this shader. Its pass has no LightMode tag, so the Built-in render pipeline draws it as an "Always" pass and
// URP and HDRP draw it as an unlit (SRPDefaultUnlit) pass: one shader looks the same in every pipeline, in edit mode,
// in Play and in a player. Shading is a simple diffuse from a fixed light direction, so it needs no scene lights.
Shader "Abstract Pixel/Samples/Sample Lit"
{
    Properties
    {
        _Color ("Colour", Color) = (0.8, 0.8, 0.8, 1)
        _EmissionColor ("Emission", Color) = (0, 0, 0, 1)
    }

    SubShader
    {
        Tags { "RenderType" = "Opaque" "Queue" = "Geometry" }

        Pass
        {
            CGPROGRAM
            #pragma vertex Vertex
            #pragma fragment Fragment
            #pragma multi_compile_instancing
            #include "UnityCG.cginc"

            fixed4 _Color;
            fixed4 _EmissionColor;

            // Light from above, slightly in front and to the right, as in Unity's default scene.
            static const float3 LightDirection = float3(0.32, 0.87, 0.37);
            // How bright a face turned away from the light still is.
            static const float AmbientLight = 0.42;

            struct VertexInput
            {
                float4 position : POSITION;
                float3 normal : NORMAL;
                UNITY_VERTEX_INPUT_INSTANCE_ID
            };

            struct FragmentInput
            {
                float4 clipPosition : SV_POSITION;
                float3 worldNormal : TEXCOORD0;
                UNITY_VERTEX_OUTPUT_STEREO
            };

            FragmentInput Vertex(VertexInput _input)
            {
                FragmentInput output;
                UNITY_SETUP_INSTANCE_ID(_input);
                UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);
                output.clipPosition = UnityObjectToClipPos(_input.position);
                output.worldNormal = UnityObjectToWorldNormal(_input.normal);
                return output;
            }

            fixed4 Fragment(FragmentInput _input) : SV_Target
            {
                float3 normal = normalize(_input.worldNormal);
                float facing = saturate(dot(normal, normalize(LightDirection)));
                float brightness = AmbientLight + (1.0 - AmbientLight) * facing;
                fixed3 colour = _Color.rgb * brightness + _EmissionColor.rgb;
                return fixed4(colour, 1.0);
            }
            ENDCG
        }
    }

    Fallback Off
}
