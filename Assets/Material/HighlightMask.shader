Shader "Custom/Highlight Mask" 
{
  SubShader 
  {
    Tags 
    {
      "Queue" = "Transparent+100"
      "RenderType" = "Transparent"
    }

    Pass 
    {
      Name "Mask"
      Cull Off
      ZTest false
      ZWrite Off
      ColorMask 0

      Stencil 
      {
        Ref 1
        Pass Replace
      }
    }
  }
}
