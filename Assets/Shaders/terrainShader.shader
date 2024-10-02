Shader "RetroTerrain/Terrain" 
{
    Properties
    {
        _MainTex("MainTex", 2D) = "white"{}
        _Layer1("Layer1", 2D) = "white"{}
        _Layer2("Layer2", 2D) = "white"{}
        _Layer3("Layer3", 2D) = "white"{}
        _Layer4("Layer4", 2D) = "white"{}
        _Layer5("Layer5", 2D) = "white"{}
        _Layer6("Layer6", 2D) = "white"{}
        _Layer7("Layer7", 2D) = "white"{}
        _Layer8("Layer8", 2D) = "white"{}
        _Layer9("Layer9", 2D) = "white"{}
        _LineThickness ("LineThickness", Range(0, 0.025)) = 0.02
        _LineVisibility ("LineVisibility", Range(0, 1)) = 0.2
    }

    SubShader 
    {
    Tags { "RenderType" = "Opaque" }
    CGPROGRAM
    #pragma surface surf Lambert addshadow
      
    struct Input 
    {
      float2 uv_MainTex;
      float2 uv2_MainTex;
      float2 uv3_MainTex;
      float2 uv4_MainTex;
      fixed4 color : COLOR;
    };

    sampler2D _MainTex;
    sampler2D _Layer1;
    sampler2D _Layer2;
    sampler2D _Layer3;
    sampler2D _Layer4;
    sampler2D _Layer5;
    sampler2D _Layer6;
    sampler2D _Layer7;
    sampler2D _Layer8;
    sampler2D _Layer9;
    float _LineThickness;
    float _LineVisibility;

    void surf (Input IN, inout SurfaceOutput o) 
    {
      fixed4 layer0 = tex2D(_MainTex, IN.uv_MainTex).rgba * IN.color.r;
      fixed4 layer1 = tex2D(_Layer1, IN.uv_MainTex).rgba * IN.color.g;
      fixed4 layer2 = tex2D(_Layer2, IN.uv_MainTex).rgba * IN.color.b;
      fixed4 layer3 = tex2D(_Layer3, IN.uv_MainTex).rgba * IN.color.a;

      fixed4 layer4 = tex2D(_Layer4, IN.uv_MainTex).rgba * IN.uv2_MainTex.x;
      fixed4 layer5 = tex2D(_Layer5, IN.uv_MainTex).rgba * IN.uv2_MainTex.y;

      fixed4 layer6 = tex2D(_Layer6, IN.uv_MainTex).rgba * IN.uv3_MainTex.x;
      fixed4 layer7 = tex2D(_Layer7, IN.uv_MainTex).rgba * IN.uv3_MainTex.y;

      fixed4 layer8 = tex2D(_Layer8, IN.uv_MainTex).rgba * IN.uv4_MainTex.x;
      fixed4 layer9 = tex2D(_Layer9, IN.uv_MainTex).rgba * IN.uv4_MainTex.y;
      float4 lines = float(1.0);


      if((IN.uv_MainTex.x < _LineThickness || IN.uv_MainTex.x > 1.0 - _LineThickness) || (IN.uv_MainTex.y < _LineThickness || IN.uv_MainTex.y > 1.0 - _LineThickness))
      {
          lines = 1 - _LineVisibility;
      }
      o.Albedo =  (layer0 + layer1 + layer2  + layer3 + layer4 + layer5 + layer6 + layer7 + layer8 + layer9)* lines;
    }
      ENDCG
    }

    Fallback "Diffuse"
  }