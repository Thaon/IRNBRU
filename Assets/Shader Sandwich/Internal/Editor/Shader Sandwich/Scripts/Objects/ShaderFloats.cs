public class Float{
	public Float2 xx{get{return new Float2(x,x);}}
	public Float3 xxx{get{return new Float3(x,x,x);}}
	public Float4 xxxx{get{return new Float4(x,x,x,x);}}
	public float x;
	public Float(float xx){
		x = xx;
	}
	public Float(Float xx){
		x = xx.x;
	}
	public Float Mul(float xx){
		x*=xx;
		return this;
	}
	public Float Add(float xx){
		x+=xx;
		return this;
	}
	public Float Div(float xx){
		x/=xx;
		return this;
	}
	public Float Sub(float xx){
		x-=xx;
		return this;
	}
	public Float Square(){
		x*=x;
		return this;
	}
	public static Float operator +(Float a, Float b){
		return (a.x+b.x);
	}
	public static Float operator -(Float a, Float b){
		return (a.x-b.x);
	}
	public static Float operator *(Float a, Float b){
		return (a.x*b.x);
	}
	public static Float operator /(Float a, Float b){
		return (a.x/b.x);
	}
	/*public static Float4 operator *(Float a, Float4 b){
		return new Float4(a.x*b.x,a.x*b.y,a.x*b.z,a.x*b.w);
	}
	public static Float3 operator *(Float a, Float3 b){
		return new Float3(a.x*b.x,a.x*b.y,a.x*b.z);
	}
	public static Float2 operator *(Float a, Float2 b){
		return new Float2(a.x*b.x,a.x*b.y);
	}
	public static Float4 operator /(Float a, Float4 b){
		return new Float4(a.x/b.x,a.x/b.y,a.x/b.z,a.x/b.w);
	}
	public static Float3 operator /(Float a, Float3 b){
		return new Float3(a.x/b.x,a.x/b.y,a.x/b.z);
	}
	public static Float2 operator /(Float a, Float2 b){
		return new Float2(a.x/b.x,a.x/b.y);
	}
	public static Float4 operator +(Float a, Float4 b){
		return new Float4(a.x+b.x,a.x+b.y,a.x+b.z,a.x+b.w);
	}
	public static Float3 operator +(Float a, Float3 b){
		return new Float3(a.x+b.x,a.x+b.y,a.x+b.z);
	}
	public static Float2 operator +(Float a, Float2 b){
		return new Float2(a.x+b.x,a.x+b.y);
	}
	public static Float4 operator -(Float a, Float4 b){
		return new Float4(a.x-b.x,a.x-b.y,a.x-b.z,a.x-b.w);
	}
	public static Float3 operator -(Float a, Float3 b){
		return new Float3(a.x-b.x,a.x-b.y,a.x-b.z);
	}
	public static Float2 operator -(Float a, Float2 b){
		return new Float2(a.x-b.x,a.x-b.y);
	}
	
	
	public static Float4 operator *(Float4 b,Float a){
		return new Float4(a.x*b.x,a.x*b.y,a.x*b.z,a.x*b.w);
	}
	public static Float3 operator *(Float3 b,Float a){
		return new Float3(a.x*b.x,a.x*b.y,a.x*b.z);
	}
	public static Float2 operator *(Float2 b,Float a){
		return new Float2(a.x*b.x,a.x*b.y);
	}
	public static Float4 operator /( Float4 b,Float a){
		return new Float4(a.x/b.x,a.x/b.y,a.x/b.z,a.x/b.w);
	}
	public static Float3 operator /(Float3 b,Float a){
		return new Float3(a.x/b.x,a.x/b.y,a.x/b.z);
	}
	public static Float2 operator /(Float2 b,Float a){
		return new Float2(a.x/b.x,a.x/b.y);
	}
	public static Float4 operator +(Float4 b,Float a){
		return new Float4(a.x+b.x,a.x+b.y,a.x+b.z,a.x+b.w);
	}
	public static Float3 operator +(Float3 b,Float a){
		return new Float3(a.x+b.x,a.x+b.y,a.x+b.z);
	}
	public static Float2 operator +(Float2 b,Float a){
		return new Float2(a.x+b.x,a.x+b.y);
	}
	public static Float4 operator -(Float4 b,Float a){
		return new Float4(a.x-b.x,a.x-b.y,a.x-b.z,a.x-b.w);
	}
	public static Float3 operator -(Float3 b,Float a){
		return new Float3(a.x-b.x,a.x-b.y,a.x-b.z);
	}
	public static Float2 operator -(Float2 b,Float a){
		return new Float2(a.x-b.x,a.x-b.y);
	}*/
	
	
	
	public static implicit operator float (Float SS){
		return SS.x;
	}
	public static implicit operator Float (float SS){
		return new Float(SS);
	}
	/*public static implicit operator Float (Float2 SS){
		return new Float(SS.x);
	}
	public static implicit operator Float (Float3 SS){
		return new Float(SS.x);
	}
	public static implicit operator Float (Float4 SS){
		return new Float(SS.x);
	}*/
}
public class Float2{
	public float y;
	public float x;
	
	public Float2 yy{get{return new Float2(y,y);}}
	public Float2 yx{get{return new Float2(y,x);}}
	public Float2 xy{get{return new Float2(x,y);}}
	public Float2 xx{get{return new Float2(x,x);}}
	
	public Float3 yyy{get{return new Float3(y,y,y);}}
	public Float3 yyx{get{return new Float3(y,y,x);}}
	public Float3 yxx{get{return new Float3(y,x,x);}}
	public Float3 xyx{get{return new Float3(x,y,x);}}
	public Float3 yxy{get{return new Float3(y,x,y);}}
	public Float3 xyy{get{return new Float3(x,y,y);}}
	public Float3 xxy{get{return new Float3(x,x,y);}}
	public Float3 xxx{get{return new Float3(x,x,x);}}
	
	public Float4 yyyy{get{return new Float4(y,y,y,y);}}
	public Float4 xxyx{get{return new Float4(x,x,y,x);}}
	public Float4 xyxx{get{return new Float4(x,y,x,x);}}
	public Float4 yxxx{get{return new Float4(y,x,x,x);}}
	public Float4 xxxy{get{return new Float4(x,x,x,y);}}
	public Float4 xyyx{get{return new Float4(x,y,y,x);}}
	public Float4 yxyx{get{return new Float4(y,x,y,x);}}
	public Float4 xyxy{get{return new Float4(x,y,x,y);}}
	public Float4 yyxx{get{return new Float4(y,y,x,x);}}
	public Float4 yxxy{get{return new Float4(y,x,x,y);}}
	public Float4 xxyy{get{return new Float4(x,x,y,y);}}
	public Float4 yyyx{get{return new Float4(y,y,y,x);}}
	public Float4 yyxy{get{return new Float4(y,y,x,y);}}
	public Float4 yxyy{get{return new Float4(y,x,y,y);}}
	public Float4 xyyy{get{return new Float4(x,y,y,y);}}
	public Float4 xxxx{get{return new Float4(x,x,x,x);}}
	
	
	public Float2(float xx,float yy){
		x = xx;
		y = yy;
	}
	public Float2(float xx){
		x = xx;
		y = xx;
	}
	
	public Float2 Mul(Float2 xx){
		x*=xx.x;
		y*=xx.y;
		return this;
	}
	public Float2 Add(Float2 xx){
		x+=xx.x;
		y+=xx.y;
		return this;
	}
	public Float2 Div(Float2 xx){
		x/=xx.x;
		y/=xx.y;
		return this;
	}
	public Float2 Sub(Float2 xx){
		x-=xx.x;
		y-=xx.y;
		return this;
	}
	public Float2 Sub(float xx){
		x-=xx;
		y-=xx;
		return this;
	}
	public Float2 Add(float xx){
		x+=xx;
		y+=xx;
		return this;
	}
	/*public Float2(Float xx,Float yy){
		x = xx.x;
		y = yy.x;
	}
	public Float2(Float xx){
		x = xx.x;
		y = xx.x;
	}*/
	public Float2(Float2 xx){
		x = xx.x;
		y = xx.y;
	}
	public Float2 Square(){
		x*=x;
		y*=y;
		return this;
	}	
	
	public static Float2 operator +(Float2 a, Float2 b){
		return new Float2(a.x+b.x,a.y+b.y);
	}
	public static Float2 operator -(Float2 a, Float2 b){
		return new Float2(a.x-b.x,a.y-b.y);
	}
	public static Float2 operator *(Float2 a, Float2 b){
		return new Float2(a.x*b.x,a.y*b.y);
	}
	public static Float2 operator /(Float2 a, Float2 b){
		return new Float2(a.x/b.x,a.y/b.y);
	}
	public static Float2 operator *(Float a, Float2 b){
		return new Float2(a.x*b.x,a.x*b.y);
	}
	public static Float2 operator /(Float a, Float2 b){
		return new Float2(a.x/b.x,a.x/b.y);
	}
	public static Float2 operator +(Float a, Float2 b){
		return new Float2(a.x+b.x,a.x+b.y);
	}
	public static Float2 operator -(Float a, Float2 b){
		return new Float2(a.x-b.x,a.x-b.y);
	}
	
	
	public static Float2 operator *(Float2 b,Float a){
		return new Float2(b.x*a.x,b.y*a.x);
	}
	public static Float2 operator /( Float2 b,Float a){
		return new Float2(b.x/a.x,b.y/a.x);
	}
	public static Float2 operator +(Float2 b,Float a){
		return new Float2(b.x+a.x,b.y+a.x);
	}
	public static Float2 operator -(Float2 b,Float a){
		return new Float2(b.x-a.x,b.y-a.x);
	}
}
public class Float3{
	public float x;
	public float y;
	public float z;
	public Float3(float xx){
		x = xx;
		y = xx;
		z = xx;
	}
	public Float3(float xx,float yy,float zz){
		x = xx;
		y = yy;
		z = zz;
	}
	public Float3(Float xx){
		x = xx.x;
		y = xx.x;
		z = xx.x;
	}
	public Float3(Float xx,Float yy,Float zz){
		x = xx.x;
		y = yy.x;
		z = zz.x;
	}
	
	
	
	public Float3 Mul(Float3 xx){
		x*=xx.x;
		y*=xx.y;
		z*=xx.z;
		return this;
	}
	public Float3 Add(Float3 xx){
		x+=xx.x;
		y+=xx.y;
		z+=xx.z;
		return this;
	}
	public Float3 Add(float xx, float yy,float zz){
		x+=xx;
		y+=yy;
		z+=zz;
		return this;
	}
	public Float3 Div(Float3 xx){
		x/=xx.x;
		y/=xx.y;
		z/=xx.z;
		return this;
	}
	public Float3 Sub(Float3 xx){
		x-=xx.x;
		y-=xx.y;
		z-=xx.z;
		return this;
	}
	public Float3 Mul(float xx){
		x*=xx;
		y*=xx;
		z*=xx;
		return this;
	}
	public Float3 Add(float xx){
		x+=xx;
		y+=xx;
		z+=xx;
		return this;
	}
	public Float3 Div(float xx){
		x/=xx;
		y/=xx;
		z/=xx;
		return this;
	}
	public Float3 Sub(float xx){
		x-=xx;
		y-=xx;
		z-=xx;
		return this;
	}
	public Float3 Square(){
		x*=x;
		y*=y;
		z*=z;
		return this;
	}
	
	public static Float3 operator +(Float3 a, Float3 b){
		return new Float3(a.x+b.x,a.y+b.y,a.z+b.z);
	}
	public static Float3 operator -(Float3 a, Float3 b){
		return new Float3(a.x-b.x,a.y-b.y,a.z-b.z);
	}
	public static Float3 operator *(Float3 a, Float3 b){
		return new Float3(a.x*b.x,a.y*b.y,a.z*b.z);
	}
	public static Float3 operator /(Float3 a, Float3 b){
		return new Float3(a.x/b.x,a.y/b.y,a.z/b.z);
	}
	public static Float3 operator *(Float a, Float3 b){
		return new Float3(a.x*b.x,a.x*b.y,a.x*b.z);
	}
	public static Float3 operator /(Float a, Float3 b){
		return new Float3(a.x/b.x,a.x/b.y,a.x/b.z);
	}
	public static Float3 operator +(Float a, Float3 b){
		return new Float3(a.x+b.x,a.x+b.y,a.x+b.z);
	}
	public static Float3 operator -(Float a, Float3 b){
		return new Float3(a.x-b.x,a.x-b.y,a.x-b.z);
	}
	
	
	public static Float3 operator *(Float3 b,Float a){
		return new Float3(a.x*b.x,a.x*b.y,a.x*b.z);
	}
	public static Float3 operator /( Float3 b,Float a){
		return new Float3(a.x/b.x,a.x/b.y,a.x/b.z);
	}
	public static Float3 operator +(Float3 b,Float a){
		return new Float3(a.x+b.x,a.x+b.y,a.x+b.z);
	}
	public static Float3 operator -(Float3 b,Float a){
		return new Float3(a.x-b.x,a.x-b.y,a.x-b.z);
	}
}
public class Float4{
	public float x;
	public float y;
	public float z;
	public float w;
	
	public Float2 yy{get{return new Float2(y,y);}}
	public Float2 yx{get{return new Float2(y,x);}}
	public Float2 xy{get{return new Float2(x,y);} set{x = value.x; y =value.y;}}
	public Float2 xx{get{return new Float2(x,x);}}
	
	public Float2 ww{get{return new Float2(w,w);}}
	public Float2 wz{get{return new Float2(w,z);}}
	public Float2 zw{get{return new Float2(z,w);}}
	
	public Float2 yz{get{return new Float2(y,z);}}
	public Float2 xz{get{return new Float2(x,z);}}
	
	public Float2 yw{get{return new Float2(y,w);}}
	public Float2 xw{get{return new Float2(x,w);}}
	
	public Float2 zz{get{return new Float2(z,z);}}
	public Float2 zy{get{return new Float2(z,y);}}
	public Float2 zx{get{return new Float2(z,x);}}
	
	public Float2 wy{get{return new Float2(w,y);}}
	public Float2 wx{get{return new Float2(w,x);}}
	
	public Float3 yyy{get{return new Float3(y,y,y);}}
	public Float3 yyx{get{return new Float3(y,y,x);}}
	public Float3 yxx{get{return new Float3(y,x,x);}}
	public Float3 xyx{get{return new Float3(x,y,x);}}
	public Float3 yxy{get{return new Float3(y,x,y);}}
	public Float3 xyy{get{return new Float3(x,y,y);}}
	public Float3 xxy{get{return new Float3(x,x,y);}}
	public Float3 xxx{get{return new Float3(x,x,x);}}
	
	public Float4 yyyy{get{return new Float4(y,y,y,y);}}
	public Float4 xxyx{get{return new Float4(x,x,y,x);}}
	public Float4 xyxx{get{return new Float4(x,y,x,x);}}
	public Float4 yxxx{get{return new Float4(y,x,x,x);}}
	public Float4 xxxy{get{return new Float4(x,x,x,y);}}
	public Float4 xyyx{get{return new Float4(x,y,y,x);}}
	public Float4 yxyx{get{return new Float4(y,x,y,x);}}
	public Float4 xyxy{get{return new Float4(x,y,x,y);}}
	public Float4 yyxx{get{return new Float4(y,y,x,x);}}
	public Float4 yxxy{get{return new Float4(y,x,x,y);}}
	public Float4 xxyy{get{return new Float4(x,x,y,y);}}
	public Float4 yyyx{get{return new Float4(y,y,y,x);}}
	public Float4 yyxy{get{return new Float4(y,y,x,y);}}
	public Float4 yxyy{get{return new Float4(y,x,y,y);}}
	public Float4 xyyy{get{return new Float4(x,y,y,y);}}
	public Float4 xxxx{get{return new Float4(x,x,x,x);}}
	
	public Float4 yyyz{get{return new Float4(y,y,y,z);}}
	public Float4 xxyz{get{return new Float4(x,x,y,z);}}
	public Float4 xyxz{get{return new Float4(x,y,x,z);}}
	public Float4 yxxz{get{return new Float4(y,x,x,z);}}
	public Float4 xxxz{get{return new Float4(x,x,x,z);}}
	public Float4 xyyz{get{return new Float4(x,y,y,z);}}
	public Float4 yxyz{get{return new Float4(y,x,y,z);}}
	public Float4 yyxz{get{return new Float4(y,y,x,z);}}
	
	public Float4 yzyz{get{return new Float4(y,z,y,z);}}
	public Float4 xzyz{get{return new Float4(x,z,y,z);}}
	public Float4 xzxz{get{return new Float4(x,z,x,z);}}
	public Float4 yzxz{get{return new Float4(y,z,x,z);}}
	
	public Float4 yyzy{get{return new Float4(y,y,z,y);}}
	public Float4 xxzx{get{return new Float4(x,x,z,x);}}
	public Float4 xyzx{get{return new Float4(x,y,z,x);}}
	public Float4 yxzx{get{return new Float4(y,x,z,x);}}
	public Float4 xxzy{get{return new Float4(x,x,z,y);}}
	public Float4 xyzy{get{return new Float4(x,y,z,y);}}
	public Float4 yyzx{get{return new Float4(y,y,z,x);}}
	public Float4 yxzy{get{return new Float4(y,x,z,y);}}
	
	public Float4 yzyy{get{return new Float4(y,z,y,y);}}
	public Float4 xzyx{get{return new Float4(x,z,y,x);}}
	public Float4 xzxx{get{return new Float4(x,z,x,x);}}
	public Float4 yzxx{get{return new Float4(y,z,x,x);}}
	public Float4 xzxy{get{return new Float4(x,z,x,y);}}
	public Float4 yzyx{get{return new Float4(y,z,y,x);}}
	
	public Float4 yyww{get{return new Float4(y,y,w,w);}}
	public Float4 wwyy{get{return new Float4(w,w,y,y);}}
	public Float4 zxzx{get{return new Float4(z,x,z,x);}}
	
	
	
	
	public Float4(Float xx){
		x = xx.x;
		y = xx.x;
		z = xx.x;
		w = xx.x;
	}
	public Float4(Float xx,Float yy,Float zz,Float ww){
		x = xx.x;
		y = yy.x;
		z = zz.x;
		w = ww.x;
	}
	public Float4(Float2 xx,Float2 yy){
		x = xx.x;
		y = xx.y;
		z = yy.x;
		w = yy.y;
	}
	
	
	public Float4 Mul(Float4 xx){
		x*=xx.x;
		y*=xx.y;
		z*=xx.z;
		w*=xx.w;
		return this;
	}
	public Float4 Add(Float4 xx){
		x+=xx.x;
		y+=xx.y;
		z+=xx.z;
		w+=xx.w;
		return this;
	}
	public Float4 Add(float xx, float yy,float zz,float ww){
		x+=xx;
		y+=yy;
		z+=zz;
		w+=ww;
		return this;
	}
	public Float4 Div(Float4 xx){
		x/=xx.x;
		y/=xx.y;
		z/=xx.z;
		w/=xx.w;
		return this;
	}
	public Float4 Sub(Float4 xx){
		x-=xx.x;
		y-=xx.y;
		z-=xx.z;
		w-=xx.w;
		return this;
	}
	public Float4 Mul(float xx){
		x*=xx;
		y*=xx;
		z*=xx;
		w*=xx;
		return this;
	}
	public Float4 Add(float xx){
		x+=xx;
		y+=xx;
		z+=xx;
		w+=xx;
		return this;
	}
	public Float4 Div(float xx){
		x/=xx;
		y/=xx;
		z/=xx;
		w/=xx;
		return this;
	}
	public Float4 Sub(float xx){
		x-=xx;
		y-=xx;
		z-=xx;
		w-=xx;
		return this;
	}
	public Float4 Square(){
		x*=x;
		y*=y;
		z*=z;
		w*=w;
		return this;
	}

	
	public static Float4 operator +(Float4 a, Float4 b){
		return new Float4(a.x+b.x,a.y+b.y,a.z+b.z,a.w+b.w);
	}
	public static Float4 operator -(Float4 a, Float4 b){
		return new Float4(a.x-b.x,a.y-b.y,a.z-b.z,a.w-b.w);
	}
	public static Float4 operator *(Float4 a, Float4 b){
		return new Float4(a.x*b.x,a.y*b.y,a.z*b.z,a.w*b.w);
	}
	public static Float4 operator /(Float4 a, Float4 b){
		return new Float4(a.x/b.x,a.y/b.y,a.z/b.z,a.w/b.w);
	}
	public static Float4 operator *(Float a, Float4 b){
		return new Float4(a.x*b.x,a.x*b.y,a.x*b.z,a.x*b.w);
	}
	public static Float4 operator /(Float a, Float4 b){
		return new Float4(a.x/b.x,a.x/b.y,a.x/b.z,a.x/b.w);
	}
	public static Float4 operator +(Float a, Float4 b){
		return new Float4(a.x+b.x,a.x+b.y,a.x+b.z,a.x+b.w);
	}
	public static Float4 operator -(Float a, Float4 b){
		return new Float4(a.x-b.x,a.x-b.y,a.x-b.z,a.x-b.w);
	}
	
	
	public static Float4 operator *(Float4 b,Float a){
		return new Float4(a.x*b.x,a.x*b.y,a.x*b.z,a.x*b.w);
	}
	public static Float4 operator /( Float4 b,Float a){
		return new Float4(a.x/b.x,a.x/b.y,a.x/b.z,a.x/b.w);
	}
	public static Float4 operator +(Float4 b,Float a){
		return new Float4(a.x+b.x,a.x+b.y,a.x+b.z,a.x+b.w);
	}
	public static Float4 operator -(Float4 b,Float a){
		return new Float4(a.x-b.x,a.x-b.y,a.x-b.z,a.x-b.w);
	}
}