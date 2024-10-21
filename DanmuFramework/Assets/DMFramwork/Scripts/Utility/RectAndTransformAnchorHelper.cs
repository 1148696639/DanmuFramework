using UnityEngine;

namespace Utility
{
    public static class RectAndTransformAnchorHelper
    {
        public static void SetPositionX(this Transform transform, float x)
        {
            var position = transform.position;
            position.x = x;
            transform.position = position;
        }

        public static void SetPositionY(this Transform transform, float y)
        {
            var position = transform.position;
            position.y = y;
            transform.position = position;
        }

        public static void SetPositionZ(this Transform transform, float z)
        {
            var position = transform.position;
            position.z = z;
            transform.position = position;
        }

        public static void SetLocalPositionX(this Transform transform, float x)
        {
            var position = transform.localPosition;
            position.x = x;
            transform.localPosition = position;
        }

        public static void SetLocalPositionY(this Transform transform, float y)
        {
            var position = transform.localPosition;
            position.y = y;
            transform.localPosition = position;
        }

        public static void SetLocalPositionZ(this Transform transform, float z)
        {
            var position = transform.localPosition;
            position.z = z;
            transform.localPosition = position;
        }

        public static void SetRectPosX(this RectTransform rectTransform, float x)
        {
            var anchorPos = rectTransform.position;
            anchorPos.x = x;
            rectTransform.anchoredPosition = anchorPos;
        }

        public static void SetRectPosY(this RectTransform rectTransform, float y)
        {
            var anchorPos = rectTransform.position;
            anchorPos.y = y;
            rectTransform.anchoredPosition = anchorPos;
        }

        public static void SetRectPosZ(this RectTransform rectTransform, float z)
        {
            var anchorPos = rectTransform.position;
            anchorPos.z = z;
            rectTransform.anchoredPosition = anchorPos;
        }

        public static void SetRectLocalPosX(this RectTransform rectTransform, float x)
        {
            var anchorPos = rectTransform.localPosition;
            anchorPos.x = x;
            rectTransform.anchoredPosition = anchorPos;
        }

        public static void SetRectLocalPosY(this RectTransform rectTransform, float y)
        {
            var anchorPos = rectTransform.localPosition;
            anchorPos.y = y;
            rectTransform.anchoredPosition = anchorPos;
        }

        public static void SetRectLocalPosZ(this RectTransform rectTransform, float z)
        {
            var anchorPos = rectTransform.localPosition;
            anchorPos.z = z;
            rectTransform.anchoredPosition = anchorPos;
        }

        public static void SetRectSizeX(this RectTransform rectTransform, float x)
        {
            var size = rectTransform.sizeDelta;
            size.x = x;
            rectTransform.sizeDelta = size;
        }

        public static void SetRectSizeY(this RectTransform rectTransform, float y)
        {
            var size = rectTransform.sizeDelta;
            size.y = y;
            rectTransform.sizeDelta = size;
        }



    }
}