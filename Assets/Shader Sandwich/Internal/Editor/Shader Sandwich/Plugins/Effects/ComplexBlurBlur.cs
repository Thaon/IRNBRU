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
public class SSEComplexBlur : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSEComplexBlur";
		SE.Name = "Blur/Complex Blur";//+UnityEngine.Random.value.ToString();

		if (BInputs==true||SE.Inputs.Count!=5){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Separate",true));
			SE.Inputs.Add(new ShaderVar("Blur X",0.1f));
			SE.Inputs.Add(new ShaderVar("Blur Y",0.1f));
			SE.Inputs.Add(new ShaderVar("Fast",true));
			SE.Inputs.Add(new ShaderVar("Quality",3f));
		} 
		SE.Inputs[1].Range0 = 0;
		SE.Inputs[1].Range1 = 0.5f;
		SE.Inputs[2].Range0 = 0;
		SE.Inputs[2].Range1 = 0.5f;
		SE.Inputs[4].NoInputs = true;
		SE.Inputs[4].Range0 = 1;
		SE.Inputs[4].Range1 = 20;
		SE.Inputs[4].Float = Mathf.Max(1,SE.Inputs[4].Float);
		SE.Inputs[4].Float = Mathf.Round(SE.Inputs[4].Float);
		
		if (!SE.Inputs[0].On){
			SE.Inputs[2].Hidden = true;
			SE.Inputs[2].Use = false;
			SE.Inputs[1].Name = "Blur";
			SE.Inputs[2].Float = SE.Inputs[1].Float;
			SE.Inputs[2].UseInput = SE.Inputs[1].UseInput;
		}
		else{
			SE.Inputs[1].Name = "Blur X";
			SE.Inputs[2].Hidden = false;
			SE.Inputs[2].Use = true;
		}		
		
		SE.WorldPos = false;
		SE.WorldRefl = false;
		SE.WorldNormals = false;		
	}
	public static ShaderColor GetAddBlur(ShaderEffect SE,ShaderColor[] OldColors,int X,int Y,int W,int H,int XAdd,int YAdd){
		int XX = (int)((((float)X/(float)W)+((float)XAdd*SE.Inputs[1].Float/SE.Inputs[4].Float))*(float)W);
		int YY = (int)((((float)Y/(float)H)+((float)YAdd*SE.Inputs[2].Float/SE.Inputs[4].Float))*(float)H);
		return OldColors[ShaderUtil.FlatArray(XX,YY,W,H)];	
	}
	public static string GetAddBlurString(ShaderGenerate SG,ShaderLayer SL,ShaderEffect SE,int Effect,int XAdd,int YAdd){
		string XX = ((float)XAdd*SE.Inputs[1].Float/SE.Inputs[4].Float).ToString();
		string YY = ((float)YAdd*SE.Inputs[2].Float/SE.Inputs[4].Float).ToString();
		if (!SE.Inputs[0].On){
			YY = ((float)YAdd*SE.Inputs[1].Float/SE.Inputs[4].Float).ToString();
		}
		
		if (SE.Inputs[1].Input!=null)
		XX = ((float)XAdd/SE.Inputs[4].Float).ToString()+"*"+SE.Inputs[1].Get();
		if (SE.Inputs[2].Input!=null)
		YY = ((float)YAdd/SE.Inputs[4].Float).ToString()+"*"+SE.Inputs[2].Get();
		
		if (!SE.Inputs[0].On){
			if (SE.Inputs[1].Input!=null)
			YY = ((float)YAdd/SE.Inputs[4].Float).ToString()+"*"+SE.Inputs[1].Get();
		}
		
		return "+("+SL.StartNewBranch(SG,SL.GCUVs(SG,XX,YY,"0"),Effect)+")";
		//return OldColors[ShaderUtil.FlatArray(XX,YY,W,H)];	
	}
	public static Color LerpBlur(float X, float Y, Color Col1,Color Col2,Color Col3,Color Col4){
		return Col1*(1f-X)*(1f-Y) + Col2*X*(1f-Y) + Col3*(1f-X)*Y + Col4*X*Y;
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor[] OldColors,int X, int Y, int W, int H){
	
	
		SE.Inputs[4].Float = Mathf.Round(SE.Inputs[4].Float);
		ShaderColor OldColor = GetAddBlur(SE,OldColors,X,Y,W,H,0,0);
		
		ShaderColor NewColor = OldColor;
		int AddCount = 0;
		
		if (SE.Inputs[3].On==false){
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,i,i);AddCount+=1;}}
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,-i,-i);AddCount+=1;}}	
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,-i,i);AddCount+=1;}}
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,i,-i);AddCount+=1;}}		
		}
		for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i++){
		if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,i,0);AddCount+=1;}}
		for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i++){
		if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,0,i);AddCount+=1;}}
		
		

		//if (SE.Inputs[2].On==false)
		NewColor/=(float)((AddCount)+1);
		//else
		//NewColor/=(SE.Inputs[3].Float*4f)+1;

		return NewColor;
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		string retVal = "((("+Line+")";
		int AddCount = 0;
		
		if (SE.Inputs[3].On==false){
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,i,i);AddCount+=1;}}
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,-i,-i);AddCount+=1;}}	
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,-i,i);AddCount+=1;}}
			for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i+=2){
			if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,i,-i);AddCount+=1;}}		
		}
		if (SE.Inputs[1].Float!=0f)
		for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i++){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,i,0);AddCount+=1;}}
		
		if (SE.Inputs[2].Float!=0f)
		for(int i = (int)(-SE.Inputs[4].Float);i<=(int)(SE.Inputs[4].Float);i++){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,0,i);AddCount+=1;}}
		
		retVal += ")/"+(AddCount+1).ToString()+")";

		//+("+SL.GetSubPixel(SG,SL.GCUVs(SG,"0.01","0","0"),Effect)+"))/2)";
		return retVal;
	}
}