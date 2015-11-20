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
using System.Xml;
[System.Serializable]
public class ShaderEffect : ScriptableObject{
	public string TypeS
	{
		get { return TypeS_Real.Text; }
		set { TypeS_Real.Text = value; }
	}
	public ShaderVar TypeS_Real = new ShaderVar("TypeS","");
	
	public string Name = "";
	public string Function = "";
	public string Line = "";
	public string LinePre = "";
	[NonSerialized]public bool HandleAlpha = true;
	public ShaderVar UseAlpha = new ShaderVar("UseAlpha",0);
	public bool Visible{
		get{
			return IsVisible.On;
		}
		set{
			IsVisible.On = value;
		}
	}
	public ShaderVar IsVisible = new ShaderVar("IsVisible",true);
	public List<ShaderVar> Inputs = new List<ShaderVar>();
	
	public bool WorldPos = false;
	public bool WorldRefl = false;
	public bool WorldNormals = false;
	
	public bool IsUVEffect = false;
	
	public bool ChangeBaseCol = false;
	public bool AutoCreated = false;
	
	//public bool Draw(Rect rect){

	//}
	/*public ShaderEffectType(List<string> Load){
		
		int CurType = 0;
		//string[] lines = Load.Split(new string[] {"\n"}, StringSplitOptions.RemoveEmptyEntries);
		//Debug.Log(lines.Length);
		foreach(string line in Load){
			if (line.Contains("Name:"))
			Name = line.Replace("Name:","").Trim();
			if (line.Contains("Arguments:"))
				CurType = 1;
			else
			if (line.Contains("Uses:"))
				CurType = 2;
			else
			if (line.Contains("Function:"))
				CurType = 3;
			else
			if (line.Contains("Line:"))
				CurType = 4;
			else
			if (line.Contains("LinePre:"))
				CurType = 5;
			else{
				if (CurType==1){
					ShaderVar SV = null;
					if (line.StartsWith("Slider")){
						string[] Args = line.Substring(line.IndexOf("(")+1,(line.IndexOf(")")-1)-line.IndexOf("(")).Split(new string[] {","}, StringSplitOptions.None);
						string N = line.Substring(line.IndexOf(")")+1,(line.IndexOf("=")-1)-line.IndexOf(")")).Trim();
						float Value = float.Parse((line.Substring(line.IndexOf("=")+1).Trim()),System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
						SV = new ShaderVar(N,Value);
						SV.Range0 = float.Parse(Args[0],System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
						SV.Range1 = float.Parse(Args[1],System.Globalization.CultureInfo.InvariantCulture.NumberFormat);
					}
					if (SV!=null)
					Inputs.Add(SV);
				}
				
				if (CurType==2){
					if (line.Contains("Position"))
						WorldPos = true;
					if (line.Contains("Reflection"))
						WorldRefl = true;
					if (line.Contains("Normal"))
						WorldNormals = true;
				}
				
				
				if (CurType==3)
					Function+=line;
				if (CurType==4)
					Line+=line+"\n";
				if (CurType==5)
					LinePre+=line+"\n";
			}
		}
	}*/
	override public string ToString(){
		return Name;
	}
	public string ToStringDetailed(){
		string Args = "";
		foreach(ShaderVar SV in Inputs)
		Args+=SV.Save()+"\n";
		return "Name: "+Name+"\n Function:"+Function+"\n Line:"+Line+"\n LinePre:"+LinePre+"\nArguments:\n"+Args;
	}
	
	public bool Draw(Rect rect){
		//GUI.Box(rect,"");
		int Y = 0;
		bool update = false;
		foreach(ShaderVar SV in Inputs){
			if (SV.Hidden == false){
				if (SV.Draw(new Rect(rect.x+2,rect.y+Y*20,rect.width-4,20),SV.Name))update = true;
				Y+=1;
			}
		}
		//Debug.Log(GUIUtility.hotControl);
		return update;
	}
	public ShaderEffect(){}
	public ShaderEffect(string Ty){
		//Type t = Type.GetType(Ty);
		//t.GetMethod("Activate").Invoke(null,new object[]{this,true});
		ShaderEffectIn(Ty);
	}
	public void ShaderEffectIn(string Ty){
		ShaderEffectIn(Ty,null);
	}
	public void ShaderEffectIn(string Ty,ShaderLayer SL){
		Type t = Type.GetType(Ty);
		
		if (t.GetMethod("Activate").GetParameters().Length==2)
		t.GetMethod("Activate").Invoke(null,new object[]{this,true});
		
		if (t.GetMethod("Activate").GetParameters().Length==3)
		t.GetMethod("Activate").Invoke(null,new object[]{this,true,SL});
		
		HandleAlpha = true;
		if (t.GetMethod("GenerateWAlpha")==null)
		HandleAlpha = false;
	}
	public void Update(){
		Type t = Type.GetType(TypeS);
		if (t.GetMethod("Activate").GetParameters().Length==2)
		t.GetMethod("Activate").Invoke(null,new object[]{this,false});
		else		
		t.GetMethod("Activate").Invoke(null,new object[]{this,false,null});		
	}
	static public System.Reflection.MethodInfo GetMethod(string Ty,string S){
		//Debug.Log(Ty);
		Type t = Type.GetType(Ty);
		//Debug.Log(t.GetMethod(S)); 
		return t.GetMethod(S);
	}
	public Dictionary<string,ShaderVar> GetSaveLoadDict(){
		Dictionary<string,ShaderVar> D = new Dictionary<string,ShaderVar>();
		
		D.Add(TypeS_Real.Name,TypeS_Real);
		D.Add(IsVisible.Name,IsVisible);
		D.Add(UseAlpha.Name,UseAlpha);
		foreach(ShaderVar SV in Inputs){
			D.Add(SV.Name,SV);
		}		
		return D;
	}	
	public string Save(){
		string S = "BeginShaderEffect\n";
		S += ShaderUtil.SaveDict(GetSaveLoadDict());
		S += "EndShaderEffect\n"; 
		return S;
	}
static public ShaderEffect Load(StringReader S){
	string ShaderEffectTypeS = ShaderUtil.LoadLineExplode(ShaderUtil.Sanitize(S.ReadLine()))[1].Trim();
	//Debug.Log(ShaderEffectTypeS);
	ShaderEffect SE = ShaderEffect.CreateInstance<ShaderEffect>();
	SE.ShaderEffectIn(ShaderEffectTypeS);
	var D = SE.GetSaveLoadDict();
	var Act = Type.GetType(ShaderEffectTypeS).GetMethod("Activate");
	while(1==1){
		string Line =  S.ReadLine();
		if (Line!=null){
			if(Line=="EndShaderEffect")break;
			
			if (Line.Contains("#!")){
			ShaderUtil.LoadLine(D,Line);
			
			if (Act.GetParameters().Length==2)
			Act.Invoke(null,new object[]{SE,false});
			else
			Act.Invoke(null,new object[]{SE,false,null});
			
			D = SE.GetSaveLoadDict();
			}
		}
		else
		break;
	}
	return SE;
}
	//public static void Activate(bool Inputs){}
	//public static Color PixelChange(Color OldColor){return OldColor;}
}