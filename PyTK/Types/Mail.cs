using PyTK.Extensions;
using StardewValley;
using System;
using System.Collections.Generic;

namespace PyTK.Types
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
            this.attachments = String.Join(" ", attachments.toList(a => a.ParentSheetIndex + " " + a.Stack));
        }

        public Mail(string id, string text, int attachment, AttachmentType attachmentType = AttachmentType.OBJECT)
        {
            this.id = id;
            this.text = text;
            this.attachmentType = attachmentType;
            attachments = attachmentType == AttachmentType.OBJECT ? attachment + " 1" : attachment.ToString();
        }


        public AssetEditInjector<IDictionary<string, string>, IDictionary<string, string>> injectIntoMail()
        {
            Func<IDictionary<string, string>, IDictionary<string, string>> merger = new Func<IDictionary<string, string>, IDictionary<string, string>>(delegate (IDictionary<string, string> asset)
            {
                string t = text;
                string type = (attachmentType == AttachmentType.QUEST || attachmentType == AttachmentType.QUEST_COMPLETION) ? "quest" : (attachmentType == AttachmentType.TOOLS) ? "tools" : "object";
                if (attachments != null && attachments != "")
                    t += $" %item { type } { String.Join(" ", (attachmentType == AttachmentType.QUEST_COMPLETION) ? attachments + " true" : attachments)} %%";

                return asset.AddOrReplace(id, t);
            });

            return new AssetEditInjector<IDictionary<string, string>, IDictionary<string, string>>($"Data/mail", merger).injectEdit();
        }

    }
}
