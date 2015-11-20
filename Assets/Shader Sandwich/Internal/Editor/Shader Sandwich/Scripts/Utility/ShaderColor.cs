using UnityEngine;
using UnityEditor;
[System.Serializable]
public class ShaderColor {
	public float r;
	public float g;
	public float b;
	public float a;
	public ShaderColor Copy(){
		return new ShaderColor(r,g,b,a);
	}
	public ShaderColor(float rr, float gg, float bb, float aa){
		r = rr;
		g = gg;
		b = bb;
		a = aa;
	}
	public ShaderColor(Vector4 Col){
		r = Col.x;
		g = Col.y;
		b = Col.z;
		a = Col.w;
	}
	public ShaderColor(Color Col){
		r = Col.r;
		g = Col.g;
		b = Col.b;
		a = Col.a;
	}
	public ShaderColor(float Col){
		r = Col;
		g = Col;
		b = Col;
		a = Col;
	}
	public ShaderColor Sub(ShaderColor SC){
		return new ShaderColor(r-SC.r,g-SC.g,b-SC.b,a-SC.a);
	}
	public ShaderColor Mul(ShaderColor SC){
		return new ShaderColor(r*SC.r,g*SC.g,b*SC.b,a*SC.a);
	}
	public ShaderColor Add(ShaderColor SC){
		return new ShaderColor(r+SC.r,g+SC.g,b+SC.b,a+SC.a);
	}
	public ShaderColor Mul(float SC){
		return new ShaderColor(r*SC,g*SC,b*SC,a*SC);
	}
	public Color ToColor(){
		return new Color(r,g,b,a);
	}
	public bool Cmp(ShaderColor SC){
		if (Mathf.Approximately(r,SC.r)&&Mathf.Approximately(g,SC.g)&&Mathf.Approximately(b,SC.b)&&Mathf.Approximately(a,SC.a))
		return true;
		
		return false;
	}
	public string Save(){
		return r.ToString()+","+g.ToString()+","+b.ToString()+","+a.ToString();
	}
	public override string ToString(){
		return Save();
	}
	public void Load(string S){
		//Debug.Log(S);
		string[] parts = S.Split(new char[] { ',' });
		r = float.Parse(parts[0]);
		g = float.Parse(parts[1]);
		b = float.Parse(parts[2]);
		a = float.Parse(parts[3]);
		//Debug.Log(r);
	}
	public static bool IsStupid(float Ass){
		if (float.IsNaN(Ass)||float.IsInfinity(Ass))
		return true;
		
		return false;
	}
	public static ShaderColor Lerp(ShaderColor S1,ShaderColor S2,float Lerp){
	/*if (Lerp==0f)
	return S1;
	if (Lerp==1f)
	return S2;*/
		//return new ShaderColor((S2.r - S1.r) * Lerp + S1.r,(S2.g - S1.g) * Lerp + S1.g,(S2.b - S1.b) * Lerp + S1.b,(S2.a - S1.a) * Lerp + S1.a);
//		float NaNRep = 0f;
		/*if (IsStupid(S1.r))S1.r=NaNRep;
		if (IsStupid(S1.g))S1.g=NaNRep;
		if (IsStupid(S1.b))S1.b=NaNRep;
		if (IsStupid(S1.a))S1.a=NaNRep;
		if (IsStupid(S2.r))S2.r=NaNRep;
		if (IsStupid(S2.g))S2.g=NaNRep;
		if (IsStupid(S2.b))S2.b=NaNRep;
		if (IsStupid(S2.a))S2.a=NaNRep;*/

		return new ShaderColor(
		(S2.r - S1.r) * Lerp + S1.r,
		(S2.g - S1.g) * Lerp + S1.g,
		(S2.b - S1.b) * Lerp + S1.b,
		(S2.a - S1.a) * Lerp + S1.a);
	}
	public static explicit operator Vector4   (ShaderColor SS){
		return new Vector4(SS.r,SS.g,SS.b,SS.a);
	}
	public static explicit operator ShaderColor (Vector4 SS){
		return new ShaderColor(SS.x,SS.y,SS.z,SS.w);
	}
	public static explicit operator Color     (ShaderColor SS){
		return new Color(SS.r,SS.g,SS.b,SS.a);
	}
	public static explicit operator ShaderColor (Color SS){
		return new ShaderColor(SS.r,SS.g,SS.b,SS.a);
	}
	public static ShaderColor operator +(ShaderColor a, ShaderColor b){
		return new ShaderColor(a.r+b.r,a.g+b.g,a.b+b.b,a.a+b.a);
	}
	public static ShaderColor operator *(ShaderColor a, ShaderColor b){
		return new ShaderColor(a.r*b.r,a.g*b.g,a.b*b.b,a.a*b.a);
	}
	public static ShaderColor operator /(ShaderColor a, ShaderColor b){
		return new ShaderColor(a.r/b.r,a.g/b.g,a.b/b.b,a.a/b.a);
	}
	public static ShaderColor operator -(ShaderColor a, ShaderColor b){
		return new ShaderColor(a.r-b.r,a.g-b.g,a.b-b.b,a.a-b.a);
	}
	public static ShaderColor operator +(ShaderColor a, float b){
		return new ShaderColor(a.r+b,a.g+b,a.b+b,a.a+b);
	}
	public static ShaderColor operator *(ShaderColor a, float b){
		return new ShaderColor(a.r*b,a.g*b,a.b*b,a.a*b);
	}
	public static ShaderColor operator /(ShaderColor a, float b){
		return new ShaderColor(a.r/b,a.g/b,a.b/b,a.a/b);
	}
	public static ShaderColor operator -(ShaderColor a, float b){
		return new ShaderColor(a.r-b,a.g-b,a.b-b,a.a-b);
	}
}