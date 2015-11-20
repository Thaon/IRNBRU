Shader "Hidden/SS/LegacyDiffuse" {
Properties {
_MainTex ("Base", 2D) = "white" {}
_Color ("Color", Color) = (1,1,1,1)
}

SubShader {
Tags { "RenderType"="Opaque" }
LOD 200
	ZWrite On
	cull Back
	CGPROGRAM

sampler2D _MainTex;
float4 _Color;
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  fullforwardshadows
	#pragma target 3.0
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
struct Input {
	float3 viewDir;
	float2 uv_MainTex;
};
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
half3 lightColor = _LightColor0.rgb;
	s.Normal = normalize(s.Normal);
	half NdotL = max (0, dot (s.Normal, lightDir));
	half4 c;
	c.rgb = s.Albedo * lightColor * (NdotL * atten * 2);
	c.a = s.Alpha;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 lightColor = light.color;
	half3 lightDir = light.dir;
	s.Normal = normalize(s.Normal);
	half NdotL = max (0, dot (s.Normal, lightDir));
	half4 c;
	c.rgb = s.Albedo * lightColor * (NdotL );
	c.a = s.Alpha;

	return c;
}
half4 LightingCLStandard (CSurfaceOutput s, half3 viewDir, UnityGI gi) {
	half4 c;
	c = LightingCLStandardLight(s,viewDir,gi.light);
	#if defined(DIRLIGHTMAP_SEPARATE)
		#ifdef LIGHTMAP_ON
			c += LightingCLStandardLight(s,viewDir,gi.light2);
		#endif
		#ifdef DYNAMICLIGHTMAP_ON
			c += LightingCLStandardLight(s,viewDir,gi.light3);
		#endif
	#endif

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT
		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal, false);
}
#endif

void vert (inout appdata_full v, out Input o) {
	UNITY_INITIALIZE_OUTPUT(Input, o);
float SSShellDepth = 0;

float4 Vertex = v.vertex;
float Mask0 = 0;
//Mask0
//Vertex

v.vertex.rgb = Vertex;
}

void frag_surf (Input IN, inout CSurfaceOutput o) {
float SSShellDepth = 1-0;
float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	o.Albedo = float3(0.8,0.8,0.8);
	float4 Emission = float4(0,0,0,0);
	o.Smoothness = 0;
	o.Alpha = 1.0;
	o.Occlusion = 1.0;
	o.Specular = float3(0.3,0.3,0.3);

float Mask0 = 0;
//Mask0
//Normals
//Diffuse
float4 Texture_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));
o.Albedo= Texture_Sample1.rgb;
float4 Texture2_Sample1 = _Color;
o.Albedo*= Texture2_Sample1.rgb;
}
	ENDCG
}

Fallback "VertexLit"
}

/*
BeginShaderParse
0.9
BeginShaderBase
BeginShaderInput
Type #! 0 #?Type
VisName #! Base #?VisName
ImageDefault #! 0 #?ImageDefault
Image #! 9f3187bc2c72c9842b24739d8ab2a1ed #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 2 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 1,1,1,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 1 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
ShaderName #! SS/LegacyDiffuse #?ShaderName
Hard Mode #! True #?Hard Mode
Tech Lod #! 200 #?Tech Lod
Cull #! 1 #?Cull
Tech Shader Target #! 3 #?Tech Shader Target
Vertex Recalculation #! False #?Vertex Recalculation
Use Fog #! True #?Use Fog
Use Ambient #! True #?Use Ambient
Use Vertex Lights #! True #?Use Vertex Lights
Use Lightmaps #! True #?Use Lightmaps
Use All Shadows #! True #?Use All Shadows
Diffuse On #! True #?Diffuse On
Lighting Type #! 0 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #?Color
Setting1 #! 0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #?Wrap Color
Specular On #! False #?Specular On
Specular Type #! 0 #?Specular Type
Spec Hardness #! 0.3 #?Spec Hardness
Spec Color #! 0.8,0.8,0.8,1 #?Spec Color
Spec Energy Conserve #! True #?Spec Energy Conserve
Spec Offset #! 0 #?Spec Offset
Emission On #! False #?Emission On
Emission Color #! 0,0,0,0 #?Emission Color
Emission Type #! 0 #?Emission Type
Transparency On #! False #?Transparency On
Transparency Type #! 0 #?Transparency Type
ZWrite #! False #?ZWrite
Use PBR #! True #?Use PBR
Transparency #! 1 #?Transparency
Receive Shadows #! False #?Receive Shadows
ZWrite Type #! 0 #?ZWrite Type
Blend Mode #! 0 #?Blend Mode
Shells On #! False #?Shells On
Shell Count #! 1 #?Shell Count
Shells Distance #! 0.1 #?Shells Distance
Shell Ease #! 0 #?Shell Ease
Shell Transparency Type #! 0 #?Shell Transparency Type
Shell Transparency ZWrite #! False #?Shell Transparency ZWrite
Shell Cull #! 0 #?Shell Cull
Shells ZWrite #! True #?Shells ZWrite
Shells Use Transparency #! True #?Shells Use Transparency
Shell Blend Mode #! 0 #?Shell Blend Mode
Shells Transparency #! 1 #?Shells Transparency
Shell Lighting #! True #?Shell Lighting
Shell Front #! True #?Shell Front
Parallax On #! False #?Parallax On
Parallax Height #! 0.1 #?Parallax Height
Parallax Quality #! 10 #?Parallax Quality
Silhouette Clipping #! False #?Silhouette Clipping
BeginShaderLayerList
LayerListUniqueName #! Mask0 #?LayerListUniqueName
LayerListName #! Mask0 #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Diffuse #?LayerListUniqueName
LayerListName #! Diffuse #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
BeginShaderLayer
Layer Name #! Texture #?Layer Name
Layer Type #! 3 #?Layer Type
Main Color #! 0.8,0.8,0.8,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #! 9f3187bc2c72c9842b24739d8ab2a1ed #^ 0 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Texture2 #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 1,1,1,1 #^ 1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 3 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellDiffuse #?LayerListUniqueName
LayerListName #! Diffuse #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Alpha #?LayerListUniqueName
LayerListName #! Alpha #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! a #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellAlpha #?LayerListUniqueName
LayerListName #! Alpha #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! a #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Specular #?LayerListUniqueName
LayerListName #! Specular #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellSpecular #?LayerListUniqueName
LayerListName #! Specular #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Normals #?LayerListUniqueName
LayerListName #! Normals #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellNormals #?LayerListUniqueName
LayerListName #! Normals #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Emission #?LayerListUniqueName
LayerListName #! Emission #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgba #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellEmission #?LayerListUniqueName
LayerListName #! Emission #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgba #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Height #?LayerListUniqueName
LayerListName #! Height #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! a #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Vertex #?LayerListUniqueName
LayerListName #! Vertex #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgba #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellVertex #?LayerListUniqueName
LayerListName #! Vertex #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgba #?EndTag
EndShaderLayerList
EndShaderBase
EndShaderParse
*/
