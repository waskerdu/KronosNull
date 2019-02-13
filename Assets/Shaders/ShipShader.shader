// Upgrade NOTE: replaced 'mul(UNITY_MATRIX_MVP,*)' with 'UnityObjectToClipPos(*)'

Shader "Custom/ShipShader" {
	Properties{
	_Main("Main Texture", 2D) = "white"{}
	_Scale("Scale", Float) = 2
	}

		SubShader{
		Tags{
		"Queue" = "Geometry"
		"RenderType" = "Opaque"
	}

		Cull Back
		ZWrite On

		CGPROGRAM
#pragma surface surf Standard fullforwardshadows

	sampler2D _Main;
	float _Scale;

	struct Input
	{
		float3 localPos;
		float3 localNormal;
	};

	void vert(inout appdata_full v, float3 normal : NORMAL, out Input o)
	{
		UNITY_INITIALIZE_OUTPUT(Input, o);
		o.localPos = v.vertex.xyz;
		o.localNormal = normal;
	}

	void surf(Input IN, inout SurfaceOutputStandard o)
	{
		float3 x = tex2D(_Main, frac(IN.localPos.zy * _Scale)) * abs(IN.localNormal.x);
		float3 y = tex2D(_Main, frac(IN.localPos.zx * _Scale)) * abs(IN.localNormal.y);
		float3 z = tex2D(_Main, frac(IN.localPos.xy * _Scale)) * abs(IN.localNormal.z);
		//o.Color = x + y + z;
		o.Albedo = x + y + z;
	}
	ENDCG
	}
		Fallback "Standard"
}
