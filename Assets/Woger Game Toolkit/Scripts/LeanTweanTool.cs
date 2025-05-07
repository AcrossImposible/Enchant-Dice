using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public static class LeanTweanTool
{
    public static LTDescr ShowImage(Image image, float time = 0.18f, LeanTweenType easeType = LeanTweenType.easeOutQuad)
    {
        return LeanTween.value(image.gameObject, a =>
        {
            if (image.gameObject)
            {
                var c = image.color;
                c.a = a;
                image.color = c;
                //Debug.Log($"{image.color.a} === {image} === {image.transform.parent}");
            }
        }, 0, 1, time).setEase(easeType);
    }

    public static LTDescr HideImage(Image image, float time = 0.18f, LeanTweenType easeType = LeanTweenType.easeOutQuad)
    {
        //Debug.Log(image);
        return LeanTween.value(image.gameObject.transform.root.gameObject, a =>
        {
            if (image)
            {
                var c = image.color;
                c.a = a;
                image.color = c;
            }
            //Debug.Log($"{image.color.a} === {image} === {image.transform.parent}");

        }, 1, 0, time).setEase(easeType);
    }

    public static LTDescr SetTransparencyImage(Image image, float target, float time = 0.18f, LeanTweenType easeType = LeanTweenType.easeOutQuad)
    {
        var start = image.color.a;
        return LeanTween.value(image.gameObject, a =>
        {
            if (image)
            {
                var c = image.color;
                c.a = a;
                image.color = c;
            }
            //Debug.Log($"{image.color.a} === {image} === {image.transform.parent}");

        }, start, target, time).setEase(easeType);
    }

    public static LTDescr SetTransparencyImage(Graphic graphic, float target, float time = 0.18f, LeanTweenType easeType = LeanTweenType.easeOutQuad)
    {
        var start = graphic.color.a;
        return LeanTween.value(graphic.gameObject, a =>
        {
            if (graphic)
            {
                var c = graphic.color;
                c.a = a;
                graphic.color = c;
            }
            //Debug.Log($"{image.color.a} === {image} === {image.transform.parent}");

        }, start, target, time).setEase(easeType);
    }

    public static LTDescr SetTransparencyImage(CanvasGroup group, float target, float time = 0.18f, LeanTweenType easeType = LeanTweenType.easeOutQuad)
    {
        var start = group.alpha;
        return LeanTween.value(group.transform.root.gameObject, a =>
        {
            if (group)
            {
                group.alpha = a;
            }
            //Debug.Log($"{image.color.a} === {image} === {image.transform.parent}");

        }, start, target, time).setEase(easeType);
    }
}
