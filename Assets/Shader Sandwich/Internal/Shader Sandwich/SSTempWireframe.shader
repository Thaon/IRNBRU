Shader "Hidden/SSTempWireframe" {//The Shaders Name
//The inputs shown in the material panel
Properties {
	_MainTex ("Texture", 2D) = "white" {}
	_SSSGreen_Despill ("Green Despill", Range(-1000,1000)) = 0
	_SSSBias ("Bias", Range(-1000,1000)) = 0
	_SSSHardness ("Hardness", Range(-1000,1000)) = 0
}

SubShader {
	Tags { "RenderType"="Opaque""Queue"="Transparent" }//A bunch of settings telling Unity a bit about the shader.
	LOD 200
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _SSSGreen_Despill;
	float _SSSBias;
	float _SSSHardness;
//Setup some time stuff for the Shader Sandwich preview
	float4 _SSTime;
	float4 _SSSinTime;
	float4 _SSCosTime;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLUnlit addshadow  noforwardadd noambient novertexlights nolightmap nodynlightmap nodirlightmap fullforwardshadows
//The Shader Target defines the maximum capabilites of the shader (Number of math operators, texture reads, etc.)
	#pragma target 3.0
//Create a struct which can contain various pixel properties, like specular colors, albedo, normals etc.
	struct CSurfaceOutput 
	{ 
		half3 Albedo; 
		half3 Normal; 
		half3 Emission; 
		half Smoothness; 
		half3 Specular; 
		half Alpha; 
		half Occlusion; 
	};
//Create an Input struct, which lets us read different things that the Surface Shader creates. These are things like the view direction, world position etc.
	struct Input {
		float3 viewDir;
		float2 uv_MainTex;
	};




//Generate simpler lighting code:
half4 LightingCLUnlit (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half4 c;
	//Just pass the color and alpha without adding any lighting.
	c.rgb = float3(1,1,1);
	c.a = s.Alpha;


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
#endif


//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
}
	ENDCG
}

Fallback "VertexLit"
}
