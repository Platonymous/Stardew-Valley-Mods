using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Newtonsoft.Json;
using PyTK;
using PyTK.ContentSync;
using PyTK.CustomElementHandler;
using PyTK.Types;
using StardewValley;
using StardewValley.Objects;
using System.Collections.Generic;

namespace CustomShirts
{
    class CustomHat : Hat, ICustomObject
    {
        public Texture2D _texture = null;
        public string hatId;
        public Texture2D texture
        {
            get
            {
                if (_texture == null && serializedTexture != null && serializedTexture != "na")
                    _texture = JsonConvert.DeserializeObject<SerializationTexture2D>(PyNet.DecompressString(serializedTexture)).getTexture();

                return _texture;
            }
            set
            {
                _texture = value;
                serializedTexture = PyNet.CompressString(JsonConvert.SerializeObject(new SerializationTexture2D(value)));
            }
        }
        public string serializedTexture = "na";
        private HatBlueprint blueprint;

        public CustomHat()
        {
        }

        public CustomHat(int which)
            :base(which)
        {
        }

        public CustomHat(HatBlueprint blueprint)
           : base(blueprint.baseid)
        {
            this.blueprint = blueprint;
            hatId = blueprint.fullid;
            texture = blueprint.texture2d;
            Name = blueprint.name;
            DisplayName = Name;
            description = blueprint.description;
        }

        public override void drawInMenu(SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            if (texture is ScaledTexture2D st)
                st.ForcedSourceRectangle = new Rectangle(0, 0, (int)(20 * st.Scale), (int)(20 * st.Scale));

            spriteBatch.Draw(texture, location + new Vector2(10f, 10f), new Rectangle?(new Rectangle(blueprint.baseid * 20 % FarmerRenderer.hatsTexture.Width, blueprint.baseid * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4, 20, 20)), color * transparency, 0.0f, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
        }

        public static bool Prefix_drawInMenu(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, bool drawStackNumber, Color color, bool drawShadow)
        {
            if (__instance is CustomHat hat)
            {
                hat.drawInMenu(spriteBatch, location, scaleSize, transparency, layerDepth, drawStackNumber, color, drawShadow);
                return false;
            }
            FarmerRenderer.hatsTexture = CustomShirtsMod.vanillaHats;
            return true;
        }

        public Dictionary<string, string> getAdditionalSaveData()
        {
            return new Dictionary<string, string>() { { "blueprint", hatId }, { "which", which.Value.ToString() }, { "name" , Name }, { "description", description } };
        }

        public override Item getOne()
        {
            return new CustomHat(blueprint);
        }

        public static bool Prefix_draw(Hat __instance, SpriteBatch spriteBatch, Vector2 location, float scaleSize, float transparency, float layerDepth, int direction)
        {
            CustomShirtsMod._monitor.Log("draw");
            if (__instance is CustomHat c)
            {
                if (direction == 0)
                    direction = 3;
                else if (direction == 2)
                    direction = 0;
                else if (direction == 3)
                    direction = 2;

                if (c.texture is ScaledTexture2D st)
                    st.ForcedSourceRectangle = new Rectangle(0, (int)(direction * 20 * st.Scale), (int)(20 * st.Scale), (int)(20 * st.Scale));

                spriteBatch.Draw(c.texture, location + new Vector2(10f, 10f), new Rectangle?(new Rectangle(c.blueprint.baseid * 20 % FarmerRenderer.hatsTexture.Width, c.blueprint.baseid * 20 / FarmerRenderer.hatsTexture.Width * 20 * 4 + direction * 20, 20, 20)), Color.White * transparency, 0.0f, new Vector2(3f, 3f), 3f * scaleSize, SpriteEffects.None, layerDepth);
                return false;
            }
            return true;
        }


        public object getReplacement()
        {
            return new Hat(blueprint.baseid);
        }

        public void rebuild(Dictionary<string, string> additionalSaveData, object replacement)
        {
            if (additionalSaveData.ContainsKey("which"))
                which.Value = int.Parse(additionalSaveData["which"]);

            if (additionalSaveData.ContainsKey("name"))
            {
                Name = additionalSaveData["name"];
                DisplayName = Name;
            }

            if (additionalSaveData.ContainsKey("description"))
                description = additionalSaveData["description"];

            hatId = additionalSaveData["blueprint"];
        }

        public ICustomObject recreate(Dictionary<string, string> additionalSaveData, object replacement)
        {
            string id = additionalSaveData["blueprint"];
            int baseid = 0;

            if(additionalSaveData.ContainsKey("which"))
                baseid = int.Parse(additionalSaveData["which"]);

            if (CustomShirtsMod.hats.Find(h => h.fullid == id) is HatBlueprint hb)
                return new CustomHat(hb);
            else
                return new CustomHat(baseid);
        }
    }
}
