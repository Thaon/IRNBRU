Shader "Shader Sandwich/Specific/Fur" {//The Shaders Name
//The inputs shown in the material panel
Properties {
	_MainTex ("Texture - Main Texture", 2D) = "white" {}
	_ShellDistance ("Fur Distance", Range(0.000000000,0.500000000)) = 0.100000000
	_SSSDepth_Darkening_AO ("Depth Darkening (AO)", Color) = (0,0,0,0.5019608)
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
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0;
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture 2
			//Sample parts of the layer:
				half4 Texture_2Diffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_2Diffuse_Sample1.rgb;

		//Generate Layer: Texture Copy 2 2
			//Sample parts of the layer:
				half4 Texture_Copy_2_2Diffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,Texture_Copy_2_2Diffuse_Sample1.rgb,Texture_Copy_2_2Diffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.1111111;
	v.vertex.xyz += v.normal*(_ShellDistance*0.1111111);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.1111111;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.2222222;
	v.vertex.xyz += v.normal*(_ShellDistance*0.2222222);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.2222222;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.3333333;
	v.vertex.xyz += v.normal*(_ShellDistance*0.3333333);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.3333333;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.4444444;
	v.vertex.xyz += v.normal*(_ShellDistance*0.4444444);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.4444444;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.5555556;
	v.vertex.xyz += v.normal*(_ShellDistance*0.5555556);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.5555556;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.6666667;
	v.vertex.xyz += v.normal*(_ShellDistance*0.6666667);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.6666667;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.7777778;
	v.vertex.xyz += v.normal*(_ShellDistance*0.7777778);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.7777778;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 0.8888889;
	v.vertex.xyz += v.normal*(_ShellDistance*0.8888889);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-0.8888889;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
	ZWrite On
	ZWrite On
	ZWrite Off
	cull Back//Culling specifies which sides of the models faces to hide.
	blend off //Disabled blending (No Transparency)
	CGPROGRAM

//Make our inputs accessible by declaring them here.
	sampler2D _MainTex;
	float _ShellDistance;
	float4 _SSSDepth_Darkening_AO;
 //Set up Unity Surface Shader Settings.
	#pragma surface frag_surf CLStandard vertex:vert  addshadow  alpha:fade fullforwardshadows
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
half4 LightingCLStandard (CSurfaceOutput s, half3 lightDir, half3 viewDir, half atten) {
	half3 SSlightColor = _LightColor0.rgb;
	half3 lightColor = _LightColor0.rgb;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor * atten * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;
	
	return c;
}
#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
#include "UnityPBSLighting.cginc" //Include some PBS stuff.
//Generate lighting code for each GI part:
half4 LightingCLStandardLight (CSurfaceOutput s, half3 viewDir, UnityLight light) {
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = light.dir;
	half3 atten = light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;
	half NdotL = max (0, dot (s.Normal, lightDir)); //Calculate the dot of the faces normal and the lights direction. This means a lower number the further the angle of the face is from the light source.
	half4 c;
	c.rgb = lightColor  * NdotL; //Output the final RGB color by multiplying the surfaces color with the light color, then by the distance from the light (or some function of it), and finally by the Dot of the normal and the light direction.
	c.a = s.Alpha; //Set the output alpha to the surface Alpha.


c.rgb = c.rgb*s.Albedo;

	return c;
}
//Generate some other Lighting code. It calls the previous lighting code a few times for different lights depending on lightmapping modes and other things.
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
	half3 SSlightColor = _LightColor0;
	half3 lightColor = _LightColor0;
	half3 lightDir = gi.light.dir;
	half3 atten = gi.light.color/_LightColor0;
	half3 SSnormal = s.Normal;
	half3 SSalbedo = s.Albedo;
	half3 SSspecular = s.Specular;
	half3 SSemission = s.Emission;
	half SSalpha = s.Alpha;

	#ifdef UNITY_LIGHT_FUNCTION_APPLY_INDIRECT

		c.rgb += s.Albedo * gi.indirect.diffuse;
	#endif
	return c;
}

//Some weird Unity stuff for GI calculation (I think?).
inline void LightingCLStandard_GI (CSurfaceOutput s,UnityGIInput data,inout UnityGI gi){
#if UNITY_VERSION >= 520
	UNITY_GI(gi, s, data);
#else
	gi = UnityGlobalIllumination (data, 1.0, 0.0, s.Normal,false);
#endif

}
#endif
//Create a struct for the inputs of the vertex shader which includes whatever Shader Sandwich might need.
	struct appdata_min {
	float4 vertex : POSITION;
	float4 tangent : TANGENT;
	float3 normal : NORMAL;
	float4 texcoord : TEXCOORD0;
	float4 texcoord1 : TEXCOORD1;
	#ifdef UNITY_GLOBAL_ILLUMINATION_INCLUDED
	#ifndef U4Imposter
	float4 texcoord2 : TEXCOORD2;
	#endif
	#endif
	fixed4 color : COLOR;
};
//Generate the vertex shader
void vert (inout appdata_min v) {
	float SSShellDepth = 1;
	v.vertex.xyz += v.normal*(_ShellDistance*1);
}

//Generate the fragment shader (Operates on pixels)
void frag_surf (Input IN, inout CSurfaceOutput o) {
	float SSShellDepth = 1-1;
	float SSParallaxDepth = 0;
	float2 uv_MainTex = IN.uv_MainTex;
	//Set reasonable defaults for the fragment outputs.
		o.Albedo = float3(0.8,0.8,0.8);
		float4 Emission = float4(0,0,0,0);
		o.Smoothness = 0;
		o.Alpha = 1.0;
		o.Occlusion = 1.0;
		o.Specular = float3(0.3,0.3,0.3);

	//Generate layers for the Alpha channel.
		//Generate Layer: Texture Copy
			//Sample parts of the layer:
				half4 Texture_CopyShellAlpha_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Alpha = Texture_CopyShellAlpha_Sample1.a;

	o.Alpha *= 1;
	//Generate layers for the Diffuse channel.
		//Generate Layer: Texture Copy 2
			//Sample parts of the layer:
				half4 Texture_Copy_2ShellDiffuse_Sample1 = tex2D(_MainTex,(((uv_MainTex.xy))));

			//Set the channel to the new color
				o.Albedo = Texture_Copy_2ShellDiffuse_Sample1.rgb;

		//Generate Layer: Texture
			//Sample parts of the layer:
				half4 TextureShellDiffuse_Sample1 = _SSSDepth_Darkening_AO;

			//Blend the layer into the channel using the Mix blend mode
				o.Albedo = lerp(o.Albedo,TextureShellDiffuse_Sample1.rgb,TextureShellDiffuse_Sample1.a*SSShellDepth);

}
	ENDCG
}

Fallback "VertexLit"
}

/*
BeginShaderParse
1.0
BeginShaderBase
BeginShaderInput
Type #! 0 #^ CC0 #?Type
VisName #! Texture - Main Texture #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #! 824bbecff5e03c7449eea0b94db3c76b #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 0 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 2 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Fur Distance #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 0.1 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 0.5 #^ CC0 #?Range1
MainType #! 11 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 1 #^ CC0 #?Type
VisName #! Depth Darkening (AO) #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0,0,0,0.5019608 #^ CC0 #?Color
Number #! 0 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 0 #^ CC0 #?SpecialType
InEditor #! 1 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
BeginShaderInput
Type #! 4 #^ CC0 #?Type
VisName #! Shell Dist #^ CC0 #?VisName
ImageDefault #! 0 #^ CC0 #?ImageDefault
Image #!  #^ CC0 #?Image
Cube #!  #^ CC0 #?Cube
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Number #! 1 #^ CC0 #?Number
Range0 #! 0 #^ CC0 #?Range0
Range1 #! 1 #^ CC0 #?Range1
MainType #! 0 #^ CC0 #?MainType
SpecialType #! 11 #^ CC0 #?SpecialType
InEditor #! 0 #^ CC0 #?InEditor
NormalMap #! 0 #^ CC0 #?NormalMap
EndShaderInput
ShaderName #! Shader Sandwich/Specific/Fur #^ CC0 #?ShaderName
Hard Mode #! True #^ CC0 #?Hard Mode
Tech Lod #! 200 #^ CC0 #?Tech Lod
Cull #! 1 #^ CC0 #?Cull
Tech Shader Target #! 3 #^ CC0 #?Tech Shader Target
Vertex Recalculation #! False #^ CC0 #?Vertex Recalculation
Use Fog #! True #^ CC0 #?Use Fog
Use Ambient #! True #^ CC0 #?Use Ambient
Use Vertex Lights #! True #^ CC0 #?Use Vertex Lights
Use Lightmaps #! True #^ CC0 #?Use Lightmaps
Use All Shadows #! True #^ CC0 #?Use All Shadows
Forward Add #! True #^ CC0 #?Forward Add
Shadows #! True #^ CC0 #?Shadows
Interpolate View #! False #^ CC0 #?Interpolate View
Half as View #! False #^ CC0 #?Half as View
Diffuse On #! True #^ CC0 #?Diffuse On
Lighting Type #! 0 #^ CC0 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #^ CC0 #?Color
Setting1 #! 0 #^ CC0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #^ CC0 #?Wrap Color
Use Normals #! 0 #^ CC0 #?Use Normals
Specular On #! False #^ CC0 #?Specular On
Specular Type #! 0 #^ CC0 #?Specular Type
Spec Hardness #! 0.3 #^ CC0 #?Spec Hardness
Spec Color #! 0.8,0.8,0.8,1 #^ CC0 #?Spec Color
Spec Energy Conserve #! True #^ CC0 #?Spec Energy Conserve
Spec Offset #! 0 #^ CC0 #?Spec Offset
Emission On #! False #^ CC0 #?Emission On
Emission Color #! 0,0,0,0 #^ CC0 #?Emission Color
Emission Type #! 0 #^ CC0 #?Emission Type
Transparency On #! True #^ CC0 #?Transparency On
Transparency Type #! 1 #^ CC0 #?Transparency Type
ZWrite #! False #^ CC0 #?ZWrite
Use PBR #! True #^ CC0 #?Use PBR
Transparency #! 1 #^ CC0 #?Transparency
Receive Shadows #! False #^ CC0 #?Receive Shadows
ZWrite Type #! 0 #^ CC0 #?ZWrite Type
Blend Mode #! 0 #^ CC0 #?Blend Mode
Shells On #! True #^ CC0 #?Shells On
Shell Count #! 9 #^ CC0 #?Shell Count
Shells Distance #! 0.1 #^ CC0 #^ 1 #?Shells Distance
Shell Ease #! 1 #^ CC0 #?Shell Ease
Shell Transparency Type #! 0 #^ CC0 #?Shell Transparency Type
Shell Transparency ZWrite #! False #^ CC0 #?Shell Transparency ZWrite
Shell Cull #! 1 #^ CC0 #?Shell Cull
Shells ZWrite #! True #^ CC0 #?Shells ZWrite
Shells Use Transparency #! True #^ CC0 #?Shells Use Transparency
Shell Blend Mode #! 0 #^ CC0 #?Shell Blend Mode
Shells Transparency #! 1 #^ CC0 #?Shells Transparency
Shell Lighting #! True #^ CC0 #?Shell Lighting
Shell Front #! True #^ CC0 #?Shell Front
Parallax On #! False #^ CC0 #?Parallax On
Parallax Height #! 0.1 #^ CC0 #?Parallax Height
Parallax Quality #! 10 #^ CC0 #?Parallax Quality
Silhouette Clipping #! False #^ CC0 #?Silhouette Clipping
Tessellation On #! False #^ CC0 #?Tessellation On
Tessellation Type #! 2 #^ CC0 #?Tessellation Type
Tessellation Quality #! 10 #^ CC0 #?Tessellation Quality
Tessellation Falloff #! 1 #^ CC0 #?Tessellation Falloff
Tessellation Smoothing Amount #! 0 #^ CC0 #?Tessellation Smoothing Amount
BeginShaderLayerList
LayerListUniqueName #! Mask0 #^ CC0 #?LayerListUniqueName
LayerListName #! Mask0 #^ CC0 #?LayerListName
Is Mask #! True #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! r #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Diffuse #^ CC0 #?LayerListUniqueName
LayerListName #! Diffuse #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Texture 2 #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 0.627451,0.8,0.8823529,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 824bbecff5e03c7449eea0b94db3c76b   #^ CC0 #^ 0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Texture Copy 2 2 #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 0,0,0,0.5019608 #^ CC0 #^ 2 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #!  #^ CC0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! True #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #^ 3 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellDiffuse #^ CC0 #?LayerListUniqueName
LayerListName #! Diffuse #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Texture Copy 2 #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 0.627451,0.8,0.8823529,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 824bbecff5e03c7449eea0b94db3c76b   #^ CC0 #^ 0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Texture #^ CC0 #?Layer Name
Layer Type #! 0 #^ CC0 #?Layer Type
Main Color #! 0,0,0,0.5019608 #^ CC0 #^ 2 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #!  #^ CC0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! True #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #^ 3 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Alpha #^ CC0 #?LayerListUniqueName
LayerListName #! Alpha #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellAlpha #^ CC0 #?LayerListUniqueName
LayerListName #! Alpha #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
BeginShaderLayer
Layer Name #! Texture Copy #^ CC0 #?Layer Name
Layer Type #! 3 #^ CC0 #?Layer Type
Main Color #! 0.627451,0.8,0.8823529,1 #^ CC0 #?Main Color
Second Color #! 0,0,0,1 #^ CC0 #?Second Color
Main Texture #! 824bbecff5e03c7449eea0b94db3c76b   #^ CC0 #^ 0 #?Main Texture
Cubemap #!  #^ CC0 #?Cubemap
Noise Type #! 0 #^ CC0 #?Noise Type
Noise Dimensions #! 0 #^ CC0 #?Noise Dimensions
Noise A #! 0 #^ CC0 #?Noise A
Noise B #! 1 #^ CC0 #?Noise B
Noise C #! False #^ CC0 #?Noise C
Light Data #! 0 #^ CC0 #?Light Data
Special Type #! 0 #^ CC0 #?Special Type
Linearize Depth #! False #^ CC0 #?Linearize Depth
UV Map #! 0 #^ CC0 #?UV Map
Map Local #! False #^ CC0 #?Map Local
Use Alpha #! False #^ CC0 #?Use Alpha
Mix Amount #! 1 #^ CC0 #?Mix Amount
Use Fadeout #! False #^ CC0 #?Use Fadeout
Fadeout Limit Min #! 0 #^ CC0 #?Fadeout Limit Min
Fadeout Limit Max #! 10 #^ CC0 #?Fadeout Limit Max
Fadeout Start #! 3 #^ CC0 #?Fadeout Start
Fadeout End #! 5 #^ CC0 #?Fadeout End
Mix Type #! 0 #^ CC0 #?Mix Type
Stencil #! -1 #^ CC0 #?Stencil
Vertex Mask #! 2 #^ CC0 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Specular #^ CC0 #?LayerListUniqueName
LayerListName #! Specular #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellSpecular #^ CC0 #?LayerListUniqueName
LayerListName #! Specular #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Normals #^ CC0 #?LayerListUniqueName
LayerListName #! Normals #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellNormals #^ CC0 #?LayerListUniqueName
LayerListName #! Normals #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Emission #^ CC0 #?LayerListUniqueName
LayerListName #! Emission #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellEmission #^ CC0 #?LayerListUniqueName
LayerListName #! Emission #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Height #^ CC0 #?LayerListUniqueName
LayerListName #! Height #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! a #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingDiffuse #^ CC0 #?LayerListUniqueName
LayerListName #! Diffuse #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingSpecular #^ CC0 #?LayerListUniqueName
LayerListName #! Specular #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingIndirect #^ CC0 #?LayerListUniqueName
LayerListName #! Ambient #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgb #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! LightingDirect #^ CC0 #?LayerListUniqueName
LayerListName #! Direct #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! True #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Vertex #^ CC0 #?LayerListUniqueName
LayerListName #! Vertex #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! ShellVertex #^ CC0 #?LayerListUniqueName
LayerListName #! Vertex #^ CC0 #?LayerListName
Is Mask #! False #^ CC0 #?Is Mask
Is Lighting #! False #^ CC0 #?Is Lighting
EndTag #! rgba #^ CC0 #?EndTag
EndShaderLayerList
EndShaderBase
EndShaderParse
*/