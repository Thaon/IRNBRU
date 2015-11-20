using UnityEngine;
using UnityEngine.Rendering;
using System.Collections;
public class SSVector4{
	public float x;
	public float y;
	public float z;
	public float w;
	public float r{
		get{
			return x;
		}
		set{
			x = value;
		}
	}
	public float g{
		get{
			return y;
		}
		set{
			y = value;
		}
	}
	public float b{
		get{
			return z;
		}
		set{
			z = value;
		}
	}
	public float a{
		get{
			return w;
		}
		set{
			w = value;
		}
	}
	public SSVector4(float X, float Y,float Z,float W){
		x = X;
		y = Y;
		z = Z;
		a = W;
	}
	public static implicit operator Vector4   (SSVector4 SS){
		return new Vector4(SS.x,SS.y,SS.z,SS.w);
	}
	public static implicit operator SSVector4 (Vector4 SS){
		return new SSVector4(SS.x,SS.y,SS.z,SS.w);
	}
	public static implicit operator Color     (SSVector4 SS){
		return new Color(SS.x,SS.y,SS.z,SS.w);
	}
	public static implicit operator SSVector4 (Color SS){
		return new SSVector4(SS.r,SS.g,SS.b,SS.a);
	}
}