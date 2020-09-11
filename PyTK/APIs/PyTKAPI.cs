using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using PyTK.Types;
using System;
using System.IO;
using PyTK.Extensions;
using StardewModdingAPI;
using System.Collections.Generic;
using xTile;
using xTile.Display;
using Microsoft.Xna.Framework.Content;

namespace PyTK.APIs
{
    public class PyTKAPI : IScalerAPI, ISerializerAPI, IMapAPI
    {
        public Texture2D CreateScaledTexture2D(Texture2D orgTexture, Texture2D scaledTexture, float scale = -1, Rectangle? forcedSourceRectangle = null)
        {
            if(scale == -1)
                scale = (float)(Convert.ToDouble(scaledTexture.Width) / Convert.ToDouble(orgTexture.Width));

            return ScaledTexture2D.FromTexture(orgTexture, scaledTexture, scale, forcedSourceRectangle); ;
        }

        public Texture2D CreateScaledTexture2D(Rectangle orgSize, Texture2D scaledTexture, float scale = -1, Rectangle? forcedSourceRectangle = null)
        {
            if (scale == -1)
                scale = (float)(Convert.ToDouble(scaledTexture.Width) / Convert.ToDouble(orgSize.Width));

            return ScaledTexture2D.FromTexture(PyUtils.getRectangle(orgSize.Width,orgSize.Height,Color.White), scaledTexture, scale, forcedSourceRectangle); ;
        }

        public Texture2D CreateScaledTexture2D(int orgWidth, Texture2D scaledTexture, Rectangle? forcedSourceRectangle = null)
        {
            float scale = (float)(Convert.ToDouble(scaledTexture.Width) / Convert.ToDouble(orgWidth));
            return CreateScaledTexture2D(new Rectangle(0, 0, orgWidth, (int)(scaledTexture.Height / scale)), scaledTexture, scale, forcedSourceRectangle);
        }

        public Texture2D CreateScaledTexture2D(Texture2D scaledTexture, float scale, Rectangle? forcedSourceRectangle = null)
        {
            return ScaledTexture2D.FromTexture(PyUtils.getRectangle((int)((float)scaledTexture.Width / scale), (int)((float)scaledTexture.Height/scale), Color.White), scaledTexture, scale, forcedSourceRectangle); ;
        }
        
        public bool IsScaledTexture2D(Texture2D texture)
        {
            return (texture is ScaledTexture2D);
        }

        public Texture2D SetForcedSourceRectangle(Texture2D texture, Rectangle? forcedSourceRectangle = null)
        {
            if (texture is ScaledTexture2D s)
            {
                s.ForcedSourceRectangle = forcedSourceRectangle;
                return s;
            }

            return texture;
        }

        public Texture2D SetScale(Texture2D scaledTexture, float scale)
        {
            if (scaledTexture is ScaledTexture2D s)
            {
                s.Scale = scale;
                return s;
            }

            return CreateScaledTexture2D(scaledTexture, scale);
        }

        public float GetScale(Texture2D scaledTexture)
        {
            if (scaledTexture is ScaledTexture2D s)
                return s.Scale;

            return 1;
        }

        public Texture2D SetScaledTexture(Texture2D scaledTexture, Texture2D newTexture)
        {
            if (scaledTexture is ScaledTexture2D s)
            {
                s.STexture = newTexture;
                return s;
            }

            return newTexture;
        }

        public Texture2D GetScaledTexture(Texture2D scaledTexture)
        {
            if (scaledTexture is ScaledTexture2D s)
                return s.STexture;

            return scaledTexture;
        }

        public Texture2D ScaleUpTexture(Texture2D texture, float scale, bool asScaledTexture2D = true, Rectangle? forcedSourceRectangle = null)
        {
            Texture2D sTexture = texture.ScaleUpTexture(scale, asScaledTexture2D, forcedSourceRectangle);

            if (!asScaledTexture2D)
                return sTexture;

            return CreateScaledTexture2D(texture, sTexture, scale, forcedSourceRectangle);
        }

        public void AddPreSerialization(IManifest manifest, Func<object, object> preserializer)
        {
            PyTKMod.PostSerializer.AddOrReplace(manifest, preserializer);
        }

        public void AddPostDeserialization(IManifest manifest, Func<object, object> postserializer)
        {
            PyTKMod.PostSerializer.AddOrReplace(manifest, postserializer);
        }

        public void ReplaceAssetAt(string assetName, Rectangle sourceRectangle, Texture2D texture)
        {
            if (Overrides.OvSpritebatchNew.repTextures.ContainsKey(assetName))
                Overrides.OvSpritebatchNew.repTextures[assetName].AddOrReplace(sourceRectangle, texture);
            else
                Overrides.OvSpritebatchNew.repTextures.Add(assetName, new Dictionary<Rectangle?, Texture2D>() { { sourceRectangle, texture } });
        }

        public void EnableMoreMapLayers(Map map)
        {
            map?.enableMoreMapLayers();
        }

        public IDisplayDevice GetPyDisplayDevice(ContentManager contentManager, GraphicsDevice graphicsDevice)
        {
            return PyDisplayDevice.Instance ?? new PyDisplayDevice(contentManager, graphicsDevice);
        }

        public IDisplayDevice GetPyDisplayDevice(ContentManager contentManager, GraphicsDevice graphicsDevice, bool compatability)
        {
            return PyDisplayDevice.Instance ?? new PyDisplayDevice(contentManager, graphicsDevice,compatability);
        }

        public Dictionary<string, string> ParseDataString(object o)
        {
            return CustomElementHandler.SaveHandler.parseDataString(o);
        }
    }
}
