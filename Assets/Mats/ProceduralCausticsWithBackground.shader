Shader "Custom/ProceduralCausticsWithBackground"
{
    Properties
    {
        // ���� � ������������ ��� �������� ��� �������� (���)
        _BackgroundColor("Background Color", Color) = (0, 0, 0, 1)
        // ����, ��������������� ����������� ������������� �������� (������ ����� ���������)
        _LowColor("Low Caustics Color", Color) = (0, 0, 0, 0)
        // ����, ��������������� ������������ ������������� �������� (������� ����� ���������)
        _HighColor("High Caustics Color", Color) = (1, 1, 1, 1)
        // ������� ������� �������������� ����
        _Frequency("Frequency", Float) = 10.0
        // �������� ��������
        _Speed("Speed", Float) = 1.0
        // ������� UV-��������� (��� ���� ��������)
        _Scale("UV Scale", Float) = 1.0
        // �����, ������ �������� ����������� ����� ��������
        _Threshold("Threshold", Range(0,1)) = 0.5
        // ������������� ���� (��������� ������ ��� ������ �� �������� �������)
        _NoiseIntensity("Noise Intensity", Float) = 0.5
        // �������������� ������������ �������� (1 � ����� ��������� ������������)
        _CausticsAlpha("Caustics Transparency", Range(0,1)) = 1.0
        // ��������, �������������� ������� (������) ����� ��������.
        // ������� �������� � ������ �����, ������� � ����.
        _CausticsThickness("Caustics Thickness", Range(0.001, 0.5)) = 0.05
    }
        SubShader
    {
        // �������� ��� ���������� ������
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            // ��������� ����������� ����������
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
        // ��������� ��������� � ����������� �������
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        // ������� ��������� ��� ���������� �������
        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

    // ���������, ���������� ������ �� ����������� ������
    struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    // ���������, ���������� �� ����������
    fixed4 _BackgroundColor;
    fixed4 _LowColor;
    fixed4 _HighColor;
    float _Frequency;
    float _Speed;
    float _Scale;
    float _Threshold;
    float _NoiseIntensity;
    float _CausticsAlpha;
    float _CausticsThickness;

    // �������-��� ��� ��������� ��������������� �����
    float hash(float2 p)
    {
        return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
    }

    // ������� ��������� 2D-����
    float noise2d(float2 p)
    {
        float2 i = floor(p);
        float2 f = frac(p);
        float a = hash(i);
        float b = hash(i + float2(1.0, 0.0));
        float c = hash(i + float2(0.0, 1.0));
        float d = hash(i + float2(1.0, 1.0));
        float2 u = f * f * (3.0 - 2.0 * f);
        return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
    }

    // ��������� ������: �������������� ��������� � ��������������� UV
    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv * _Scale;
        return o;
    }

    // ����������� ������: ���������� ������� �������� � ���������� ����� � ������������ �������� ���� ����
    fixed4 frag(v2f i) : SV_Target
    {
        float time = _Time.y * _Speed;
        float2 uv = i.uv;

        // ��������� ������� ����������������� ������� �� ���������� �������������� ����
        float pattern = 0.0;
        pattern += sin(dot(uv, float2(1, 0)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0, 1)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0.7071, 0.7071)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0.7071, -0.7071)) * _Frequency + time);
        pattern /= 4.0;
        // �������� �������� �������� �� [-1,1] � [0,1]
        pattern = (pattern + 1.0) * 0.5;

        // ���������� �������������� ���
        float noiseValue = noise2d(uv * _Frequency * 0.5 + time);
        float combined = saturate(pattern + noiseValue * _NoiseIntensity);

        // ��������� ����� ������ (�����) �������� ����� �������� ���� smoothstep-�������.
        // �������� _CausticsThickness ������������ ������ ���� ������.
        float lower = smoothstep(_Threshold - _CausticsThickness, _Threshold, combined);
        float upper = smoothstep(_Threshold, _Threshold + _CausticsThickness, combined);
        float caustics = lower - upper;

        // ��������� ����������� ���� ��� ����� �������� (�� _LowColor �� _HighColor)
        fixed4 causticsColor = lerp(_LowColor, _HighColor, caustics);
        // ������������ �����-����� ����� �������� � ������������ � ���������� _CausticsAlpha
        causticsColor.a *= _CausticsAlpha;

        // ��������� ���� � �������� ���������� �������� ����� � ����� �������� �� ����� caustics.
        // ���� caustics = 0, ������������ _BackgroundColor (�� ����� �����-�������),
        // ���� caustics = 1, ������������ ����������� causticsColor.
        fixed4 finalColor = lerp(_BackgroundColor, causticsColor, caustics);

        return finalColor;
    }
    ENDCG
}
    }
        FallBack "Transparent/Diffuse"
}
