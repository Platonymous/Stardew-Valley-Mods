
using Netcode;
using PyTK.Extensions;
using StardewValley;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Xml;
using System.Xml.Serialization;

namespace PyTK.CustomElementHandler
{
    public class PySync : INetSerializable
    {
        public PySync(ISyncableElement element)
        {
            Element = element;
        }

        public void init()
        {
            (Element as Item).NetFields.AddField(Element.syncObject);
        }

        ISyncableElement Element { get; set; }

        public virtual XmlSerializer ReplacementSerializer { get; set; }

        public bool Tick()
        {
            return false;
        }

        public virtual void Read(BinaryReader reader, NetVersion version)
        {
            string dataString = PyNet.DecompressString(reader.ReadString());
            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach(string s in dataString.Split(SaveHandler.seperator))
            {
                string[] d = s.Split(SaveHandler.valueSeperator);
                data.Add(d[0], d[1]);
            }

            Element.sync(data);
        }

        public virtual void Write(BinaryWriter writer)
        {
            Dictionary<string, string> data = Element.getSyncData();
            if(data == null)
                data = new Dictionary<string, string>() { { "na", "na" } };

            string dataString = string.Join(SaveHandler.seperator.ToString(), data.Select(x => x.Key + SaveHandler.valueSeperator + x.Value));
            writer.Write(PyNet.CompressString(dataString));
        }

        public virtual void ReadFull(BinaryReader reader, NetVersion version)
        {
            string dataString = PyNet.DecompressString(reader.ReadString());
            reader.ReadBoolean();
            string replacementString = PyNet.DecompressString(reader.ReadString());

            Dictionary<string, string> data = new Dictionary<string, string>();
            foreach (string s in dataString.Split(SaveHandler.seperator))
            {
                string[] d = s.Split(SaveHandler.valueSeperator);
                data.Add(d[0], d[1]);
            }

            object elementReplacement = Element.getReplacement();
            SaveHandler.ReplaceAll(elementReplacement, elementReplacement);

            if (ReplacementSerializer == null)
                ReplacementSerializer = new XmlSerializer(elementReplacement.GetType());

            StringReader replacementStringReader = new StringReader(replacementString);
            object replacement = ReplacementSerializer.Deserialize(replacementStringReader);

            SaveHandler.RebuildAll(replacement, replacement);

            Element.rebuild(data, replacement);
        }

        public virtual void WriteFull(BinaryWriter writer)
        {
            Dictionary<string, string> data = Element.getAdditionalSaveData();
            string dataString = string.Join(SaveHandler.seperator.ToString(), data.Select(x => x.Key + SaveHandler.valueSeperator + x.Value)); ;

            object elementReplacement = Element.getReplacement();
            SaveHandler.ReplaceAll(elementReplacement, elementReplacement);

            if (ReplacementSerializer == null)
                ReplacementSerializer = new XmlSerializer(elementReplacement.GetType());

            StringWriter replacementStringWriter = new StringWriter();
            ReplacementSerializer.Serialize(replacementStringWriter, elementReplacement);
            string replacementString = replacementStringWriter.ToString();

            writer.Write(PyNet.CompressString(dataString));
            writer.Write(true);
            writer.Write(PyNet.CompressString(replacementString));
        }

        private uint dirtyTick = uint.MaxValue;
        protected readonly NetVersion ChangeVersion = new NetVersion();
        private uint minNextDirtyTime;
        public ushort DeltaAggregateTicks;
        private INetSerializable parent;

        public uint DirtyTick
        {
            get
            {
                return this.dirtyTick;
            }
            set
            {
                if (value < this.dirtyTick)
                {
                    this.SetDirtySooner(value);
                }
                else
                {
                    if (value <= this.dirtyTick)
                        return;
                    this.SetDirtyLater(value);
                }
            }
        }

        public virtual bool Dirty
        {
            get
            {
                return (int)this.dirtyTick != -1;
            }
        }

        public bool NeedsTick { get; set; } = false;
        public bool ChildNeedsTick { get; set; } = false;

        public virtual INetRoot Root { get; private set; }

        public INetSerializable Parent
        {
            get
            {
                return this.parent;
            }
            set
            {
                this.SetParent(value);
            }
        }

        protected void SetDirtySooner(uint tick)
        {
            tick = Math.Max(tick, this.minNextDirtyTime);
            if (this.dirtyTick <= tick)
                return;
            this.dirtyTick = tick;
            if (this.Parent != null)
                this.Parent.DirtyTick = Math.Min(this.Parent.DirtyTick, tick);
            if (this.Root != null)
            {
                this.minNextDirtyTime = this.Root.Clock.GetLocalTick() + (uint)this.DeltaAggregateTicks;
                this.ChangeVersion.Set(this.Root.Clock.netVersion);
            }
            else
            {
                this.minNextDirtyTime = 0U;
                this.ChangeVersion.Clear();
            }
        }

        protected void SetDirtyLater(uint tick)
        {
            if (this.dirtyTick >= tick)
                return;
            this.dirtyTick = tick;
            this.ForEachChild((Action<INetSerializable>)(child => child.DirtyTick = Math.Max(child.DirtyTick, tick)));
            if ((int)tick != -1)
                return;
            this.CleanImpl();
        }

        protected virtual void CleanImpl()
        {
            if (this.Root == null)
                this.minNextDirtyTime = 0U;
            else
                this.minNextDirtyTime = this.Root.Clock.GetLocalTick() + (uint)this.DeltaAggregateTicks;
        }

        public void MarkDirty()
        {
            if (this.Root == null)
                this.SetDirtySooner(0U);
            else
                this.SetDirtySooner(this.Root.Clock.GetLocalTick());
        }

        public void MarkClean()
        {
            this.SetDirtyLater(uint.MaxValue);
        }

        protected uint GetLocalTick()
        {
            if (this.Root != null)
                return this.Root.Clock.GetLocalTick();
            return 0;
        }

        protected NetTimestamp GetLocalTimestamp()
        {
            if (this.Root != null)
                return this.Root.Clock.GetLocalTimestamp();
            return new NetTimestamp();
        }

        protected NetVersion GetLocalVersion()
        {
            if (this.Root != null)
                return new NetVersion(this.Root.Clock.netVersion);
            return new NetVersion();
        }

        protected virtual void SetParent(INetSerializable parent)
        {
            if (this.parent != parent)
                this.minNextDirtyTime = 0U;
            this.parent = parent;
            if (parent != null)
            {
                this.Root = parent.Root;
                this.SetChildParents();
            }
            else
                this.ClearChildParents();
            this.MarkClean();
        }

        protected virtual void SetChildParents()
        {
            this.ForEachChild((Action<INetSerializable>)(child => child.Parent = (INetSerializable)this));
        }

        protected virtual void ClearChildParents()
        {
            this.ForEachChild((Action<INetSerializable>)(child =>
            {
                if (child.Parent != this)
                    return;
                child.Parent = (INetSerializable)null;
            }));
        }

        protected virtual void ValidateChild(INetSerializable child)
        {
            if ((this.Parent != null || this.Root == this) && child.Parent != this)
                throw new InvalidOperationException();
        }

        protected virtual void ValidateChildren()
        {
            if (this.Parent == null && this.Root != this)
                return;
            this.ForEachChild(new Action<INetSerializable>(this.ValidateChild));
        }

        protected virtual void ForEachChild(Action<INetSerializable> childAction)
        {
        }
    }
}
