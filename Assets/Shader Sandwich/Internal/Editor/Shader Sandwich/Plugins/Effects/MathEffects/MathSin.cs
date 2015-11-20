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
public class SSEMathSin : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEMathSin";
		SE.Name = "Maths/Sine";//+UnityEngine.Random.value.ToString();
		if (BInputs==true||SE.Inputs.Count!=1){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("0-1",true));
		}
		SE.Function = "";
		SE.LinePre = "";
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		if (SE.Inputs[0].On)
		return "((sin("+Line+")+1)/2)";
		else
		return "sin("+Line+")";
	}
	public static string GenerateAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		if (SE.Inputs[0].On)
		return "((sin("+Line+")+1)/2)";
		else
		return "sin("+Line+")";
	}
	public static string GenerateWAlpha(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		if (SE.Inputs[0].On)
		return "((sin("+Line+")+1)/2)";
		else
		return "sin("+Line+")";
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor OldColor){
		//if (OldColor.r<0)
		//Debug.Log(Mathf.Sin(OldColor.r));
		ShaderColor NewColor;
		if (SE.Inputs[0].On)
		NewColor = ((new ShaderColor(Mathf.Sin(OldColor.r),Mathf.Sin(OldColor.g),Mathf.Sin(OldColor.b),Mathf.Sin(OldColor.a)))+1)/2;
		else
		NewColor = ((new ShaderColor(Mathf.Sin(OldColor.r),Mathf.Sin(OldColor.g),Mathf.Sin(OldColor.b),Mathf.Sin(OldColor.a))));
		//Debug.Log(NewColor.ToString());
		return NewColor;
	}
}

