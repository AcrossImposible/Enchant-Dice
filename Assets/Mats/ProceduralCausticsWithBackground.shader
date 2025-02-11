Shader "Custom/ProceduralCausticsWithBackground"
{
    Properties
    {
        // Цвет и прозрачность для областей без каустики (фон)
        _BackgroundColor("Background Color", Color) = (0, 0, 0, 1)
        // Цвет, соответствующий минимальной интенсивности каустики (нижняя точка градиента)
        _LowColor("Low Caustics Color", Color) = (0, 0, 0, 0)
        // Цвет, соответствующий максимальной интенсивности каустики (верхняя точка градиента)
        _HighColor("High Caustics Color", Color) = (1, 1, 1, 1)
        // Частота базовых синусоидальных волн
        _Frequency("Frequency", Float) = 10.0
        // Скорость анимации
        _Speed("Speed", Float) = 1.0
        // Масштаб UV-координат (для зума паттерна)
        _Scale("UV Scale", Float) = 1.0
        // Порог, вокруг которого формируются линии каустики
        _Threshold("Threshold", Range(0,1)) = 0.5
        // Интенсивность шума (насколько сильно шум влияет на итоговый паттерн)
        _NoiseIntensity("Noise Intensity", Float) = 0.5
        // Дополнительная прозрачность каустики (1 – линии полностью непрозрачные)
        _CausticsAlpha("Caustics Transparency", Range(0,1)) = 1.0
        // Параметр, контролирующий толщину (ширину) линий каустики.
        // Меньшие значения – тоньше линии, большие – шире.
        _CausticsThickness("Caustics Thickness", Range(0.001, 0.5)) = 0.05
    }
        SubShader
    {
        // Рендерим как прозрачный объект
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
        float a = hash(i);
        float b = hash(i + float2(1.0, 0.0));
        float c = hash(i + float2(0.0, 1.0));
        float d = hash(i + float2(1.0, 1.0));
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

    // Фрагментный шейдер: вычисление эффекта каустики с градиентом цвета и возможностью задавать цвет фона
    fixed4 frag(v2f i) : SV_Target
    {
        float time = _Time.y * _Speed;
        float2 uv = i.uv;

        // Вычисляем базовый интерференционный паттерн из нескольких синусоидальных волн
        float pattern = 0.0;
        pattern += sin(dot(uv, float2(1, 0)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0, 1)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0.7071, 0.7071)) * _Frequency + time);
        pattern += sin(dot(uv, float2(0.7071, -0.7071)) * _Frequency + time);
        pattern /= 4.0;
        // Приводим диапазон значений от [-1,1] к [0,1]
        pattern = (pattern + 1.0) * 0.5;

        // Генерируем дополнительный шум
        float noiseValue = noise2d(uv * _Frequency * 0.5 + time);
        float combined = saturate(pattern + noiseValue * _NoiseIntensity);

        // Вычисляем узкую полосу (линии) каустики через разность двух smoothstep-функций.
        // Параметр _CausticsThickness контролирует ширину этой полосы.
        float lower = smoothstep(_Threshold - _CausticsThickness, _Threshold, combined);
        float upper = smoothstep(_Threshold, _Threshold + _CausticsThickness, combined);
        float caustics = lower - upper;

        // Вычисляем градиентный цвет для линий каустики (от _LowColor до _HighColor)
        fixed4 causticsColor = lerp(_LowColor, _HighColor, caustics);
        // Модифицируем альфа-канал линий каустики в соответствии с параметром _CausticsAlpha
        causticsColor.a *= _CausticsAlpha;

        // Финальный цвет — линейное смешивание фонового цвета и цвета каустики по маске caustics.
        // Если caustics = 0, используется _BackgroundColor (со своим альфа-каналом),
        // если caustics = 1, используется вычисленный causticsColor.
        fixed4 finalColor = lerp(_BackgroundColor, causticsColor, caustics);

        return finalColor;
    }
    ENDCG
}
    }
        FallBack "Transparent/Diffuse"
}
