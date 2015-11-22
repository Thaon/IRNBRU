using UnityEngine;
using System.Collections;

public class PauseMenu : MonoBehaviour {

     public bool paused = false;

 
     void Update()
     {
         if (Input.GetButtonDown("PauseButton"))
         {
             paused = !paused;
             TogglePause();
         }
            
     }
     
     void OnGUI()
     {
         if(paused)
         {
             GUILayout.BeginArea (new Rect((Screen.width/2)-50, (Screen.height/2), 100, 100), "<color=White><size=40>Paused</size></color>");
             //GUILayout.Label("Game is paused!");
             
             GUILayout.EndArea();

             GUILayout.BeginArea(new Rect((0) +100, (0), 100, 100), "<color=White><size=28>For Hints: Press H or A Button</size></color>");
             //GUILayout.Label("Game is paused!");

             GUILayout.EndArea();

             
            
         }
     }  
     
     public bool TogglePause()
     {
         if(Time.timeScale == 0f)
         {
             Time.timeScale = 1f;
             return(false);
         }
         else
         {
             Time.timeScale = 0f;
             return(true);    
         }
     }


    }
    

