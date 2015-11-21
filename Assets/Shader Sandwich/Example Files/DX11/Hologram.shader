Shader "Shader Sandwich/Hologram" {
Properties {
_SSSFlicker_Axis ("Flicker Axis", Color) = (0.2794118,0,0.2794118,1)
_SSSForce_Flicker ("Force Flicker", Range(0.000000000,1.000000000)) = 0.000000000
_SSSPull_Height ("Pull Height", Float) = 0.530000000
_SSSPull_Axis ("Pull Axis", Color) = (0,1,0,1)
}

SubShader {
Tags { "RenderType"="Opaque""Queue"="Transparent" }
LOD 200

Pass
{
	Name "ALPHAMASK"
	ColorMask 0
	cull Back
blend off 
	CGPROGRAM

float4 _SSSFlicker_Axis;
float _SSSForce_Flicker;
float _SSSPull_Height;
float4 _SSSPull_Axis;
	#pragma target 3.0
	#pragma vertex vert
	#pragma fragment frag
	#include "UnityCG.cginc"
float4 permute(float4 x) { return fmod(((x)+1.0)*x, 289.0); }

float2 fade(float2 t) {
	return t*t*t*(t*(t*6.0-15.0)+10.0);
}

// Classic Perlin noise
float Noise2D(float2 P)
{
	float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
	float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
	Pi = fmod(Pi, 289.0); // To avoid truncation effects in permutation
	float4 ix = Pi.xzxz;
	float4 iy = Pi.yyww;
	float4 fx = Pf.xzxz;
	float4 fy = Pf.yyww;
	float4 i = permute(permute(ix) + iy);
	float4 gx = 2.0 * frac(i * 0.0243902439) - 1.0; // 1/41 = 0.024...
	float4 gy = abs(gx) - 0.5;
	float4 tx = floor(gx + 0.5);
	gx = gx - tx;
	float2 g00 = float2(gx.x,gy.x);
	float2 g10 = float2(gx.y,gy.y);
	float2 g01 = float2(gx.z,gy.z);
	float2 g11 = float2(gx.w,gy.w);
	float n00 = dot(g00, float2(fx.x, fy.x));
	float n10 = dot(g10, float2(fx.y, fy.y));
	float n01 = dot(g01, float2(fx.z, fy.z));
	float n11 = dot(g11, float2(fx.w, fy.w));
	float2 fade_xy = fade(Pf.xy);
	float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
	float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
	return 4 * n_xy;
}
float4 permute2(float4 x)
{
  return fmod(((x*34.0)+1.0)*x, 289.0);
}

float4 taylorInvSqrt(float4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

float3 fade3D(float3 t) {
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}

// Classic Perlin noise
float Noise3D(float3 P)
{
  float3 Pi0 = floor(P); // Integer part for indexing
  float3 Pi1 = Pi0 + float3(1,1,1); // Integer part + 1
  Pi0 = fmod(Pi0, 289.0);
  Pi1 = fmod(Pi1, 289.0);
  float3 Pf0 = frac(P); // fracional part for interpolation
  float3 Pf1 = Pf0 - float3(1,1,1); // fracional part - 1.0
  float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
  float4 iy = float4(Pi0.yy, Pi1.yy);
  float4 iz0 = Pi0.zzzz;
  float4 iz1 = Pi1.zzzz;

  float4 ixy = permute2(permute2(ix) + iy);
  float4 ixy0 = permute2(ixy + iz0);
  float4 ixy1 = permute2(ixy + iz1);

  float4 gx0 = ixy0 / 7.0;
  float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
  gx0 = frac(gx0);
  float4 gz0 = float4(0.5,0.5,0.5,0.5) - abs(gx0) - abs(gy0);
  float4 sz0 = step(gz0, float4(0,0,0,0));
  gx0 -= sz0 * (step(0.0, gx0) - 0.5);
  gy0 -= sz0 * (step(0.0, gy0) - 0.5);

  float4 gx1 = ixy1 / 7.0;
  float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
  gx1 = frac(gx1);
  float4 gz1 = float4(0.5,0.5,0.5,0.5) - abs(gx1) - abs(gy1);
  float4 sz1 = step(gz1, float4(0,0,0,0));
  gx1 -= sz1 * (step(0.0, gx1) - 0.5);
  gy1 -= sz1 * (step(0.0, gy1) - 0.5);

  float3 g000 = float3(gx0.x,gy0.x,gz0.x);
  float3 g100 = float3(gx0.y,gy0.y,gz0.y);
  float3 g010 = float3(gx0.z,gy0.z,gz0.z);
  float3 g110 = float3(gx0.w,gy0.w,gz0.w);
  float3 g001 = float3(gx1.x,gy1.x,gz1.x);
  float3 g101 = float3(gx1.y,gy1.y,gz1.y);
  float3 g011 = float3(gx1.z,gy1.z,gz1.z);
  float3 g111 = float3(gx1.w,gy1.w,gz1.w);

  float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
  g000 *= norm0.x;
  g010 *= norm0.y;
  g100 *= norm0.z;
  g110 *= norm0.w;
  float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
  g001 *= norm1.x;
  g011 *= norm1.y;
  g101 *= norm1.z;
  g111 *= norm1.w;

  float n000 = dot(g000, Pf0);
  float n100 = dot(g100, float3(Pf1.x, Pf0.yz));
  float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
  float n110 = dot(g110, float3(Pf1.xy, Pf0.z));
  float n001 = dot(g001, float3(Pf0.xy, Pf1.z));
  float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
  float n011 = dot(g011, float3(Pf0.x, Pf1.yz));
  float n111 = dot(g111, Pf1);

  float3 fade_xyz = fade3D(Pf0);
  float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
  float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
  float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x); 
  return 2.2 * n_xyz;
  }
float Unique1D(float t){
	//return frac(sin(floor(t.x))*43558.5453);
	return frac(sin(dot(t ,12.9898)) * 43758.5453);
	//return frac(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
	//return frac(sin(n)*43758.5453);
}
float Lerpify(float P){
	float ft = P * 3.1415927;
	float f = (1 - cos(ft)) * 0.5;
	return f;
}
float D1Lerp(float P, float Col1,float Col2){
	float ft = P * 3.1415927;
	float f = (1 - cos(ft)) * 0.5;
	return Col1+((Col2-Col1)*f);//(Col1*P)+(Col2*(1-P));
}
float Unique2D(float2 t){
	float x = frac(sin(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453);
	//float x = frac(frac(tan(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453)*7.35);
	return x;
}
float Lerp2D(float2 P, float Col1,float Col2,float Col3,float Col4){
	float2 ft = P * 3.1415927;
	float2 f = (1 - cos(ft)) * 0.5;
	P = f;
	float S1 = lerp(Col1,Col2,P.x);
	float S2 = lerp(Col3,Col4,P.x);
	float L = lerp(S1,S2,P.y);
	return L;
}
float NoiseB2D(float2 P)
{
	float SS = Unique2D(P);
	float SE = Unique2D(P+float2(1.00001,0));
	float ES = Unique2D(P+float2(0,1.00001));
	float EE = Unique2D(P+float2(1,1.00001));
	float xx = Lerp2D(frac(P),SS,SE,ES,EE);
	return xx;
}

float NoiseB1D(float P)
{
	float SS = Unique1D(P);
	float SE = Unique1D(P+1.00001);
	float xx = D1Lerp(frac(P),SS,SE);
	return xx;
}
float Unique3D(float3 t){
	float x = frac(tan(dot(tan(floor(t)),float3(12.9898,78.233,35.344))) * 9.5453);
	return x;
}

float Lerp3D(float3 P, float SSS,float SES,float ESS,float EES, float SSE,float SEE,float ESE,float EEE){
	float3 ft = P * 3.1415927;
	float3 f = P;//(1 - cos(ft)) * 0.5;
	float S1 = lerp(SSS,SES,f.x);
	float S2 = lerp(ESS,EES,f.x);
	float F1 = lerp(S1,S2,f.y);
	float S3 = lerp(SSE,SEE,f.x);
	float S4 = lerp(ESE,EEE,f.x);
	float F2 = lerp(S3,S4,f.y);
	float L = lerp(F1,F2,f.z);//F1;
	return L;
}
float NoiseB3D(float3 P)
{
	float SSS = Unique3D(P+float3(0,0,0));
	float SES = Unique3D(P+float3(1.00001,0,0));
	float ESS = Unique3D(P+float3(0,1.00001,0));
	float EES = Unique3D(P+float3(1.00001,1.00001,0));
	float SSE = Unique3D(P+float3(0,0,1.00001));
	float SEE = Unique3D(P+float3(1.00001,0,1.00001));
	float ESE = Unique3D(P+float3(0,1.00001,1.00001));
	float EEE = Unique3D(P+float3(1.00001,1.00001,1.00001));
	float xx = Lerp3D(frac(P),SSS,SES,ESS,EES,SSE,SEE,ESE,EEE);
	return xx;
}










float4 vert(appdata_base v) : POSITION {
float SSShellDepth = 0;

float4 Vertex = v.vertex;
float Mask0 = 0;
//Dust
float4 Mask0_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)));
Mask0= Mask0_Sample1.r;
float Mask1 = 0;
//Sparkles
float4 Mask0_Copy_2_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)));
Mask1= Mask0_Copy_2_Sample1.r;
float4 Mask0_Copy_Copy_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)));
;
float4 Mask0_Copy_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)));
Mask1*= Mask0_Copy_Sample1.r;
float4 Mask1_Sample1 = float4(Mask1,Mask1,Mask1,0);
Mask1_Sample1.rgb = round(Mask1_Sample1.rgb);
Mask1= Mask1_Sample1.r;
float Mask2 = 0;
//Squares
float4 Mask2_Sample1 = float4(NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)));
Mask2= Mask2_Sample1.r;
float4 Mask2_Copy_Sample1 = float4(NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)));
Mask2*= Mask2_Copy_Sample1.r;
float Mask3 = 0;
//Lines
float4 Mask3_Sample1 = float4(((((float3(v.texcoord.xyz.xy,0).xyz))+float3(_Time.y/6,0,0))),1);
Mask3_Sample1.rgb = (Mask3_Sample1.rgb*52.92);
Mask3_Sample1.rgb = ((sin(Mask3_Sample1.rgb)+1)/2);
Mask3= Mask3_Sample1.r;
float4 Mask3_Copy_Sample1 = float4(((((float3(v.texcoord.xyz.xy,0).xyz))+float3(_Time.y/6,0,0))),1);
Mask3_Copy_Sample1.rgb = (Mask3_Copy_Sample1.rgb*22.1);
Mask3_Copy_Sample1.rgb = sin(Mask3_Copy_Sample1.rgb);
Mask3*= Mask3_Copy_Sample1.r;
float4 Mask32_Sample1 = float4(Mask3,Mask3,Mask3,0);
Mask32_Sample1.rgb = clamp(Mask32_Sample1.rgb,0,1);
Mask32_Sample1.rgb = pow(Mask32_Sample1.rgb,6.44);
Mask3= Mask32_Sample1.r;
float Mask4 = 0;
//Flicker
float4 Flicker_Sample1 = float4((((((float3(v.texcoord.xyz.xy,0).xyz))*float3(0.1,0.1,0.1))+float3(_Time.y,0,0))),1);
Flicker_Sample1.rgb = sin(Flicker_Sample1.rgb);
Flicker_Sample1.rgb = (Flicker_Sample1.rgb*56.1);
Flicker_Sample1.rgb = (Flicker_Sample1.rgb-(53.54));
Flicker_Sample1.rgb = round(Flicker_Sample1.rgb);
Flicker_Sample1.rgb = clamp(Flicker_Sample1.rgb,0,1);
Mask4= Flicker_Sample1.r;
float4 Flicker2_Sample1 = float4(1, 1, 1, 1);
Mask4= lerp(Mask4,Flicker2_Sample1.r,_SSSForce_Flicker);
float Mask5 = 0;
//FlickerNoise
float4 FlickerNoise_Sample1 = float4((Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2);
Mask5= lerp(Mask5,FlickerNoise_Sample1.r,Mask4);
float Mask6 = 0;
//Pull
float4 Mask6_Sample1 = float4((((mul(_Object2World, v.vertex).xyz))),1);
Mask6_Sample1.rgb = (Mask6_Sample1.rgb+_SSSPull_Height);
Mask6_Sample1.rgb = (float3(1,1,1)-Mask6_Sample1.rgb);
Mask6_Sample1.rgb = pow(Mask6_Sample1.rgb,5.95);
Mask6_Sample1.rgb = clamp(Mask6_Sample1.rgb,0,1);
Mask6= Mask6_Sample1.g;
//Vertex
float4 Vertex3_Sample1 = _SSSFlicker_Axis;
Vertex-= ((Vertex3_Sample1)*float4(v.normal.rgb,1)).rgba*Mask5;
float4 Vertex_Sample1 = _SSSPull_Axis;
Vertex= lerp(Vertex,((Vertex_Sample1)*v.vertex).rgba,Mask6);

v.vertex.rgb = Vertex;
	return mul (UNITY_MATRIX_MVP, v.vertex);
}
 fixed4 frag() : SV_Target {
    return fixed4(1.0,0.0,0.0,1.0);
}
	ENDCG
}
	ZWrite On
	cull Back
blend off 
	CGPROGRAM

float4 _SSSFlicker_Axis;
float _SSSForce_Flicker;
float _SSSPull_Height;
float4 _SSSPull_Axis;
	#pragma surface frag_surf CLPBR_Standard vertex:vert  addshadow  alpha:fade noambient novertexlights nolightmap nodynlightmap nodirlightmap
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
	float3 worldPos;
	float3 viewDir;
float2 Texcoord;
};
float4 permute(float4 x) { return fmod(((x)+1.0)*x, 289.0); }

float2 fade(float2 t) {
	return t*t*t*(t*(t*6.0-15.0)+10.0);
}

// Classic Perlin noise
float Noise2D(float2 P)
{
	float4 Pi = floor(P.xyxy) + float4(0.0, 0.0, 1.0, 1.0);
	float4 Pf = frac(P.xyxy) - float4(0.0, 0.0, 1.0, 1.0);
	Pi = fmod(Pi, 289.0); // To avoid truncation effects in permutation
	float4 ix = Pi.xzxz;
	float4 iy = Pi.yyww;
	float4 fx = Pf.xzxz;
	float4 fy = Pf.yyww;
	float4 i = permute(permute(ix) + iy);
	float4 gx = 2.0 * frac(i * 0.0243902439) - 1.0; // 1/41 = 0.024...
	float4 gy = abs(gx) - 0.5;
	float4 tx = floor(gx + 0.5);
	gx = gx - tx;
	float2 g00 = float2(gx.x,gy.x);
	float2 g10 = float2(gx.y,gy.y);
	float2 g01 = float2(gx.z,gy.z);
	float2 g11 = float2(gx.w,gy.w);
	float n00 = dot(g00, float2(fx.x, fy.x));
	float n10 = dot(g10, float2(fx.y, fy.y));
	float n01 = dot(g01, float2(fx.z, fy.z));
	float n11 = dot(g11, float2(fx.w, fy.w));
	float2 fade_xy = fade(Pf.xy);
	float2 n_x = lerp(float2(n00, n01), float2(n10, n11), fade_xy.x);
	float n_xy = lerp(n_x.x, n_x.y, fade_xy.y);
	return 4 * n_xy;
}
float4 permute2(float4 x)
{
  return fmod(((x*34.0)+1.0)*x, 289.0);
}

float4 taylorInvSqrt(float4 r)
{
  return 1.79284291400159 - 0.85373472095314 * r;
}

float3 fade3D(float3 t) {
  return t*t*t*(t*(t*6.0-15.0)+10.0);
}

// Classic Perlin noise
float Noise3D(float3 P)
{
  float3 Pi0 = floor(P); // Integer part for indexing
  float3 Pi1 = Pi0 + float3(1,1,1); // Integer part + 1
  Pi0 = fmod(Pi0, 289.0);
  Pi1 = fmod(Pi1, 289.0);
  float3 Pf0 = frac(P); // fracional part for interpolation
  float3 Pf1 = Pf0 - float3(1,1,1); // fracional part - 1.0
  float4 ix = float4(Pi0.x, Pi1.x, Pi0.x, Pi1.x);
  float4 iy = float4(Pi0.yy, Pi1.yy);
  float4 iz0 = Pi0.zzzz;
  float4 iz1 = Pi1.zzzz;

  float4 ixy = permute2(permute2(ix) + iy);
  float4 ixy0 = permute2(ixy + iz0);
  float4 ixy1 = permute2(ixy + iz1);

  float4 gx0 = ixy0 / 7.0;
  float4 gy0 = frac(floor(gx0) / 7.0) - 0.5;
  gx0 = frac(gx0);
  float4 gz0 = float4(0.5,0.5,0.5,0.5) - abs(gx0) - abs(gy0);
  float4 sz0 = step(gz0, float4(0,0,0,0));
  gx0 -= sz0 * (step(0.0, gx0) - 0.5);
  gy0 -= sz0 * (step(0.0, gy0) - 0.5);

  float4 gx1 = ixy1 / 7.0;
  float4 gy1 = frac(floor(gx1) / 7.0) - 0.5;
  gx1 = frac(gx1);
  float4 gz1 = float4(0.5,0.5,0.5,0.5) - abs(gx1) - abs(gy1);
  float4 sz1 = step(gz1, float4(0,0,0,0));
  gx1 -= sz1 * (step(0.0, gx1) - 0.5);
  gy1 -= sz1 * (step(0.0, gy1) - 0.5);

  float3 g000 = float3(gx0.x,gy0.x,gz0.x);
  float3 g100 = float3(gx0.y,gy0.y,gz0.y);
  float3 g010 = float3(gx0.z,gy0.z,gz0.z);
  float3 g110 = float3(gx0.w,gy0.w,gz0.w);
  float3 g001 = float3(gx1.x,gy1.x,gz1.x);
  float3 g101 = float3(gx1.y,gy1.y,gz1.y);
  float3 g011 = float3(gx1.z,gy1.z,gz1.z);
  float3 g111 = float3(gx1.w,gy1.w,gz1.w);

  float4 norm0 = taylorInvSqrt(float4(dot(g000, g000), dot(g010, g010), dot(g100, g100), dot(g110, g110)));
  g000 *= norm0.x;
  g010 *= norm0.y;
  g100 *= norm0.z;
  g110 *= norm0.w;
  float4 norm1 = taylorInvSqrt(float4(dot(g001, g001), dot(g011, g011), dot(g101, g101), dot(g111, g111)));
  g001 *= norm1.x;
  g011 *= norm1.y;
  g101 *= norm1.z;
  g111 *= norm1.w;

  float n000 = dot(g000, Pf0);
  float n100 = dot(g100, float3(Pf1.x, Pf0.yz));
  float n010 = dot(g010, float3(Pf0.x, Pf1.y, Pf0.z));
  float n110 = dot(g110, float3(Pf1.xy, Pf0.z));
  float n001 = dot(g001, float3(Pf0.xy, Pf1.z));
  float n101 = dot(g101, float3(Pf1.x, Pf0.y, Pf1.z));
  float n011 = dot(g011, float3(Pf0.x, Pf1.yz));
  float n111 = dot(g111, Pf1);

  float3 fade_xyz = fade3D(Pf0);
  float4 n_z = lerp(float4(n000, n100, n010, n110), float4(n001, n101, n011, n111), fade_xyz.z);
  float2 n_yz = lerp(n_z.xy, n_z.zw, fade_xyz.y);
  float n_xyz = lerp(n_yz.x, n_yz.y, fade_xyz.x); 
  return 2.2 * n_xyz;
  }
float Unique1D(float t){
	//return frac(sin(floor(t.x))*43558.5453);
	return frac(sin(dot(t ,12.9898)) * 43758.5453);
	//return frac(sin(dot(co.xy ,vec2(12.9898,78.233))) * 43758.5453);
	//return frac(sin(n)*43758.5453);
}
float Lerpify(float P){
	float ft = P * 3.1415927;
	float f = (1 - cos(ft)) * 0.5;
	return f;
}
float D1Lerp(float P, float Col1,float Col2){
	float ft = P * 3.1415927;
	float f = (1 - cos(ft)) * 0.5;
	return Col1+((Col2-Col1)*f);//(Col1*P)+(Col2*(1-P));
}
float Unique2D(float2 t){
	float x = frac(sin(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453);
	//float x = frac(frac(tan(dot(floor(t) ,float2(12.9898,78.233))) * 43758.5453)*7.35);
	return x;
}
float Lerp2D(float2 P, float Col1,float Col2,float Col3,float Col4){
	float2 ft = P * 3.1415927;
	float2 f = (1 - cos(ft)) * 0.5;
	P = f;
	float S1 = lerp(Col1,Col2,P.x);
	float S2 = lerp(Col3,Col4,P.x);
	float L = lerp(S1,S2,P.y);
	return L;
}
float NoiseB2D(float2 P)
{
	float SS = Unique2D(P);
	float SE = Unique2D(P+float2(1.00001,0));
	float ES = Unique2D(P+float2(0,1.00001));
	float EE = Unique2D(P+float2(1,1.00001));
	float xx = Lerp2D(frac(P),SS,SE,ES,EE);
	return xx;
}

float NoiseB1D(float P)
{
	float SS = Unique1D(P);
	float SE = Unique1D(P+1.00001);
	float xx = D1Lerp(frac(P),SS,SE);
	return xx;
}
float Unique3D(float3 t){
	float x = frac(tan(dot(tan(floor(t)),float3(12.9898,78.233,35.344))) * 9.5453);
	return x;
}

float Lerp3D(float3 P, float SSS,float SES,float ESS,float EES, float SSE,float SEE,float ESE,float EEE){
	float3 ft = P * 3.1415927;
	float3 f = P;//(1 - cos(ft)) * 0.5;
	float S1 = lerp(SSS,SES,f.x);
	float S2 = lerp(ESS,EES,f.x);
	float F1 = lerp(S1,S2,f.y);
	float S3 = lerp(SSE,SEE,f.x);
	float S4 = lerp(ESE,EEE,f.x);
	float F2 = lerp(S3,S4,f.y);
	float L = lerp(F1,F2,f.z);//F1;
	return L;
}
float NoiseB3D(float3 P)
{
	float SSS = Unique3D(P+float3(0,0,0));
	float SES = Unique3D(P+float3(1.00001,0,0));
	float ESS = Unique3D(P+float3(0,1.00001,0));
	float EES = Unique3D(P+float3(1.00001,1.00001,0));
	float SSE = Unique3D(P+float3(0,0,1.00001));
	float SEE = Unique3D(P+float3(1.00001,0,1.00001));
	float ESE = Unique3D(P+float3(0,1.00001,1.00001));
	float EEE = Unique3D(P+float3(1.00001,1.00001,1.00001));
	float xx = Lerp3D(frac(P),SSS,SES,ESS,EES,SSE,SEE,ESE,EEE);
	return xx;
}










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
//Dust
float4 Mask0_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)));
Mask0= Mask0_Sample1.r;
float Mask1 = 0;
//Sparkles
float4 Mask0_Copy_2_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)));
Mask1= Mask0_Copy_2_Sample1.r;
float4 Mask0_Copy_Copy_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)));
;
float4 Mask0_Copy_Sample1 = float4(NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((mul(_Object2World, v.vertex).xyz))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)));
Mask1*= Mask0_Copy_Sample1.r;
float4 Mask1_Sample1 = float4(Mask1,Mask1,Mask1,0);
Mask1_Sample1.rgb = round(Mask1_Sample1.rgb);
Mask1= Mask1_Sample1.r;
float Mask2 = 0;
//Squares
float4 Mask2_Sample1 = float4(NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)));
Mask2= Mask2_Sample1.r;
float4 Mask2_Copy_Sample1 = float4(NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((mul(_Object2World, v.vertex).xyz))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)));
Mask2*= Mask2_Copy_Sample1.r;
float Mask3 = 0;
//Lines
float4 Mask3_Sample1 = float4(((((float3(v.texcoord.xyz.xy,0).xyz))+float3(_Time.y/6,0,0))),1);
Mask3_Sample1.rgb = (Mask3_Sample1.rgb*52.92);
Mask3_Sample1.rgb = ((sin(Mask3_Sample1.rgb)+1)/2);
Mask3= Mask3_Sample1.r;
float4 Mask3_Copy_Sample1 = float4(((((float3(v.texcoord.xyz.xy,0).xyz))+float3(_Time.y/6,0,0))),1);
Mask3_Copy_Sample1.rgb = (Mask3_Copy_Sample1.rgb*22.1);
Mask3_Copy_Sample1.rgb = sin(Mask3_Copy_Sample1.rgb);
Mask3*= Mask3_Copy_Sample1.r;
float4 Mask32_Sample1 = float4(Mask3,Mask3,Mask3,0);
Mask32_Sample1.rgb = clamp(Mask32_Sample1.rgb,0,1);
Mask32_Sample1.rgb = pow(Mask32_Sample1.rgb,6.44);
Mask3= Mask32_Sample1.r;
float Mask4 = 0;
//Flicker
float4 Flicker_Sample1 = float4((((((float3(v.texcoord.xyz.xy,0).xyz))*float3(0.1,0.1,0.1))+float3(_Time.y,0,0))),1);
Flicker_Sample1.rgb = sin(Flicker_Sample1.rgb);
Flicker_Sample1.rgb = (Flicker_Sample1.rgb*56.1);
Flicker_Sample1.rgb = (Flicker_Sample1.rgb-(53.54));
Flicker_Sample1.rgb = round(Flicker_Sample1.rgb);
Flicker_Sample1.rgb = clamp(Flicker_Sample1.rgb,0,1);
Mask4= Flicker_Sample1.r;
float4 Flicker2_Sample1 = float4(1, 1, 1, 1);
Mask4= lerp(Mask4,Flicker2_Sample1.r,_SSSForce_Flicker);
float Mask5 = 0;
//FlickerNoise
float4 FlickerNoise_Sample1 = float4((Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((mul(_Object2World, v.vertex).xyz))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2);
Mask5= lerp(Mask5,FlickerNoise_Sample1.r,Mask4);
float Mask6 = 0;
//Pull
float4 Mask6_Sample1 = float4((((mul(_Object2World, v.vertex).xyz))),1);
Mask6_Sample1.rgb = (Mask6_Sample1.rgb+_SSSPull_Height);
Mask6_Sample1.rgb = (float3(1,1,1)-Mask6_Sample1.rgb);
Mask6_Sample1.rgb = pow(Mask6_Sample1.rgb,5.95);
Mask6_Sample1.rgb = clamp(Mask6_Sample1.rgb,0,1);
Mask6= Mask6_Sample1.g;
//Vertex
float4 Vertex3_Sample1 = _SSSFlicker_Axis;
Vertex-= ((Vertex3_Sample1)*float4(v.normal.rgb,1)).rgba*Mask5;
float4 Vertex_Sample1 = _SSSPull_Axis;
Vertex= lerp(Vertex,((Vertex_Sample1)*v.vertex).rgba,Mask6);

v.vertex.rgb = Vertex;
}

void frag_surf (Input IN, inout CSurfaceOutput o) {
float SSShellDepth = 1-0;
float SSParallaxDepth = 0;
	o.Albedo = float3(0.8,0.8,0.8);
	float4 Emission = float4(0,0,0,0);
	o.Smoothness = 0.8821609;
	o.Alpha = 1.0;
	o.Occlusion = 1.0;
	o.Specular = float3(0.3,0.3,0.3);
float4 MultiUse1 = float4(NoiseB3D(((((((IN.worldPos))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(29.26,29.26,29.26))+float3(0,0,_Time.y)))*3)));//2
float4 MultiUse2 = float4(((((float3(IN.Texcoord.xy,0).xyz))+float3(_Time.y/6,0,0))),1);//2
float4 MultiUse3 = float4(1, 1, 1, 1);//3

float Mask0 = 0;
//Dust
float4 Mask0_Sample1 = MultiUse1;
Mask0= Mask0_Sample1.r;
float Mask1 = 0;
//Sparkles
float4 Mask0_Copy_2_Sample1 = float4(NoiseB3D(((((((IN.worldPos))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(308.93,308.93,308.93))+float3(0,0,_Time.y)))*3)));
Mask1= Mask0_Copy_2_Sample1.r;
float4 Mask0_Copy_Copy_Sample1 = float4(NoiseB3D(((((((IN.worldPos))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)),NoiseB3D(((((((IN.worldPos))*float3(19.86,19.86,19.86))+float3(0,0,_Time.y)))*3)));
;
float4 Mask0_Copy_Sample1 = MultiUse1;
Mask1*= Mask0_Copy_Sample1.r;
float4 Mask1_Sample1 = float4(Mask1,Mask1,Mask1,0);
Mask1_Sample1.rgb = round(Mask1_Sample1.rgb);
Mask1= Mask1_Sample1.r;
float Mask2 = 0;
//Squares
float4 Mask2_Sample1 = float4(NoiseB3D((((((round(((IN.worldPos))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((IN.worldPos))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((IN.worldPos))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)),NoiseB3D((((((round(((IN.worldPos))/float3(0.1071437,0.1071437,0.1071437))*float3(0.1071437,0.1071437,0.1071437))+float3(0,0,_Time.y/2))*float3(-2.1,-2.1,-2.1)))*3)));
Mask2= Mask2_Sample1.r;
float4 Mask2_Copy_Sample1 = float4(NoiseB3D(((((round(((IN.worldPos))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((IN.worldPos))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((IN.worldPos))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)),NoiseB3D(((((round(((IN.worldPos))/float3(0.142858,0.142858,0.142858))*float3(0.142858,0.142858,0.142858))*float3(5.9,5.9,5.9)))*3)));
Mask2*= Mask2_Copy_Sample1.r;
float Mask3 = 0;
//Lines
float4 Mask3_Sample1 = MultiUse2;
Mask3_Sample1.rgb = (Mask3_Sample1.rgb*52.92);
Mask3_Sample1.rgb = ((sin(Mask3_Sample1.rgb)+1)/2);
Mask3= Mask3_Sample1.r;
float4 Mask3_Copy_Sample1 = MultiUse2;
Mask3_Copy_Sample1.rgb = (Mask3_Copy_Sample1.rgb*22.1);
Mask3_Copy_Sample1.rgb = sin(Mask3_Copy_Sample1.rgb);
Mask3*= Mask3_Copy_Sample1.r;
float4 Mask32_Sample1 = float4(Mask3,Mask3,Mask3,0);
Mask32_Sample1.rgb = clamp(Mask32_Sample1.rgb,0,1);
Mask32_Sample1.rgb = pow(Mask32_Sample1.rgb,6.44);
Mask3= Mask32_Sample1.r;
float Mask4 = 0;
//Flicker
float4 Flicker_Sample1 = float4((((((float3(IN.Texcoord.xy,0).xyz))*float3(0.1,0.1,0.1))+float3(_Time.y,0,0))),1);
Flicker_Sample1.rgb = sin(Flicker_Sample1.rgb);
Flicker_Sample1.rgb = (Flicker_Sample1.rgb*56.1);
Flicker_Sample1.rgb = (Flicker_Sample1.rgb-(53.54));
Flicker_Sample1.rgb = round(Flicker_Sample1.rgb);
Flicker_Sample1.rgb = clamp(Flicker_Sample1.rgb,0,1);
Mask4= Flicker_Sample1.r;
float4 Flicker2_Sample1 = MultiUse3;
Mask4= lerp(Mask4,Flicker2_Sample1.r,_SSSForce_Flicker);
float Mask5 = 0;
//FlickerNoise
float4 FlickerNoise_Sample1 = float4((Noise3D(((((((IN.worldPos))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((IN.worldPos))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((IN.worldPos))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2,(Noise3D(((((((IN.worldPos))+float3(0,_Time.y,0))*float3(7.74,7.74,7.74)))*3))+1)/2);
Mask5= lerp(Mask5,FlickerNoise_Sample1.r,Mask4);
float Mask6 = 0;
//Pull
float4 Mask6_Sample1 = float4((((IN.worldPos))),1);
Mask6_Sample1.rgb = (Mask6_Sample1.rgb+_SSSPull_Height);
Mask6_Sample1.rgb = (float3(1,1,1)-Mask6_Sample1.rgb);
Mask6_Sample1.rgb = pow(Mask6_Sample1.rgb,5.95);
Mask6_Sample1.rgb = clamp(Mask6_Sample1.rgb,0,1);
Mask6= Mask6_Sample1.g;
//Normals
//Alpha
float4 Alpha_2_Sample1 = float4((((float3((pow(1-dot(o.Normal, IN.viewDir),3)),0,0).xyz))),1);
Alpha_2_Sample1.rgb = pow(Alpha_2_Sample1.rgb,0.17);
o.Alpha= Alpha_2_Sample1.r;
float4 Alpha2_Copy_2_Sample1 = MultiUse3;
o.Alpha+= Alpha2_Copy_2_Sample1.r*Mask1;
float4 Alpha3_Sample1 = MultiUse3;
o.Alpha+= Alpha3_Sample1.r*Mask2;
	o.Alpha *= 1;
//Diffuse
float4 Texture_Sample1 = float4(0, 0, 0, 1);
o.Albedo= Texture_Sample1.rgb;
//Emission
float4 Emission2_Sample1 = lerp(float4(0, 0, 0, 1), float4(0, 0.5034485, 1, 1), ((((pow(1-dot(o.Normal, IN.viewDir),3))))));
Emission2_Sample1.rgb = (Emission2_Sample1.rgb*2.38);
Emission= Emission2_Sample1.rgba;
float4 Alpha2_Copy_Sample1 = float4(0.6137934, 0, 1, 1);
Emission+= Alpha2_Copy_Sample1.rgba*0.5619835*Mask0;
float4 Emission_Sample1 = float4(0, 0.08965492, 1, 1);
Emission_Sample1.rgb = (Emission_Sample1.rgb*6.47);
Emission+= Emission_Sample1.rgba*Mask1;
float4 Alpha2_Copy_2_Copy_Sample1 = float4(1, 0.3931034, 0, 1);
Emission+= Alpha2_Copy_2_Copy_Sample1.rgba*Mask2;
float4 Emission3_Sample1 = float4(1, 0.4758621, 0, 1);
Emission+= Emission3_Sample1.rgba*Mask3;
//Gloss
float4 Specular_Sample1 = float4(0, 0.5862069, 1, 1);
o.Specular= Specular_Sample1.rgb;
	o.Emission = Emission.rgb;}
	ENDCG
}

Fallback "VertexLit"
}

/*
BeginShaderParse
0.9
BeginShaderBase
BeginShaderInput
Type #! 4 #?Type
VisName #! Transparency #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 1 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Specular - Main Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0,0.5862069,1,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Spec Hardness #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.8821609 #?Number
Range0 #! 0.0001 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask0 - Z Offset #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 173.8359 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 1 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask0 - Scale #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 29.26 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask0 Copy - Scale #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 308.93 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask0 Copy Copy - Scale #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 19.86 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Texture Copy - Main Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0,0.1724138,1,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Alpha - Power #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.17 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Alpha2 Copy - Main Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.6137934,0,1,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Emission2 - Main Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0,0.5034485,1,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Emission2 - Multiply #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 2.38 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Normal Map - Scale #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 5.38 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Emission - Main Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0,0.08965492,1,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Mask2 - Size #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.1071437 #?Number
Range0 #! 1E-06 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask2 - Z Offset #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 86.91797 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 3 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask2 - Scale #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! -2.1 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Alpha2 Copy 2 Copy - Main Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 1,0.3931034,0,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Alpha2 Copy 2 Copy - Mix Amount #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 1 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask2 Copy - Scale #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 5.9 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Mask2 Copy - Size #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.142858 #?Number
Range0 #! 1E-06 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask3 - X Offset #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 28.97266 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 4 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Emission3 - Main Color #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 1,0.4758621,0,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Mask32 - Power #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 6.44 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Flicker - X Offset #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 173.8359 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 1 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Flicker Axis #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.2794118,0,0.2794118,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! FlickerNoise - Scale #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 7.74 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! FlickerNoise - Y Offset #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 173.8359 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 1 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 4 #?Type
VisName #! Force Flicker #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 3 #?Type
VisName #! Pull Height #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0.8,0.8,0.8,1 #?Color
Number #! 0.53 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
BeginShaderInput
Type #! 1 #?Type
VisName #! Pull Axis #?VisName
ImageDefault #! 0 #?ImageDefault
Image #!  #?Image
Cube #!  #?Cube
Color #! 0,1,0,1 #?Color
Number #! 0 #?Number
Range0 #! 0 #?Range0
Range1 #! 1 #?Range1
MainType #! 0 #?MainType
SpecialType #! 0 #?SpecialType
EndShaderInput
ShaderName #! Shader Sandwich/Hologram #?ShaderName
Hard Mode #! True #?Hard Mode
Tech Lod #! 200 #?Tech Lod
Cull #! 1 #?Cull
Tech Shader Target #! 3 #?Tech Shader Target
Vertex Recalculation #! False #?Vertex Recalculation
Use Fog #! True #?Use Fog
Use Ambient #! False #?Use Ambient
Use Vertex Lights #! False #?Use Vertex Lights
Use Lightmaps #! False #?Use Lightmaps
Use All Shadows #! False #?Use All Shadows
Diffuse On #! True #?Diffuse On
Lighting Type #! 4 #?Lighting Type
Color #! 0.8,0.8,0.8,1 #?Color
Setting1 #! 0 #?Setting1
Wrap Color #! 0.4,0.2,0.2,1 #?Wrap Color
Specular On #! True #?Specular On
Specular Type #! 0 #?Specular Type
Spec Hardness #! 0.8821609 #^ 2 #?Spec Hardness
Spec Color #! 0.8,0.8,0.8,1 #?Spec Color
Spec Energy Conserve #! True #?Spec Energy Conserve
Spec Offset #! 0 #?Spec Offset
Emission On #! True #?Emission On
Emission Color #! 0,0,0,0 #?Emission Color
Emission Type #! 0 #?Emission Type
Transparency On #! True #?Transparency On
Transparency Type #! 1 #?Transparency Type
ZWrite #! True #?ZWrite
Use PBR #! False #?Use PBR
Transparency #! 1 #^ 0 #?Transparency
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
LayerListName #! Dust #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! Mask0 #?Layer Name
Layer Type #! 5 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 1 #?Noise Type
Noise Dimensions #! 1 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! 29.26 #^ 4 #?Scale
Y Scale #! 29.26 #^ 4 #?Y Scale
Z Scale #! 29.26 #^ 4 #?Z Scale
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 0 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 173.8359 #^ 3 #?Z Offset
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask1 #?LayerListUniqueName
LayerListName #! Sparkles #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! Mask0 Copy 2 #?Layer Name
Layer Type #! 5 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 1 #?Noise Type
Noise Dimensions #! 1 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! 308.93 #^ 5 #?Scale
Y Scale #! 308.93 #^ 5 #?Y Scale
Z Scale #! 308.93 #^ 5 #?Z Scale
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 0 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 173.8359 #^ 3 #?Z Offset
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask0 Copy Copy #?Layer Name
Layer Type #! 5 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 1 #?Noise Type
Noise Dimensions #! 1 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 0 #?Mix Amount
Mix Type #! 3 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! 19.86 #^ 6 #?Scale
Y Scale #! 19.86 #^ 6 #?Y Scale
Z Scale #! 19.86 #^ 6 #?Z Scale
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 0 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 173.8359 #^ 3 #?Z Offset
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask0 Copy #?Layer Name
Layer Type #! 5 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 1 #?Noise Type
Noise Dimensions #! 1 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 3 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! 29.26 #^ 4 #?Scale
Y Scale #! 29.26 #^ 4 #?Y Scale
Z Scale #! 29.26 #^ 4 #?Z Scale
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 0 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 173.8359 #^ 3 #?Z Offset
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask1 #?Layer Name
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
TypeS #! SSEMathRound #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask2 #?LayerListUniqueName
LayerListName #! Squares #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! Mask2 #?Layer Name
Layer Type #! 5 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 1 #?Noise Type
Noise Dimensions #! 1 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEPixelate #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Size #! 0.1071437 #^ 14 #?Size
Y Size #! 0.1071437 #^ 14 #?Y Size
Z Size #! 0.1071437 #^ 14 #?Z Size
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 0 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 86.91797 #^ 15 #?Z Offset
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! -2.1 #^ 16 #?Scale
Y Scale #! -2.1 #^ 16 #?Y Scale
Z Scale #! -2.1 #^ 16 #?Z Scale
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask2 Copy #?Layer Name
Layer Type #! 5 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 1 #?Noise Type
Noise Dimensions #! 1 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 3 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEPixelate #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Size #! 0.142858 #^ 20 #?Size
Y Size #! 0.142858 #^ 20 #?Y Size
Z Size #! 0.142858 #^ 20 #?Z Size
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! 5.9 #^ 19 #?Scale
Y Scale #! 5.9 #^ 19 #?Y Scale
Z Scale #! 5.9 #^ 19 #?Z Scale
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask3 #?LayerListUniqueName
LayerListName #! Lines #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! Mask3 #?Layer Name
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
TypeS #! SSEMathMul #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Multiply #! 52.92 #?Multiply
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathSin #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
0-1 #! True #?0-1
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 28.97266 #^ 21 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 0 #?Z Offset
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask3 Copy #?Layer Name
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
Mix Type #! 3 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathMul #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Multiply #! 22.1 #?Multiply
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathSin #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
0-1 #! False #?0-1
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 28.97266 #^ 21 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 0 #?Z Offset
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Mask32 #?Layer Name
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
TypeS #! SSEMathRound #?TypeS
IsVisible #! False #?IsVisible
UseAlpha #! 0 #?UseAlpha
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathClamp #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Min #! 0 #?Min
Max #! 1 #?Max
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathPow #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Power #! 6.44 #^ 23 #?Power
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask4 #?LayerListUniqueName
LayerListName #! Flicker #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! Flicker #?Layer Name
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
TypeS #! SSEMathSin #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
0-1 #! False #?0-1
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathMul #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Multiply #! 56.1 #?Multiply
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathSub #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Subtract #! 53.54 #?Subtract
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! 0.1 #?Scale
Y Scale #! 0.1 #?Y Scale
Z Scale #! 0.1 #?Z Scale
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 173.8359 #^ 24 #?X Offset
Y Offset #! 0 #?Y Offset
Z Offset #! 0 #?Z Offset
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathRound #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathClamp #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Min #! 0 #?Min
Max #! 1 #?Max
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Flicker2 #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 0 #^ 28 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask5 #?LayerListUniqueName
LayerListName #! FlickerNoise #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! FlickerNoise #?Layer Name
Layer Type #! 5 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 1 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! 4 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEUVOffset #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
X Offset #! 0 #?X Offset
Y Offset #! 173.8359 #^ 27 #?Y Offset
Z Offset #! 0 #?Z Offset
EndShaderEffect
BeginShaderEffect
TypeS #! SSEUVScale #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Seperate #! False #?Seperate
Scale #! 7.74 #^ 26 #?Scale
Y Scale #! 7.74 #^ 26 #?Y Scale
Z Scale #! 7.74 #^ 26 #?Z Scale
EndShaderEffect
EndShaderLayer
EndShaderLayerList
BeginShaderLayerList
LayerListUniqueName #! Mask6 #?LayerListUniqueName
LayerListName #! Pull #?LayerListName
Is Mask #! True #?Is Mask
EndTag #! g #?EndTag
BeginShaderLayer
Layer Name #! Mask6 #?Layer Name
Layer Type #! 7 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 7 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathAdd #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Add #! 0.53 #^ 29 #?Add
EndShaderEffect
BeginShaderEffect
TypeS #! SSEInvert #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
EndShaderEffect
BeginShaderEffect
TypeS #! SSEMathPow #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Power #! 5.95 #?Power
EndShaderEffect
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
Layer Name #! Texture #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0,0,0,1 #?Main Color
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
EndTag #! r #?EndTag
BeginShaderLayer
Layer Name #! Alpha 2 #?Layer Name
Layer Type #! 7 #?Layer Type
Main Color #! 0,0,0,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 4 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathPow #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Power #! 0.17 #^ 8 #?Power
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Alpha2 Copy 2 #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 1 #?Mix Type
Stencil #! 1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Alpha3 #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 1,1,1,1 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 1 #?Mix Type
Stencil #! 2 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
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
Main Color #! 0,0.5862069,1,1 #^ 1 #?Main Color
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
BeginShaderLayer
Layer Name #! Emission2 #?Layer Name
Layer Type #! 1 #?Layer Type
Main Color #! 0,0.5034485,1,1 #^ 10 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 4 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! -1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathMul #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Multiply #! 2.38 #^ 11 #?Multiply
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Alpha2 Copy #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0.6137934,0,1,1 #^ 9 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 0.5619835 #?Mix Amount
Mix Type #! 1 #?Mix Type
Stencil #! 0 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Emission #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0,0.08965492,1,1 #^ 13 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 1 #?Mix Type
Stencil #! 1 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
BeginShaderEffect
TypeS #! SSEMathMul #?TypeS
IsVisible #! True #?IsVisible
UseAlpha #! 0 #?UseAlpha
Multiply #! 6.47 #?Multiply
EndShaderEffect
EndShaderLayer
BeginShaderLayer
Layer Name #! Alpha2 Copy 2 Copy #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 1,0.3931034,0,1 #^ 17 #?Main Color
Second Color #! 0.001,0.001,0.001,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #^ 18 #?Mix Amount
Mix Type #! 1 #?Mix Type
Stencil #! 2 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Emission3 #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 1,0.4758621,0,1 #^ 22 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 1 #?Mix Type
Stencil #! 3 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
EndShaderLayer
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
BeginShaderLayer
Layer Name #! Vertex3 #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0.2794118,0,0.2794118,1 #^ 25 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 2 #?Mix Type
Stencil #! 5 #?Stencil
Vertex Mask #! 1 #?Vertex Mask
EndShaderLayer
BeginShaderLayer
Layer Name #! Vertex #?Layer Name
Layer Type #! 0 #?Layer Type
Main Color #! 0,1,0,1 #^ 30 #?Main Color
Second Color #! 0,0,0,1 #?Second Color
Main Texture #!  #?Main Texture
Cubemap #!  #?Cubemap
Noise Type #! 0 #?Noise Type
Noise Dimensions #! 0 #?Noise Dimensions
Use Alpha #! False #?Use Alpha
UV Map #! 0 #?UV Map
Mix Amount #! 1 #?Mix Amount
Mix Type #! 0 #?Mix Type
Stencil #! 6 #?Stencil
Vertex Mask #! 2 #?Vertex Mask
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
