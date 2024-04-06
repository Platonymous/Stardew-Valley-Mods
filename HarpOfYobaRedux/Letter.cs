using StardewModdingAPI.Events;
using StardewModdingAPI;
using StardewValley;
using System;
using System.Collections.Generic;
using System.Linq;

namespace HarpOfYobaRedux
{
    public class Mail
    {
        public string text;
        public string id;
        public string attachments;
        public AttachmentType attachmentType;

        public enum AttachmentType
        {
            OBJECT,
            QUEST,
            QUEST_COMPLETION,
            TOOLS
        }

        public Mail()
        {

        }

        public Mail(string id, string text, string attachments = "", AttachmentType attachmentType = AttachmentType.OBJECT)
        {
            this.id = id;
            this.text = text;
            this.attachments = attachments;
            this.attachmentType = attachmentType;
        }

        public Mail(string id, string text, List<Item> attachments, AttachmentType attachmentType = AttachmentType.OBJECT)
        {
            this.id = id;
            this.text = text;
            this.attachmentType = attachmentType;
            this.attachments = String.Join(" ", attachments.Select(a => a.ParentSheetIndex + " " + a.Stack));
        }

        public Mail(string id, string text, int attachment, AttachmentType attachmentType = AttachmentType.OBJECT)
        {
            this.id = id;
            this.text = text;
            this.attachmentType = attachmentType;
            attachments = attachmentType == AttachmentType.OBJECT ? attachment + " 1" : attachment.ToString();
        }


        public void injectIntoMail(IModHelper helper)
        {
            Func<IDictionary<string, string>, IDictionary<string, string>> merger = new Func<IDictionary<string, string>, IDictionary<string, string>>(delegate (IDictionary<string, string> asset)
            {
                string t = text;
                string type = (attachmentType == AttachmentType.QUEST || attachmentType == AttachmentType.QUEST_COMPLETION) ? "quest" : (attachmentType == AttachmentType.TOOLS) ? "tools" : "object";
                if (attachments != null && attachments != "")
                    t += $" %item {type} {String.Join(" ", (attachmentType == AttachmentType.QUEST_COMPLETION) ? attachments + " true" : attachments)} %%";

                if (asset.ContainsKey(id))
                    asset.Remove(id);
                asset.Add(id, t);
                return asset;
            });

            new AssetEditInjector<IDictionary<string, string>, IDictionary<string, string>>($"Data/mail", merger).injectEdit(helper);
        }

    }

    public class AssetEditInjector<TAsset, TSource>
    {
        private readonly Func<TSource, TAsset> asset;
        private readonly Func<IAssetName, bool> predicate;

        public void Invalidate(IModHelper helper)
        {
            helper.GameContent.InvalidateCache(a => predicate(a.Name));
        }

        public void injectEdit(IModHelper Helper)
        {
            Helper.Events.Content.AssetRequested += this.OnAssetRequested;
        }

        public AssetEditInjector(string assetName, TAsset asset)
        {
            this.asset = _ => asset;
            predicate = a => a.IsEquivalentTo(assetName, useBaseName: true);
        }

        public AssetEditInjector(string assetName, Func<TSource, TAsset> asset)
        {
            this.asset = asset;
            predicate = a => a.IsEquivalentTo(assetName, useBaseName: true);
        }

        internal void OnAssetRequested(object sender, AssetRequestedEventArgs e)
        {
            if (predicate(e.NameWithoutLocale))
            {
                if (typeof(TSource) == typeof(IAssetDataForImage))
                    e.Edit(asset => this.asset((TSource)asset.AsImage()));
                else
                    e.Edit(asset => asset.ReplaceWith(this.asset.Invoke(asset.GetData<TSource>())));
            }
        }
    }

    public class Letter : Mail
    {
        public Item item;

        public Letter()
        {

        }

        public Letter(IModHelper helper, string id, string text, Item item = null)
            :base()
        {
            this.text = text + " %item object 388 1 %%";
            this.attachments = null;
            if (item == null)
                item = new SheetMusic(id);

            this.id = "hoy_" + id;
            this.item = item;
            injectIntoMail(helper);
        }

    }
}
