using UnityEngine;
using UnityEngine.Experimental.Rendering;
using UnityEngine.UIElements;


namespace BallMaze.UI
{
    public class NonRectangularButton : Button
    {
        public new class UxmlFactory : UxmlFactory<NonRectangularButton> { }

        private float _alphaHitTestMinThreshold = 0.5f;

        bool DefaultContains(Vector2 local)
        {
            return local.x >= 0 && local.y >= 0 &&
                   local.x <= resolvedStyle.width &&
                   local.y <= resolvedStyle.height;
        }

        bool SpriteSupportsAlphaHitTest(Texture2D texture)
        {
            return texture != null &&
                   !GraphicsFormatUtility.IsCrunchFormat(texture.format) &&
                   texture.isReadable;
        }

        public override bool ContainsPoint(Vector2 local)
        {
            if (_alphaHitTestMinThreshold <= 0)
            {
                return DefaultContains(local);
            }

            Background b = resolvedStyle.backgroundImage;

            Texture2D tex = b.texture;
            if (tex == null && b.sprite != null)
            {
                tex = b.sprite.texture;
            }

            if (!SpriteSupportsAlphaHitTest(tex))
            {
                Debug.LogWarning("Sprite Doesn't support pixel read");
                _alphaHitTestMinThreshold = 0;
                return DefaultContains(local);
            }

            float ratio = Mathf.Min(resolvedStyle.width / tex.width, resolvedStyle.height / tex.height);
            Vector2 resolvedImageSize = new Vector2(tex.width * ratio, tex.height * ratio);
            Vector2 topLeft = new Vector2((resolvedStyle.width - resolvedImageSize.x) / 2, (resolvedStyle.height - resolvedImageSize.y) / 2);

            // if (local.x < topLeft.x || local.x > topLeft.x + resolvedImageSize.x ||
            //     local.y < topLeft.y || local.y > topLeft.y + resolvedImageSize.y)
            // {
            //     return false;
            // }

            // Convert local coordinates to texture space.
            float x = (local.x - topLeft.x) / resolvedImageSize.x;
            float y = 1 - (local.y - topLeft.y) / resolvedImageSize.y; // Texture UV (0,0) is bottom-left

            try
            {
                var a = tex.GetPixelBilinear(x, y).a;
                return a >= _alphaHitTestMinThreshold;
            }
            catch (UnityException e)
            {
                Debug.LogError(
                    "Using alphaHitTestThreshold greater than 0 on Image whose sprite texture cannot be read. " +
                    e.Message + " Also make sure to disable sprite packing for this sprite.");
                _alphaHitTestMinThreshold = 0;
                return DefaultContains(local);
            }
        }
    }
}