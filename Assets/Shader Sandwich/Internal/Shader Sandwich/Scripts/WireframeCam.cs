using UnityEngine;
using System.Collections;

public class WireframeCam : MonoBehaviour {

	// Use this for initialization
    void OnPreRender() {
        GL.wireframe = true;
		//GL.Color(new Color(0,0,0,0));
    }
    void OnPostRender() {
        //GL.wireframe = false;
    }
}
