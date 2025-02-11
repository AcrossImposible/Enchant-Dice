Shader "Custom/ProceduralCausticsTransparentGradientThinBand"
{
    Properties
    {
        // Цвет для минимальной интенсивности (фон)
        _LowColor("Low Color", Color) = (0, 0, 0, 1)
        // Цвет для максимальной интенсивности (линии каустики)
        _HighColor("High Color", Color) = (1, 1, 1, 1)
        // Частота базовых синусоидальных волн
        _Frequency("Frequency", Float) = 10.0
        // Скорость анимации
        _Speed("Speed", Float) = 1.0
        // Масштаб UV-координат (для зума паттерна)
        _Scale("UV Scale", Float) = 1.0
        // Порог, вокруг которого формируются яркие линии
        _Threshold("Threshold", Range(0,1)) = 0.5
        // Интенсивность шума (насколько сильно шум влияет на итоговый паттерн)
        _NoiseIntensity("Noise Intensity", Float) = 0.5
        // Общая прозрачность каустики (1 - линии полностью непрозрачные)
        _CausticsAlpha("Caustics Transparency", Range(0,1)) = 1.0
        // Параметр, контролирующий толщину (ширину) линий каустики.
        // Меньшие значения – тоньше линии, большие – шире.
        _CausticsThickness("Caustics Thickness", Range(0.001, 0.5)) = 0.05
    }
        SubShader
    {
        // Указываем, что шейдер прозрачный
        Tags { "Queue" = "Transparent" "RenderType" = "Transparent" }
        LOD 100

        Pass
        {
            // Настройка прозрачного рендеринга
            Blend SrcAlpha OneMinusSrcAlpha
            ZWrite Off

            CGPROGRAM
        // Объявляем вершинный и фрагментный шейдеры
        #pragma vertex vert
        #pragma fragment frag
        #include "UnityCG.cginc"

        // Входная структура для вершинного шейдера
        struct appdata {
            float4 vertex : POSITION;
            float2 uv : TEXCOORD0;
        };

    // Структура, передающая данные во фрагментный шейдер
    struct v2f {
        float2 uv : TEXCOORD0;
        float4 vertex : SV_POSITION;
    };

    // Параметры, задаваемые из инспектора
    fixed4 _LowColor;
    fixed4 _HighColor;
    float _Frequency;
    float _Speed;
    float _Scale;
    float _Threshold;
    float _NoiseIntensity;
    float _CausticsAlpha;
    float _CausticsThickness;

    // Функция-хэш для генерации псевдослучайных чисел
    float hash(float2 p)
    {
        return frac(sin(dot(p, float2(127.1, 311.7))) * 43758.5453123);
    }

    // Функция генерации 2D-шума
    float noise2d(float2 p)
    {
        float2 i = floor(p);
        float2 f = frac(p);

        // Значения для углов
        float a = hash(i);
        float b = hash(i + float2(1.0, 0.0));
        float c = hash(i + float2(0.0, 1.0));
        float d = hash(i + float2(1.0, 1.0));

        // Интерполяция по обоим направлениям
        float2 u = f * f * (3.0 - 2.0 * f);
        return lerp(a, b, u.x) + (c - a) * u.y * (1.0 - u.x) + (d - b) * u.x * u.y;
    }

    // Вершинный шейдер: преобразование координат и масштабирование UV
    v2f vert(appdata v)
    {
        v2f o;
        o.vertex = UnityObjectToClipPos(v.vertex);
        o.uv = v.uv * _Scale;
        return o;
    }

    // Фрагментный шейдер: вычисление эффекта каустики с градиентом цвета и настраиваемой толщиной линий
    fixed4 frag(v2f i) : SV_Target
    {
        float time = _Time.y * _Speed;
        float2 uv = i.uv;

        // Создаём базовый интерференционный паттерн из нескольких синусоидальных волн
        float pattern = 0.0;
        pattern += sin(dot(uv, float2(1, 0)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0, 1)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0.7071, 0.7071)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0.7071, -0.7071)) * _Frequency + time);
        pattern /= 4.0;
        // Приводим диапазон значений от [-1,1] к [0,1]
        pattern = (pattern + 1.0) * 0.5;

        // Генерация дополнительного шума
        float noiseValue = noise2d(uv * _Frequency * 0.5 + time);

        // Комбинируем базовый паттерн и шум с учетом интенсивности шума
        float combined = saturate(pattern + noiseValue * _NoiseIntensity);

        // Для получения узких линий используем разность двух smoothstep.
        // Функция lower быстро возрастает от 0 до 1 при подходе к _Threshold,
        // а функция upper убывает от 1 до 0 сразу после _Threshold.
        // Разность (lower - upper) даст пик (значение 1) ровно в _Threshold, а ширина этого пика контролируется _CausticsThickness.
        float lower = smoothstep(_Threshold - _CausticsThickness, _Threshold, combined);
        float upper = smoothstep(_Threshold, _Threshold + _CausticsThickness, combined);
        float caustics = lower - upper;

        // Градиент цвета: интерполируем между _LowColor и _HighColor по значению caustics
        fixed4 colorOut = lerp(_LowColor, _HighColor, caustics);
        // Прозрачность каждой линии определяется отдельно (_CausticsAlpha) и умножается на вычисленное caustics (где caustics = 1 – линия полностью видна)
        colorOut.a = caustics * _CausticsAlpha;

        return colorOut;
    }
    ENDCG
}
    }
        FallBack "Transparent/VertexLit"
}
