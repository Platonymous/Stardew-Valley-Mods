using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Harmony;
using System.Xml;
using System.Xml.Serialization;
using System.Reflection;

namespace CustomElementHandlerHarmony
{
    class SerializerFix
    {

        [HarmonyPatch(typeof(XmlSerializer),"Serialize",new[] { typeof(XmlWriter), typeof(object), typeof(XmlSerializerNamespaces), typeof(string), typeof(string) })]
        internal static class SerializeCEH
        {
            internal static void Prefix(XmlWriter xmlWriter, object o, XmlSerializerNamespaces namespaces, string encodingStyle, string id)
            {
                Log(o.GetType().ToString());   
            }
        }
        /*
        [HarmonyPatch]
        internal static class DeSerializeCEH
        {

            internal static MethodInfo TargetMethod()
            {
                    return AccessTools.Method(Type.GetType("System.Xml.Serialization.TempAssembly, System.Xml.Serialization"), "InvokeReader");
            }

            internal static void Postfix(object __result, XmlMapping mapping, XmlReader xmlReader, XmlDeserializationEvents events, string encodingStyle)
            {
                Log(__result.GetType().ToString());
            }
        }
        */
        internal static void Log(string text)
        {
            CustomElementHandlerHarmonyMod._monitor.Log(text);
        }
    }
}
