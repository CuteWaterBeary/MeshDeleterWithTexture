﻿Shader "Unlit/TextureEdit"
{
	Properties
	{
		_MainTex ("Texture", 2D) = "white" {}
		_Color ("Color", Color) = (1, 1, 1, 1)
		_EditType("EditType", Float) = 0
		_Threshold ("Threshold", Float) = 1
		_TextureScale ("TextureScale", Float) = 1
		_Offset ("Offset", Vector) = (0, 0, 0, 0)

		_SecondTex ("Second Texture", 2D) = "black" {}
		_CurrentPos ("Mouse Current Pos", Vector) = (0, 0, 0, 0)
		_StartPos ("Drag Start Pos", Vector) = (0, 0, 0, 0)
		_EndPos ("Drag End Pos", Vector) = (0, 0, 0, 0)
		_LineWidth ("Line Width", Float) = 0.002
		_PenSize("Pen Size", Float) = 0.01
	}
	SubShader
	{
		Tags { "RenderType"="Opaque" }
		LOD 100

		Pass
		{
			CGPROGRAM
			#pragma vertex vert
			#pragma fragment frag
			// make fog work
			#pragma multi_compile_fog
			
			#include "UnityCG.cginc"

			struct appdata
			{
				float4 vertex : POSITION;
				float2 uv : TEXCOORD0;
			};

			struct v2f
			{
				float2 uv : TEXCOORD0;
				UNITY_FOG_COORDS(1)
				float4 vertex : SV_POSITION;
			};

			sampler2D _MainTex;
			float4 _MainTex_ST;
			float4 _MainTex_TexelSize;
			fixed4 _Color;
			float _EditType;
			float _Threshold;
			float _TextureScale;
			float4 _Offset;

			sampler2D _SecondTex;

			float4 _StartPos;
			float4 _EndPos;
			float4 _CurrentPos;

			float _LineWidth;

			float _PenSize;
			
			v2f vert (appdata v)
			{
				v2f o;
				o.vertex = UnityObjectToClipPos(v.vertex);
				o.uv = TRANSFORM_TEX(v.uv, _MainTex);
				UNITY_TRANSFER_FOG(o,o.vertex);
				return o;
			}
			
			fixed4 frag (v2f i) : SV_Target
			{
				float2 uv = (float2(0.5, 0.5) * (1-_TextureScale) + _Offset.xy * 0.5) + i.uv * _TextureScale;
				fixed4 col = tex2D(_MainTex, uv);

				col.rgb *= (1-tex2D(_SecondTex, i.uv).rgb);

				
				if ((abs(i.uv.x - _StartPos.x) <= _LineWidth || abs(i.uv.x - _EndPos.x) <= _LineWidth) && i.uv.y >= min(_StartPos.y, _EndPos.y)-_LineWidth && i.uv.y <= max(_StartPos.y, _EndPos.y)+_LineWidth ||
					(abs(i.uv.y - _StartPos.y) <= _LineWidth || abs(i.uv.y - _EndPos.y) <= _LineWidth) && i.uv.x >= min(_StartPos.x, _EndPos.x)-_LineWidth && i.uv.x <= max(_StartPos.x, _EndPos.x)+_LineWidth
				)
					col = fixed4(1, 0.7, 0, 1);

				float raito = _MainTex_TexelSize.x / _MainTex_TexelSize.y;
				if (distance (i.uv * float2(1, raito), _CurrentPos.xy * float2(1, raito)) <= _PenSize)
					col = fixed4(1, 1, 0, 1);

				return pow(col, 1/2.2);
			}
			ENDCG
		}
	}
}
