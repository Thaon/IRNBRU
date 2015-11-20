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
public class SSECAntiAliasing : ShaderEffect{
	public static void Activate(ShaderEffect SE,bool BInputs){
		SE.TypeS = "SSECAntiAliasing";
		SE.Name = "Blur/Anti Aliasing";//+UnityEngine.Random.value.ToString();

		if (BInputs==true||SE.Inputs.Count!=1){
			SE.Inputs = new List<ShaderVar>();
			SE.Inputs.Add(new ShaderVar("Quality",1f));
		} 
		SE.Inputs[0].Range0 = 1;
		SE.Inputs[0].Range1 = 4;
		SE.Inputs[0].Float = Mathf.Max(1,SE.Inputs[0].Float);
		SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);	
	}
	public static void SetUsed(ShaderGenerate SG){
		SG.UsedScreenPos = true;
	}
	public static string GetAddBlurString(ShaderGenerate SG,ShaderLayer SL,ShaderEffect SE,int Effect,int XAdd,int YAdd){
		string XAnti = "AntiAlias";//IN.screenPos.z/300";
		string YAnti = XAnti;//"IN.screenPos.z";
		
		
		//if (SE.Inputs[1].Input!=null)
		string XX = ((float)XAdd/SE.Inputs[0].Float).ToString()+"*"+XAnti;
		//if (SE.Inputs[2].Input!=null)
		string YY = ((float)YAdd/SE.Inputs[0].Float).ToString()+"*"+YAnti;
		
		//if (!SE.Inputs[0].On){
		//	if (SE.Inputs[1].Input!=null)
		//	YY = ((float)YAdd/SE.Inputs[0].Float).ToString()+"*"+XAnti;
		//}
		
		return "+("+SL.StartNewBranch(SG,SL.GCUVs(SG,XX,YY,"0"),Effect)+")";
		//return OldColors[ShaderUtil.FlatArray(XX,YY,W,H)];	
	}
	public static Color LerpBlur(float X, float Y, Color Col1,Color Col2,Color Col3,Color Col4){
		return Col1*(1f-X)*(1f-Y) + Col2*X*(1f-Y) + Col3*(1f-X)*Y + Col4*X*Y;
	}
	public static ShaderColor Preview(ShaderEffect SE,ShaderColor[] OldColors,int X, int Y, int W, int H){
	
		ShaderColor OldColor = OldColors[ShaderUtil.FlatArray(X,Y,W,H)];//GetAddBlur(SE,OldColors,X,Y,W,H,0,0);
		/*SE.Inputs[0].Float = Mathf.Round(SE.Inputs[0].Float);
		
		
		Color NewColor = OldColor;
		int AddCount = 0;
		
		if (SE.Inputs[3].On==false){
			for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,i,i);AddCount+=1;}}
			for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,-i,-i);AddCount+=1;}}	
			for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,-i,i);AddCount+=1;}}
			for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
			if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,i,-i);AddCount+=1;}}		
		}
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i++){
		if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,i,0);AddCount+=1;}}
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i++){
		if (i!=0){NewColor+=GetAddBlur(SE,OldColors,X,Y,W,H,0,i);AddCount+=1;}}
		
		

		//if (SE.Inputs[2].On==false)
		NewColor/=(float)((AddCount)+1);
		//else
		//NewColor/=(SE.Inputs[3].Float*4f)+1;
*/
		return OldColor;
	}
	public static string Generate(ShaderGenerate SG,ShaderEffect SE, ShaderLayer SL,string Line,int Effect){
		if (!SL.IsVertex){
		string retVal = "((("+Line+")";
		int AddCount = 0;
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,i,i);AddCount+=1;}}
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,-i,-i);AddCount+=1;}}	
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,-i,i);AddCount+=1;}}
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i+=2){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,i,-i);AddCount+=1;}}
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i++){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,i,0);AddCount+=1;}}
		for(int i = (int)(-SE.Inputs[0].Float);i<=(int)(SE.Inputs[0].Float);i++){
		if (i!=0){retVal+=GetAddBlurString(SG,SL,SE,Effect,0,i);AddCount+=1;}}
		
		retVal += ")/"+(AddCount+1).ToString()+")";

		//+("+SL.GetSubPixel(SG,SL.GCUVs(SG,"0.01","0","0"),Effect)+"))/2)";
		return retVal;
		}
		return Line;
	}
	public static string GenerateStart(ShaderGenerate SG){
		return "half AntiAlias = (IN.screenPos.z*abs((ddx(IN.screenPos.z)+ddy(IN.screenPos.z))/4/IN.screenPos.z)+(IN.screenPos.z/600));\n";
	}
}