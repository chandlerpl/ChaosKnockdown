Shader "Custom/Highlight" 
{
  Properties 
  {
    _HighlightColor("Highlight Color", Color) = (0, 1, 0, 1)
    _HighlightThickness("Highlight Thickness", Range(0, 10)) = 2
  }

  SubShader 
  {
    Tags 
    {
      "Queue" = "Transparent+110"
      "RenderType" = "Transparent"
      "DisableBatching" = "True"
    }

    Pass 
    {
      Name "Fill"
      Cull Off
      ZTest false
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB

      Stencil 
      {
        Ref 1
        Comp NotEqual
      }

      CGPROGRAM
      #include "UnityCG.cginc"

      #pragma vertex vert
      #pragma fragment frag

      struct a2v 
      {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f 
      {
        float4 position : SV_POSITION;
        fixed4 color : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      uniform fixed4 _HighlightColor;
      uniform float _HighlightThickness;

      v2f vert(a2v input) 
      {
        v2f output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normal;
        float3 viewPosition = UnityObjectToViewPos(input.vertex);
        float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

        output.position = UnityViewToClipPos(viewPosition + viewNormal * -viewPosition.z * _HighlightThickness / 1000.0);
        output.color = _HighlightColor;

        return output;
      }

      fixed4 frag(v2f input) : SV_Target 
      {
        return input.color;
      }
      ENDCG
    }
  }
}
