using UnityEngine;
using System.Collections;

public class GUIControl : MonoBehaviour
{
    public GameObject music;
    public Texture muteEnabledTexture;
    public Texture muteDisabledTexture;
    public Texture p1;
    public Texture p2;
    public Texture p1Break;
    public Texture p2Break;
    private Texture currentTex;

    private static Texture2D _staticRectTexture;
    private static GUIStyle _staticRectStyle;

    private static float health1;
    private static float health2;
    private static bool mute = false;
    private static int mode = 0;
    // 1: P1 Win
    // 2: P2 Win
    // 3: P1 Break
    // 4: P2 Break

	// Use this for initialization
	void Awake ()
    {
	   currentTex = muteDisabledTexture;
	}
	
    void OnGUI()
    {
        int r = (int) Mathf.Round(Mathf.Min((100.0f - health2) / 50.0f * 255.0f, 255.0f));
        int g = 255 - (int)Mathf.Round(Mathf.Max(Mathf.Min((50.0f - health2) / 50.0f * 255.0f, 255.0f), 0.0f));
        GUIDrawRect(new Rect(10.0f, 10.0f, health2 * 2.0f, 16.0f), new Color(r, g, 0.0f, 256.0f));

        r = (int) Mathf.Round(Mathf.Min((100.0f - health1) / 50.0f * 255.0f, 255.0f));
        g = 255 - (int)Mathf.Round(Mathf.Max(Mathf.Min((50.0f - health1) / 50.0f * 255.0f, 255.0f), 0.0f));
        GUIDrawRect(new Rect(Screen.width * 0.505f + 10.0f, 10.0f, health1 * 2.0f, 16.0f), new Color(r, g, 0.0f, 256.0f));

        if (GUI.Button(new Rect(Screen.width - 38.0f, 5.0f, 33.0f, 33.0f), currentTex, ""))
        {
            //music.Toggle();
            mute = !mute;
            if (currentTex == muteEnabledTexture) currentTex = muteDisabledTexture;
            else currentTex = muteEnabledTexture;
        }

        if (mode > 0)
        {
            // Make everything black
            GUIDrawRect(new Rect(0.0f, 0.0f, Screen.width, Screen.height), new Color(0.05f, 0.05f, 0.05f, 1.0f));

            switch (mode)
            {
            case 1:
                GUI.Box(new Rect(Screen.width / 2.0f - 225.0f, Screen.height / 2.0f - 22.5f, 450.0f, 45.0f), p1);
                break;
            case 2:
                GUI.Box(new Rect(Screen.width / 2.0f - 225.0f, Screen.height / 2.0f - 22.5f, 450.0f, 45.0f), p2);
                break;
            case 3:
                GUI.Box(new Rect(Screen.width / 2.0f - 255.0f, Screen.height / 2.0f - 21.0f, 510.0f, 42.0f), p1Break);
                break;
            case 4:
                GUI.Box(new Rect(Screen.width / 2.0f - 255.0f, Screen.height / 2.0f - 21.0f, 510.0f, 42.0f), p2Break);
                break;
            }
        }
    }

    public void SupplyHealth1(float health1New)
    {
        health1 = Mathf.Max(health1New, 0.0f);
    }

    public void SupplyHealth2(float health2New)
    {
        health2 = Mathf.Max(health2New, 0.0f);
    }

    public void SetMode(int newMode)
    {
        mode = newMode;
    }

    // Note that this function is only meant to be called from OnGUI() functions.
    private static void GUIDrawRect( Rect position, Color color )
    {
        if( _staticRectTexture == null )
        {
            _staticRectTexture = new Texture2D( 1, 1 );
        }
 
        if( _staticRectStyle == null )
        {
            _staticRectStyle = new GUIStyle();
        }
 
        _staticRectTexture.SetPixel( 0, 0, color );
        _staticRectTexture.Apply();
 
        _staticRectStyle.normal.background = _staticRectTexture;
 
        GUI.Box( position, GUIContent.none, _staticRectStyle ); 
    }
}
