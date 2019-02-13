Shader "Custom/WallShader" {
	Properties{
		_Main("Main Texture", 2D) = "white"{}
		_Scale("Scale", Float)=2
	}

		SubShader{
		Tags{
		"Queue" = "Geometry"
		"RenderType" = "Opaque"
	}

		Cull Back
		ZWrite On

		CGPROGRAM
//#pragma surface surf Lambert
#pragma surface surf Standard fullforwardshadows
//#pragma surface surfaceFunction lightModel [optionalparams]

		sampler2D _Side, _Top, _Bottom, _Main;
	float _SideScale, _TopScale, _BottomScale, _Scale;

	struct Input
	{
		float3 worldPos;
		float3 worldNormal;
	};

	void surf(Input IN, inout SurfaceOutputStandard o)
	{
		float3 x = tex2D(_Main, frac(IN.worldPos.zy * _Scale)) * abs(IN.worldNormal.x);
		float3 y = tex2D(_Main, frac(IN.worldPos.zx * _Scale)) * abs(IN.worldNormal.y);
		float3 z = tex2D(_Main, frac(IN.worldPos.xy * _Scale)) * abs(IN.worldNormal.z);
		//o.Color = x + y + z;
		o.Albedo = x + y + z;
	}
	ENDCG
	}
		Fallback "Standard"
}
