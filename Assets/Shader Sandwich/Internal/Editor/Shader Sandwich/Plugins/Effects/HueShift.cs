using UnityEngine;
using UnityEditor;
using System.IO;
using System.Collections.Generic;
using System.Collections;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using SU = ShaderUtil;
using System.Xml.Serialization;
using System.Runtime.Serialization;
//using System.Xml;
[System.Serializable]
public class SSEHueShift : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEHueShift";
		SE.Name = "Color/Hue Shift";//+UnityEngine.Random.value.ToString();
		SE.Function = @"
float3 rgb2hsv(float3 c)
{
	float4 K = float4(0.0, -1.0 / 3.0, 2.0 / 3.0, -1.0);
	float4 p = lerp(float4(c.bg, K.wz), float4(c.gb, K.xy), step(c.b, c.g));
	float4 q = lerp(float4(p.xyw, c.r), float4(c.r, p.yzx), step(p.x, c.r));

	float d = q.x - min(q.w, q.y);
	float e = 1.0e-10;
	return float3(abs(q.z + (q.w - q.y) / (6.0 * d + e)), d / (q.x + e), q.x);
}
float3 hsv2rgb(float3 c)
{
	c.r = frac(c.r);
	c.gb = saturate(c.gb);
	float4 K = float4(1.0, 2.0 / 3.0, 1.0 / 3.0, 3.0);
	float3 p = abs(frac(c.xxx + K.xyz) * 6.0 - K.www);
	float3 pp = c.z * lerp(K.xxx, clamp(p - K.xxx, 0.0, 1.0), c.y);
	return pp;
}";
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;	
	
		if (BInputs==true){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Hue",0));
			SE.Inputs.Add(new ShaderVar("Saturation",0));
			SE.Inputs.Add(new ShaderVar("Value",0));
		} 
		SE.Inputs[0].Range0 = -1;
		SE.Inputs[0].Range1 = 1;		
		SE.Inputs[1].Range0 = -1;
		SE.Inputs[1].Range1 = 1;			
		SE.Inputs[2].Range0 = -1;
		SE.Inputs[2].Range1 = 1;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		return "hsv2rgb(rgb2hsv("+Line+")+float3("+SE.Inputs[0].Get()+","+SE.Inputs[1].Get()+","+SE.Inputs[2].Get()+"))";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		float Hue = 0;
		float Sat = 0;
		float Val = 0;
		EditorGUIUtility.RGBToHSV((Color)OldColor,out Hue,out Sat,out Val);
		Hue+=SE.Inputs[0].Float;
		if (Hue<0)
		Hue += 1f;
		if (Hue>1)
		Hue -= 1;
		Sat=Mathf.Clamp01(Sat+SE.Inputs[1].Float);
		Val=Mathf.Clamp01(Val+SE.Inputs[2].Float);
		ShaderColor NewColor = (ShaderColor)EditorGUIUtility.HSVToRGB(Hue,Sat,Val);
		NewColor.a = OldColor.a;
		return NewColor;
	}
}

