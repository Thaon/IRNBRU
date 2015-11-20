Shader "Hidden/SSCubemapSkybox" {
Properties {
_Cube ("Texture - Cubemap", Cube) = "bump" {}
}

SubShader {
Tags { "RenderType"="Opaque" "Queue" = "Background"}
LOD 200
	cull Front
	CGPROGRAM

samplerCUBE _Cube;
float4 _SSTime;
float4 _SSSinTime;
float4 _SSCosTime;
	#pragma surface frag_surf CLUnlit vertex:vert noforwardadd noambient novertexlights nolightmap nodynlightmap nodirlightmap
	#pragma target 3.0
struct CSurfaceOutput 
{ 
	half3 Albedo; 
	half3 Normal; 
	half3 Emission; 
	half Specular; 
	half3 Gloss; 
	half Alpha; 
};
struct Input {
	float3 worldNormal;
	float3 worldRefl;
	float3 viewDir;
INTERNAL_DATA
};


half4 LightingCLUnlit (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
half3 lightColor = _LightColor0.rgb;
	half4 c;
	c.rgb = s.Albedo;
	c.a = s.Alpha;
	
	return c;
}

void vert (inout appdata_full v, out Input o) {
	UNITY_INITIALIZE_OUTPUT(Input, o);

float4 Vertex = v.vertex;
//Vertex

v.vertex.rgb = Vertex;
}

void frag_surf (Input IN, inout CSurfaceOutput o) {
	o.Albedo = float3(0.8,0.8,0.8);
	o.Emission = 0.0;
	o.Specular = 0.3*2;
	o.Alpha = 1.0;
	o.Gloss = float3(1.0,1.0,1.0);

float Mask0 = 0;
//Mask0
//Normals
//Diffuse
float4 Texture_Sample2 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(-0.2071429, 0, 0))),5.357142));
float4 Texture_Sample3 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(-0.1380952, 0, 0))),5.357142));
float4 Texture_Sample4 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(-0.06904762, 0, 0))),5.357142));
float4 Texture_Sample5 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0.06904762, 0, 0))),5.357142));
float4 Texture_Sample6 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0.1380952, 0, 0))),5.357142));
float4 Texture_Sample7 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0.2071429, 0, 0))),5.357142));
float4 Texture_Sample8 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0, -0.2071429, 0))),5.357142));
float4 Texture_Sample9 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0, -0.1380952, 0))),5.357142));
float4 Texture_Sample10 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0, -0.06904762, 0))),5.357142));
float4 Texture_Sample11 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0, 0.06904762, 0))),5.357142));
float4 Texture_Sample12 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0, 0.1380952, 0))),5.357142));
float4 Texture_Sample13 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)) + float3(0, 0.2071429, 0))),5.357142));
float4 Texture_Sample1 = texCUBElod(_Cube,float4((((WorldNormalVector(IN, o.Normal)))),5.357142));
Texture_Sample1.rgb = (((Texture_Sample1.rgb)+(Texture_Sample2)+(Texture_Sample3)+(Texture_Sample4)+(Texture_Sample5)+(Texture_Sample6)+(Texture_Sample7)+(Texture_Sample8)+(Texture_Sample9)+(Texture_Sample10)+(Texture_Sample11)+(Texture_Sample12)+(Texture_Sample13))/13);
//o.Albedo= Texture_Sample1.rgb;
o.Albedo = texCUBE(_Cube,(((WorldNormalVector(IN, o.Normal)))));
//o.Albedo = Unity_GlossyEnvironment (UNITY_PASS_TEXCUBE(unity_SpecCube0), data.probeHDR[0], worldNormal0, 1-oneMinusRoughness);
}
	ENDCG
}

Fallback "VertexLit"
}

/*
BeginShaderParse
0.9
BeginShaderBase
ShaderName #! Hidden/SSCubemapSkybox #?ShaderName
Hard Mode #! True #?Hard Mode
Tech Lod #! 200 #?Tech Lod
Cull #! 2 #?Cull
Tech Shader Target #! 1 #?Tech Shader Target
Vertex Recalculation #! False #?Vertex Recalculation
Use Fog #! True #?Use Fog
Use Ambient #! True #?Use Ambient
Use Vertex Lights #! True #?Use Vertex Lights
Use Lightmaps #! True #?Use Lightmaps
Diffuse On #! True #?Diffuse On
Lighting Type #! 3 #?Lighting Type
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
Transparency Type #! 1 #?Transparency Type
ZWrite #! True #?ZWrite
Transparency #! 1 #?Transparency
Shells On #! False #?Shells On
Shell Count #! 1 #?Shell Count
Shells Distance #! 0 #?Shells Distance
Shell Ease #! 0 #?Shell Ease
Shell Transparency Type #! 0 #?Shell Transparency Type
Shell Transparency ZWrite #! False #?Shell Transparency ZWrite
Shell Cull #! 0 #?Shell Cull
Shells Transparency #! 1 #?Shells Transparency
Shell Lighting #! False #?Shell Lighting
Shell Front #! False #?Shell Front
Parallax On #! False #?Parallax On
Parallax Height #! 0.1 #?Parallax Height
Parallax Quality #! 10 #?Parallax Quality
Silhouette Clipping #! False #?Silhouette Clipping
BeginShaderInput
Type #! 2 #?Type
VisName #! Texture - Cubemap #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #! 959fc31ee74485840a38a7aea9037744 #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 4 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
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
Layer Type #! 4 #?Layer Type
Main Color #! 0.8,0.8,0.8,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #! 959fc31ee74485840a38a7aea9037744 #^ 0 #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 3 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 1 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEComplexBlur #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Blur #! 0.2071429 #?Blur
Blur Y #! 0.2071429 #?Blur Y
Fast #! True #?Fast
Quality #! 3 #?Quality
EndShaderEffect
BeginShaderEffect
TypeS #! SSESimpleBlur #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Blur #! 5.357142 #?Blur
EndShaderEffect
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
EndTag #! rgb #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellEmission #?LayerListUniqueName
LayerListName #! Emission #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
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
