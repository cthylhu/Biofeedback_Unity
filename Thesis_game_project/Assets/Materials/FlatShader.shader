﻿Shader "Custom/FlatShader" {
    Properties {
        _Color ("Main Color", Color) = (1,1,1,1)
    }
    SubShader {
        Pass { Color [_Color] }
    }
}
