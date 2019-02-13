// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Unlit/ShieldShader"
{
	Properties
	{
		_Color("_Color", Color) = (0.0,1.0,0.0,1.0)
		_Main("Main Texture", 2D) = "white"{}
		_Scale("Scale", Float) = 2
	}

		SubShader
	{
		Tags
	{
		"Queue" = "Transparent"
		"IgnoreProjector" = "True"
		"RenderType" = "Transparent"

	}


		Cull Back
		ZWrite On
		ZTest LEqual


		CGPROGRAM
		#pragma surface surf BlinnPhong alpha
		//#pragma target 3.0


		fixed4 _Color;
		sampler2D _Main;
		float _Scale;
		sampler2D _CameraDepthTexture;

	struct EditorSurfaceOutput
	{
		half3 Albedo;
		half3 Normal;
		half3 Emission;
		half3 Gloss;
		half Specular;
		half Alpha;
	};

	struct Input
	{
		float3 worldPos;
		float3 worldNormal;
	};

	void surf(Input IN, inout SurfaceOutput o)
	{
		float3 x = tex2D(_Main, frac(IN.worldPos.zy * _Scale)) * abs(IN.worldNormal.x);
		float3 y = tex2D(_Main, frac(IN.worldPos.zx * _Scale)) * abs(IN.worldNormal.y);
		float3 z = tex2D(_Main, frac(IN.worldPos.xy * _Scale)) * abs(IN.worldNormal.z);
		//o.Color = x + y + z;
		//o.Albedo = _Color;
		o.Alpha = x + y + z;
		o.Emission = _Color;
	}

	
	ENDCG
	}
		Fallback "Diffuse"
}
