Shader "Hidden/SS/ParallaxOcclusionMapped" {//The Shaders Name
//The inputs shown in the material panel
Properties {
	[HideInInspector]Texcoord ("Generic UV Coords (You shouldn't be seeing this aaaaah!)", 2D) = "white" {}
	_MainTex ("Base", 2D) = "white" {}
	_Color ("Color", Color) = (1,1,1,1)
	_SpecColor ("Specular Color", Color) = (0.1029412,0.1029412,0.1029412,1)
	_Shininess ("Hardness", Range(0.000100000,1.000000000)) = 0.692598800
	_ParallaxMap ("Height Map", 2D) = "white" {}
	_Parallax ("Parallax Height", Range(0.000000000,0.400000000)) = 0.135361200
	_SSSDepth_Color ("Depth Color", Color) = (0,0,0,0.5764706)
}

SubShader {
	Tags { "RenderType"="Opaque" }//A bunch of settings telling Unity a bit about the shader.
	LOD 200
	ZWrite On
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float4 _Color;
	float _Shininess;
	sampler2D _ParallaxMap;
	float _Parallax;
	float4 _SSSDepth_Color;
 //Set up Unity Surface Shader Settings.
//Set up some Parallax Occlusion Mapping Settings
#define LINEAR_SEARCH 14
#define BINARY_SEARCH 28
	#pragma surface frag_surf CLPBR_Standard addshadow  fullforwardshadows
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
		float2 uv_ParallaxMap;
		float2 uvTexcoord;
	};

//Generate simpler lighting code:
half4 LightingCLPBR_Standard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 lightColor = _LightColor0.rgb;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = s.Albedo * lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.
	float3 Spec;
	half3 h = normalize (lightDir + viewDir);	
	float nh = max (0, dot (s.Normal, h));
	Spec = pow (nh, s.Smoothness*128.0) * s.Specular;
	Spec = Spec * atten * 2 * lightColor.rgb;
	Spec = Spec * ((((s.Smoothness*128.0f)+9.0f)/(28.26))/9.0f);
	c.rgb+=Spec;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code similar to the Unity Standard Shader. Not gonna deny, I have no clue how much of it works.
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


//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	float2 uv_ParallaxMap = IN.uv_ParallaxMap;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = _Shininess;
		o.Normal = float3(0,0,1);
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);
IN.viewDir = normalize(IN.viewDir);
	float3 view = IN.viewDir*(-1*_Parallax);

		float size = 1.0/LINEAR_SEARCH; // stepping size
		float depth = 0;//pos
		int i;
		float Height = 1;
		for(i = 0; i < LINEAR_SEARCH-1; i++)// search until it steps over (Front to back)
		{
	//Generate layers for the Parallax channel.
		//Generate Layer: Normal Map Copy
			//Sample parts of the layer:
				half4 Normal_Map_Copy_Sample1 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy)))+((view*(depth)).xy));

			//Set the channel to the new color
				Height = Normal_Map_Copy_Sample1.r;


			
			if(depth < (1-Height))
				depth += size;				
		}
		//depth = best_depth;
		for(i = 0; i < BINARY_SEARCH; i++) // look around for a closer match
		{
			size*=0.5;
			
	//Generate layers for the Parallax channel.
		//Generate Layer: Normal Map Copy
			//Sample parts of the layer:
				half4 Normal_Map_Copy_Sample1 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy)))+((view*(depth)).xy));

			//Set the channel to the new color
				Height = Normal_Map_Copy_Sample1.r;


			
			if(depth < (1-Height))
				depth += (2*size);
			
			depth -= size;			
		}
		
SSParallaxDepth = depth;
	uv_MainTex.xy += view.xy*depth;
	uv_ParallaxMap.xy += view.xy*depth;
IN.uvTexcoord.xy += view.xy*depth;
	clip(IN.uvTexcoord.x);
	clip(IN.uvTexcoord.y);
	clip(-(IN.uvTexcoord.x-1));
	clip(-(IN.uvTexcoord.y-1));

	//Set default mask color
		float Mask0 = 0;
	//Generate layers for the Normals channel.
		//Generate Layer: Normal Map
			//Sample parts of the layer:
				half4 Normal_Map_Sample2 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy)) + float2(0.02, 0)));
				half4 Normal_Map_Sample3 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy)) + float2(0, 0.02)));
				half4 Normal_Map_Sample1 = tex2D(_ParallaxMap,(((uv_ParallaxMap.xy))));

			//Apply Effects:
				Normal_Map_Sample1 = (float4(((Normal_Map_Sample1.r-Normal_Map_Sample2.r)*0.5714285),((Normal_Map_Sample1.r-Normal_Map_Sample3.r)*0.5714285),1,Normal_Map_Sample1.a));

			//Set the channel to the new color
				o.Normal = Normal_Map_Sample1.rgb;

	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture 2
			//Sample parts of the layer:
				half4 Texture_2_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_2_Sample1.rgb;

		//Generate Layer: Texture2
			//Sample parts of the layer:
				half4 Texture2_Sample1 = _Color;

			//Blend the layer into the channel using the Multiply blend mode
				o.Albedo *= Texture2_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 Texture_Sample1 = _SSSDepth_Color;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,Texture_Sample1.rgb,Texture_Sample1.a*SSParallaxDepth);

	//Generate layers for the Gloss channel.
		//Generate Layer: Specular
			//Sample parts of the layer:
				half4 Specular_Sample1 = _SpecColor;

			//Set the channel to the new color
				o.Specular = Specular_Sample1.rgb;

		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_Copy_Sample1 = float4(0, 0, 0, 1);

			//Blend the layer into the channel using the Mix blend mode
				o.Specular = lerp(o.Specular,Texture_Copy_Sample1.rgb,SSParallaxDepth);

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
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 2 #?MainType
SpecialType #! 0 #?SpecialType
InEditor #! 1 #?InEditor
NormalMap #! 0 #?NormalMap
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
InEditor #! 1 #?InEditor
NormalMap #! 0 #?NormalMap
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
InEditor #! 1 #?InEditor
NormalMap #! 0 #?NormalMap
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
InEditor #! 1 #?InEditor
NormalMap #! 0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 0 #?Type
VisName #! Height Map #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 9 #?MainType
SpecialType #! 0 #?SpecialType
InEditor #! 1 #?InEditor
NormalMap #! 0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Parallax Height #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.1353612 #?Number
Range0 #! 0 #?Range0
Range1 #! 0.4 #?Range1
MainType #! 8 #?MainType
SpecialType #! 0 #?SpecialType
InEditor #! 1 #?InEditor
NormalMap #! 0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Depth Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0,0,0,0.5764706 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
InEditor #! 1 #?InEditor
NormalMap #! 0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Depth #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 1 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 12 #?SpecialType
InEditor #! 0 #?InEditor
NormalMap #! 0 #?NormalMap
EndShaderInput
ShaderName #! SS/ParallaxOcclusionMapped #?ShaderName
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
Forward Add #! True #?Forward Add
Shadows #! True #?Shadows
Interpolate View #! False #?Interpolate View
Half as View #! False #?Half as View
Diffuse On #! True #?Diffuse On
Lighting Type #! 4 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #?Color
Setting1 #! 0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #?Wrap Color
Use Normals #! 0 #?Use Normals
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
Parallax On #! True #?Parallax On
Parallax Height #! 0.1353612 #^ 5 #?Parallax Height
Parallax Quality #! 28 #?Parallax Quality
Silhouette Clipping #! True #?Silhouette Clipping
Tessellation On #! False #?Tessellation On
Tessellation Type #! 2 #?Tessellation Type
Tessellation Quality #! 10 #?Tessellation Quality
Tessellation Falloff #! 1 #?Tessellation Falloff
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
Layer Name #! Texture 2 #?Layer Name
Layer Type #! 3 #?Layer Type
Main Color #! 0.8,0.8,0.8,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #^ 0 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
UV Map #! 0 #?UV Map
Map Local #! False #?Map Local
Use Alpha #! False #?Use Alpha
Mix Amount #! 1 #?Mix Amount
Use Fadeout #! False #?Use Fadeout
Fadeout Limit Min #! 0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #?Fadeout Limit Max
Fadeout Start #! 3 #?Fadeout Start
Fadeout End #! 5 #?Fadeout End
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
UV Map #! 0 #?UV Map
Map Local #! False #?Map Local
Use Alpha #! False #?Use Alpha
Mix Amount #! 1 #?Mix Amount
Use Fadeout #! False #?Use Fadeout
Fadeout Limit Min #! 0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #?Fadeout Limit Max
Fadeout Start #! 3 #?Fadeout Start
Fadeout End #! 5 #?Fadeout End
Mix Type #! 3 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Texture #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0,0,0,0.5764706 #^ 6 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
UV Map #! 0 #?UV Map
Map Local #! False #?Map Local
Use Alpha #! True #?Use Alpha
Mix Amount #! 1 #^ 7 #?Mix Amount
Use Fadeout #! False #?Use Fadeout
Fadeout Limit Min #! 0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #?Fadeout Limit Max
Fadeout Start #! 3 #?Fadeout Start
Fadeout End #! 5 #?Fadeout End
Mix Type #! 0 #?Mix Type
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
UV Map #! 0 #?UV Map
Map Local #! False #?Map Local
Use Alpha #! False #?Use Alpha
Mix Amount #! 1 #?Mix Amount
Use Fadeout #! False #?Use Fadeout
Fadeout Limit Min #! 0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #?Fadeout Limit Max
Fadeout Start #! 3 #?Fadeout Start
Fadeout End #! 5 #?Fadeout End
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Texture Copy #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0,0,0,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
UV Map #! 0 #?UV Map
Map Local #! False #?Map Local
Use Alpha #! False #?Use Alpha
Mix Amount #! 1 #^ 7 #?Mix Amount
Use Fadeout #! False #?Use Fadeout
Fadeout Limit Min #! 0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #?Fadeout Limit Max
Fadeout Start #! 3 #?Fadeout Start
Fadeout End #! 5 #?Fadeout End
Mix Type #! 0 #?Mix Type
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
Main Texture #! 44b74e71e8352a24a80fd896ab2aef69   #^ 4 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
UV Map #! 0 #?UV Map
Map Local #! False #?Map Local
Use Alpha #! False #?Use Alpha
Mix Amount #! 1 #?Mix Amount
Use Fadeout #! False #?Use Fadeout
Fadeout Limit Min #! 0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #?Fadeout Limit Max
Fadeout Start #! 3 #?Fadeout Start
Fadeout End #! 5 #?Fadeout End
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
BeginShaderLayer
Layer Name #! Normal Map Copy #?Layer Name
Layer Type #! 3 #?Layer Type
Main Color #! 0,0,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #! 44b74e71e8352a24a80fd896ab2aef69   #^ 4 #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
UV Map #! 0 #?UV Map
Map Local #! False #?Map Local
Use Alpha #! False #?Use Alpha
Mix Amount #! 1 #?Mix Amount
Use Fadeout #! False #?Use Fadeout
Fadeout Limit Min #! 0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #?Fadeout Limit Max
Fadeout Start #! 3 #?Fadeout Start
Fadeout End #! 5 #?Fadeout End
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
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
