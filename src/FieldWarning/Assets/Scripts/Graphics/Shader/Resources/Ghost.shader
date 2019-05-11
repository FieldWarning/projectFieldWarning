﻿Shader "Custom/Ghost" {
Properties {
    _Color ("Main Color", Color) = (1,1,1,1)
    _SpecColor ("Spec Color", Color) = (1,1,1,0)
    _Emission ("Emissive Color", Color) = (0,0,0,0)
    _Shininess ("Shininess", Range (0.1, 1)) = 0.7
    _MainTex ("Base (RGB) Trans (A)", 2D) = "white" {}
}
 
SubShader {
    Tags {"RenderType"="Transparent" "Queue"="Transparent"}
    // Render into depth buffer only
    Pass {		
		Lighting Off
        ColorMask 0		
    }

    // Render normally
    Pass {
        ZWrite Off
        Blend SrcAlpha OneMinusSrcAlpha
        ColorMask RGB
        Material {
            Diffuse [_Color]
            Ambient [_Color]
            Shininess [_Shininess]
            Specular [_SpecColor]
            Emission [_Emission]
        }
        Lighting On
        SetTexture [_MainTex] {
            Combine texture * primary DOUBLE, texture * primary
        }
    }
}
}
