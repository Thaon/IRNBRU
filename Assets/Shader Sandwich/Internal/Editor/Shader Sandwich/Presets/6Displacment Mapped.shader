Shader "Hidden/SS/DisplacmentMapped" {
Properties {
_MainTex ("Base", 2D) = "white" {}
_Color ("Color", Color) = (1,1,1,1)
_SpecColor ("Specular Color", Color) = (0.1029412,0.1029412,0.1029412,1)
_Shininess ("Hardness", Range(0.000100000,1.000000000)) = 0.692598800
_ParallaxMap ("Height Map", 2D) = "white" {}
_Parallax ("Parallax Height", Range(0.000000000,0.400000000)) = 0.084297520
_SSSMask0_Copy_Copy_Copy_aPower ("Mask0 Copy Copy Copy - Power", Float) = 35.810000000
}

SubShader {
Tags { "RenderType"="Opaque" }
LOD 200
	ZWrite On
	cull Back
	CGPROGRAM

sampler2D _MainTex;
float4 _Color;
float _Shininess;
sampler2D _ParallaxMap;
float _Parallax;
float _SSSMask0_Copy_Copy_Copy_aPower;
	#pragma surface frag_surf CLPBR_Standard vertex:vert  addshadow  fullforwardshadows
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
	float2 uv_ParallaxMap;
float2 Texcoord;
};







half4 LightingCLPBR_Standard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
half3 lightColor = _LightColor0.rgb;
	s.Normal = normalize(s.Normal);
	half NdotL = max (0, dot (s.Normal, lightDir));
	half4 c;
	c.rgb = s.Albedo * lightColor * (NdotL * atten * 2);
	c.a = s.Alpha;
	float3 Spec;
	half3 h = normalize (lightDir + viewDir);	
	float nh = max (0, dot (s.Normal, h));
	Spec = pow (nh, s.Smoothness*128.0) * s.Specular;
	Spec = Spec * atten * 2 * lightColor.rgb;
	Spec = Spec * float4(0.8, 0.8, 0.8, 1).rgb;
	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/(28.26))/9.0f);
	c.rgb+=Spec;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc"
half4 LightingCLPBR_Standard (CSurfaceOutput s, half3 viewDir, UnityGI gi){
	s.Normal = normalize(s.Normal);
	// energy conservation
	half oneMinusReflectivity;
	s.Albedo = EnergyConservationBetweenDiffuseAndSpecular (s.Albedo, s.Specular, /*out*/ oneMinusReflectivity);
	// shader relies on pre-multiply alpha-blend (_SrcBlend = One, _DstBlend = OneMinusSrcAlpha)
	// this is necessary to handle transparency in physically correct way - only diffuse component gets affected by alpha
	half outputAlpha;
	s.Albedo = PreMultiplyAlpha (s.Albedo, s.Alpha, oneMinusReflectivity, /*out*/ outputAlpha);
	half4 c = UNITY_BRDF_PBS (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, gi.light, gi.indirect);
	c.rgb += UNITY_BRDF_GI (s.Albedo, s.Specular, oneMinusReflectivity, s.Smoothness, s.Normal, viewDir, s.Occlusion, gi);
	c.a = outputAlpha;
	return c;
}

inline void LightingCLPBR_Standard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
	gi = UnityGlobalIllumination (data, 1.0, s.Smoothness, s.Normal);
}
#endif

void vert (inout appdata_full v, out Input o) {
	UNITY_INITIALIZE_OUTPUT(Input, o);
	o.Texcoord = v.texcoord;
float SSShellDepth = 0;

float4 Vertex = v.vertex;
float Mask0 = 0;
//Edge Fix
float4 Mask0_Sample1 = float4((((float3(v.texcoord.xyz.xy,0).xyz))),1);
Mask0_Sample1.rgb = pow(Mask0_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= Mask0_Sample1.r;
float4 Mask0_Copy_Sample1 = float4((((float3(v.texcoord.xyz.xy,0).xyz))),1);
Mask0_Copy_Sample1 = Mask0_Copy_Sample1.ggba;
Mask0_Copy_Sample1.rgb = pow(Mask0_Copy_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= max(Mask0,Mask0_Copy_Sample1.r);
float4 Mask0_Copy_Copy_Sample1 = float4((float3(((float3(v.texcoord.xyz.xy,0).xyz)).x,1-((float3(v.texcoord.xyz.xy,0).xyz)).y,((float3(v.texcoord.xyz.xy,0).xyz)).z)),1);
Mask0_Copy_Copy_Sample1 = Mask0_Copy_Copy_Sample1.ggba;
Mask0_Copy_Copy_Sample1.rgb = pow(Mask0_Copy_Copy_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= max(Mask0,Mask0_Copy_Copy_Sample1.r);
float4 Mask0_Copy_Copy_Copy_Sample1 = float4((float3(1-((float3(v.texcoord.xyz.xy,0).xyz)).x,((float3(v.texcoord.xyz.xy,0).xyz)).y,((float3(v.texcoord.xyz.xy,0).xyz)).z)),1);
Mask0_Copy_Copy_Copy_Sample1 = Mask0_Copy_Copy_Copy_Sample1.rgba;
Mask0_Copy_Copy_Copy_Sample1.rgb = pow(Mask0_Copy_Copy_Copy_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= max(Mask0,Mask0_Copy_Copy_Copy_Sample1.r);
float4 Mask02_Sample1 = float4(Mask0,Mask0,Mask0,0);
Mask02_Sample1.rgb = clamp(Mask02_Sample1.rgb,0,1);
Mask0= Mask02_Sample1.r;
//Vertex
float4 Normal_Map_Copy_Copy_Sample1 = tex2Dlod(_ParallaxMap,float4((((v.texcoord.xyz.xy))),0,1.785714));
Normal_Map_Copy_Copy_Sample1.rgb = (float3(1,1,1)-Normal_Map_Copy_Copy_Sample1.rgb);
Vertex-= ((Normal_Map_Copy_Copy_Sample1)*float4(v.normal.rgb,1)).rgba*_Parallax;

v.vertex.rgb = Vertex;
}

void frag_surf (Input IN, inout CSurfaceOutput o) {
float SSShellDepth = 1-0;
float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	float2 uv_ParallaxMap = IN.uv_ParallaxMap;
	o.Albedo = float3(0.8,0.8,0.8);
	float4 Emission = float4(0,0,0,0);
	o.Smoothness = _Shininess;
	o.Normal = float3(0,0,1);
	o.Alpha = 1.0;
	o.Occlusion = 1.0;
	o.Specular = float3(0.3,0.3,0.3);
float4 MultiUse1 = float4((((float3(IN.Texcoord.xy,0).xyz))),1);//2
float4 MultiUse2 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy))));//3

float Mask0 = 0;
//Edge Fix
float4 Mask0_Sample1 = MultiUse1;
Mask0_Sample1.rgb = pow(Mask0_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= Mask0_Sample1.r;
float4 Mask0_Copy_Sample1 = MultiUse1;
Mask0_Copy_Sample1 = Mask0_Copy_Sample1.ggba;
Mask0_Copy_Sample1.rgb = pow(Mask0_Copy_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= max(Mask0,Mask0_Copy_Sample1.r);
float4 Mask0_Copy_Copy_Sample1 = float4((float3(((float3(IN.Texcoord.xy,0).xyz)).x,1-((float3(IN.Texcoord.xy,0).xyz)).y,((float3(IN.Texcoord.xy,0).xyz)).z)),1);
Mask0_Copy_Copy_Sample1 = Mask0_Copy_Copy_Sample1.ggba;
Mask0_Copy_Copy_Sample1.rgb = pow(Mask0_Copy_Copy_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= max(Mask0,Mask0_Copy_Copy_Sample1.r);
float4 Mask0_Copy_Copy_Copy_Sample1 = float4((float3(1-((float3(IN.Texcoord.xy,0).xyz)).x,((float3(IN.Texcoord.xy,0).xyz)).y,((float3(IN.Texcoord.xy,0).xyz)).z)),1);
Mask0_Copy_Copy_Copy_Sample1 = Mask0_Copy_Copy_Copy_Sample1.rgba;
Mask0_Copy_Copy_Copy_Sample1.rgb = pow(Mask0_Copy_Copy_Copy_Sample1.rgb,_SSSMask0_Copy_Copy_Copy_aPower);
Mask0= max(Mask0,Mask0_Copy_Copy_Copy_Sample1.r);
float4 Mask02_Sample1 = float4(Mask0,Mask0,Mask0,0);
Mask02_Sample1.rgb = clamp(Mask02_Sample1.rgb,0,1);
Mask0= Mask02_Sample1.r;
//Normals
float4 Normal_Map_Sample2 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy) + float2(0.02, 0))));
float4 Normal_Map_Sample3 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy) + float2(0, 0.02))));
float4 Normal_Map_Sample1 = MultiUse2;
Normal_Map_Sample1 = (float4(((Normal_Map_Sample1.r-Normal_Map_Sample2.r)*0.5714285),((Normal_Map_Sample1.r-Normal_Map_Sample3.r)*0.5714285),sqrt(1-((Normal_Map_Sample1.r-Normal_Map_Sample2.r)*0.5714285)*((Normal_Map_Sample1.r-Normal_Map_Sample2.r)*0.5714285)-((Normal_Map_Sample1.r-Normal_Map_Sample3.r)*0.5714285)*((Normal_Map_Sample1.r-Normal_Map_Sample3.r)*0.5714285)),Normal_Map_Sample1.a));
o.Normal= Normal_Map_Sample1.rgb;
//Diffuse
float4 Texture_2_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));
o.Albedo= Texture_2_Sample1.rgb;
float4 Texture2_Sample1 = _Color;
o.Albedo*= Texture2_Sample1.rgb;
float4 Normal_Map_Copy_Sample1 = MultiUse2;
o.Albedo= lerp(o.Albedo,o.Albedo*Normal_Map_Copy_Sample1.rgb,0.2644628);
//Gloss
float4 Specular_Sample1 = _SpecColor;
o.Specular= Specular_Sample1.rgb;
float4 Normal_Map_Copy_Copy_2_Sample1 = MultiUse2;
o.Specular= lerp(o.Specular,o.Specular*Normal_Map_Copy_Copy_2_Sample1.rgb,0.2644628);
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
BeginShaderInput
Type #! 1 #?Type
VisName #! Specular Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.1029412,0.1029412,0.1029412,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 5 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Hardness #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.6925988 #?Number
Range0 #! 0.0001 #?Range0
Range1 #! 1 #?Range1
MainType #! 6 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 0 #?Type
VisName #! Height Map #?VisName
ImageDefault #! 0 #?ImageDefault
Image #! 44b74e71e8352a24a80fd896ab2aef69 #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 9 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Parallax Height #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.08429752 #?Number
Range0 #! 0 #?Range0
Range1 #! 0.4 #?Range1
MainType #! 8 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask0 Copy Copy Copy - Power #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 35.81 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
ShaderName #! SS/DisplacmentMapped #?ShaderName
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
Lighting Type #! 4 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #?Color
Setting1 #! 0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #?Wrap Color
Specular On #! True #?Specular On
Specular Type #! 0 #?Specular Type
Spec Hardness #! 0.6925988 #^ 3 #?Spec Hardness
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
Parallax Height #! 0.08429752 #^ 5 #?Parallax Height
Parallax Quality #! 28 #?Parallax Quality
Silhouette Clipping #! True #?Silhouette Clipping
BeginShaderLayerList
LayerListUniqueName #! Mask0 #?LayerListUniqueName
LayerListName #! Edge Fix #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! Mask0 #?Layer Name
Layer Type #! 7 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathPow #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Power #! 35.81 #^ 6 #?Power
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask0 Copy #?Layer Name
Layer Type #! 7 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 5 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSESwizzle #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 1 #?UseAlpha
Channel R #! 1 #?Channel R
Channel G #! 1 #?Channel G
Channel B #! 2 #?Channel B
Channel A #! 3 #?Channel A
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathPow #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Power #! 35.81 #^ 6 #?Power
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask0 Copy Copy #?Layer Name
Layer Type #! 7 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 5 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSESwizzle #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 1 #?UseAlpha
Channel R #! 1 #?Channel R
Channel G #! 1 #?Channel G
Channel B #! 2 #?Channel B
Channel A #! 3 #?Channel A
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathPow #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Power #! 35.81 #^ 6 #?Power
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVFlip #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Flip #! False #?X Flip
Y Flip #! True #?Y Flip
Z Flip #! False #?Z Flip
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask0 Copy Copy Copy #?Layer Name
Layer Type #! 7 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 5 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSESwizzle #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 1 #?UseAlpha
Channel R #! 0 #?Channel R
Channel G #! 1 #?Channel G
Channel B #! 2 #?Channel B
Channel A #! 3 #?Channel A
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathPow #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Power #! 35.81 #^ 6 #?Power
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVFlip #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Flip #! True #?X Flip
Y Flip #! False #?Y Flip
Z Flip #! False #?Z Flip
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask02 #?Layer Name
Layer Type #! 6 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathClamp #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Min #! 0 #?Min
Max #! 1 #?Max
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Diffuse #?LayerListUniqueName
LayerListName #! Diffuse #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgb #?EndTag
BeginShaderLayer
Layer Name #! Texture 2 #?Layer Name
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
BeginShaderLayer
Layer Name #! Normal Map Copy #?Layer Name
Layer Type #! 3 #?Layer Type
Main Color #! 0,0,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #^ 4 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 0.2644628 #?Mix Amount
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
BeginShaderLayer
Layer Name #! Specular #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0.1029412,0.1029412,0.1029412,1 #^ 2 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
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
Layer Name #! Normal Map Copy Copy 2 #?Layer Name
Layer Type #! 3 #?Layer Type
Main Color #! 0,0,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #! 44b74e71e8352a24a80fd896ab2aef69   #^ 4 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 0.2644628 #?Mix Amount
Mix Type #! 3 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
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
BeginShaderLayer
Layer Name #! Normal Map #?Layer Name
Layer Type #! 3 #?Layer Type
Main Color #! 0,0,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #! 44b74e71e8352a24a80fd896ab2aef69  #^ 4 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSENormalMap #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 1 #?UseAlpha
Size #! 0.02 #?Size
Height #! 0.5714285 #?Height
Channel #! 0 #?Channel
EndShaderEffect
EndShaderLayer
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
EndTag #! r #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Vertex #?LayerListUniqueName
LayerListName #! Vertex #?LayerListName
Is Mask #! False #?Is Mask
EndTag #! rgba #?EndTag
BeginShaderLayer
Layer Name #! Normal Map Copy Copy #?Layer Name
Layer Type #! 3 #?Layer Type
Main Color #! 0,0,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #! 44b74e71e8352a24a80fd896ab2aef69  #^ 4 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 0.08429752 #^ 5 #?Mix Amount
Mix Type #! 2 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 1 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEInvert #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
EndShaderEffect
BeginShaderEffect
TypeS #! SSESimpleBlur #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Blur #! 1.785714 #?Blur
EndShaderEffect
EndShaderLayer
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
