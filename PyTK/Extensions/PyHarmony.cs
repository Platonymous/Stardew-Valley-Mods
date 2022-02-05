using HarmonyLib;
using StardewModdingAPI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace PyTK.Extensions
{
    public static class PyHarmony
    {
        internal static IModHelper Helper { get; } = PyTKMod._helper;
        internal static IMonitor Monitor { get; } = PyTKMod._monitor;

        private static Dictionary<string, Harmony> harmonyInstances = new Dictionary<string, Harmony>();

        public static void PatchBase(this Type type, IModHelper helper)
        {
            type.PatchType(type.BaseType, helper);
        }

        public static void PatchBase(this Type type, string harmonyId)
        {
            type.PatchType(type.BaseType, harmonyId);
        }

        public static void PatchType(this Type type, Type typeToPatch, IModHelper helper)
        {
            type.PatchType(typeToPatch, "Platonymous.PyTK.PyHarmony." + helper.ModRegistry.ModID);
        }

        public static void PatchType(this Type type, Type typeToPatch, string harmonyId)
        {
            if (!harmonyInstances.ContainsKey(harmonyId))
                harmonyInstances.Add(harmonyId, new Harmony(harmonyId));

            Harmony harmony = harmonyInstances[harmonyId];

            List<MethodInfo> originals = typeToPatch.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static).Where(m => m != m.GetBaseDefinition()).ToList();
            foreach (MethodInfo method in originals)
            {
                MethodInfo[] preMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "Prefix_" + method.Name).ToArray();
                MethodInfo[] postMethods = type.GetMethods(BindingFlags.Public | BindingFlags.Static).Where(m => m.Name == "Postfix_" + method.Name).ToArray();

                MethodInfo preMethod = null;
                MethodInfo postMethod = null;

                if (preMethods.Length == 1)
                    preMethod = preMethods.First();

                if (postMethods.Length == 1)
                    postMethod = postMethods.First();

                if (preMethods.Length > 1)
                {
                    List<String> paramter = method.GetParameters().toList(p => p.Name + ":" + p.ParameterType);
                    preMethod = preMethods.ToList().Find(m =>
                    {
                        List<String> mParamter = m.GetParameters().Where(p => !p.Name.Contains("__")).ToArray().toList(p => p.Name + ":" + p.ParameterType);
                        return mParamter == paramter;
                    });
                }

                if (postMethods.Length > 1)
                {
                    List<String> paramter = method.GetParameters().toList(p => p.Name + ":" + p.ParameterType);
                    postMethod = postMethods.ToList().Find(m =>
                    {
                        List<String> mParamter = m.GetParameters().Where(p => !p.Name.Contains("__")).ToArray().toList(p => p.Name + ":" + p.ParameterType);
                        return mParamter == paramter;
                    });
                }

                if (postMethod != null || preMethod != null)
                    harmony.Patch(method, preMethod == null ? null : new HarmonyMethod(preMethod), postMethod == null ? null : new HarmonyMethod(postMethod));
            }
        }
    }

}
