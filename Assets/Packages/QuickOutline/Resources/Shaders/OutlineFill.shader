//
//  OutlineFill.shader
//  QuickOutline
//
//  Created by Chris Nolet on 2/21/18.
//  Copyright © 2018 Chris Nolet. All rights reserved.
//

Shader "Custom/Outline Fill" {
  Properties {
    [Enum(UnityEngine.Rendering.CompareFunction)] _ZTest("ZTest", Float) = 0

    _OutlineColor("Outline Color", Color) = (0, 0, 0, 1)
    _OutlineWidth("Outline Width", Range(0, 30)) = 10

    // 스텐실 비교 함수 (스크립트에서 CompareFunction.Never로 설정하여 패스 스킵 제어)
    // 이 프로퍼티는 셰이더 내부의 Stencil { Comp [_StencilComp] } 로 매핑됩니다.
    [Enum(UnityEngine.Rendering.CompareFunction)] _StencilComp("Stencil Comparison", Float) = 6

    // 아웃라인 활성화/비활성화를 위한 Float 프로퍼티 추가
    // 이 값을 1.0f로 설정하면 활성화, 0.0f로 설정하면 비활성화
    _OutlineEnabledToggle("Outline Enabled Toggle", Float) = 1

    // 스크립트에서 활성화:
    // SetFloat("_OutlineEnabledToggle", 1f);
    // SetInt(k_StencilCompID, (int)CompareFunction.NotEqual);

    // 스크립트에서 비활성화:
    // SetFloat("_OutlineEnabledToggle", 0f);
    // SetInt(k_StencilCompID, (int)CompareFunction.Never);
  }

  SubShader {
    Tags {
      "Queue" = "Transparent+110"
      "RenderType" = "Transparent"
      "DisableBatching" = "True"
    }

    Pass {
      Name "Fill"
      Cull Off
      ZTest LEqual
      ZWrite Off
      Blend SrcAlpha OneMinusSrcAlpha
      ColorMask RGB

      Stencil {
        Ref 1
        Comp [_StencilComp] // 스크립트에서 설정된 _StencilComp 값 사용
      }

      CGPROGRAM
      #include "UnityCG.cginc"

      #pragma vertex vert
      #pragma fragment frag

      struct appdata {
        float4 vertex : POSITION;
        float3 normal : NORMAL;
        float3 smoothNormal : TEXCOORD3;
        UNITY_VERTEX_INPUT_INSTANCE_ID
      };

      struct v2f {
        float4 position : SV_POSITION;
        fixed4 color : COLOR;
        UNITY_VERTEX_OUTPUT_STEREO
      };

      uniform fixed4 _OutlineColor;
      uniform float _OutlineWidth;
      uniform float _OutlineEnabledToggle;

      v2f vert(appdata input) {
        v2f output;

        UNITY_SETUP_INSTANCE_ID(input);
        UNITY_INITIALIZE_VERTEX_OUTPUT_STEREO(output);

        // _OutlineEnabledToggle 값이 0.5보다 크면 아웃라인 활성화 로직 실행
        if (_OutlineEnabledToggle > 0.5) 
        {
          float3 normal = any(input.smoothNormal) ? input.smoothNormal : input.normal;
          float3 viewPosition = UnityObjectToViewPos(input.vertex);
          float3 viewNormal = normalize(mul((float3x3)UNITY_MATRIX_IT_MV, normal));

          output.position = UnityViewToClipPos(viewPosition + viewNormal * -viewPosition.z * _OutlineWidth / 1000.0);
          output.color = _OutlineColor;
        }
        else // 비활성화 시: 최소한의 정점 위치 계산만 수행
        {
          output.position = UnityObjectToClipPos(input.vertex);
          output.color = fixed4(0,0,0,0); // 색상은 사용되지 않지만, 오류 방지를 위해 초기화
        }

        return output;
      }

      fixed4 frag(v2f input) : SV_Target {
        // _OutlineEnabledToggle 값이 0.5보다 크면 아웃라인 색상 반환
        if (_OutlineEnabledToggle > 0.5) 
        {
          return input.color;
        }
        else // 비활성화 시: 픽셀 버림
        {
          discard;
          return fixed4(0,0,0,0);
        }
      }
      ENDCG
    }
  }
}
