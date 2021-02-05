using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Harmony;

namespace SundropNPCTest
{
    public static class SubTypePatcher
    {
        private const string Prefix = "Patch_";
        private static readonly HarmonyInstance Harmony = HarmonyInstance.Create("SubTypePatcher");

        public static void Patch<TType,TSubType>()
        {
            var typeMethods = AccessTools.GetDeclaredMethods(typeof(TType));
            var patchMethods = AccessTools.GetDeclaredMethods(typeof(TSubType)).Where(m => m.Name.StartsWith(Prefix));
            
            patchMethods.ToList().ForEach((patchMethod) =>
            {
                bool hasReturnType = patchMethod.ReturnType != Type.GetType("System.Void");
               
                var methodName = patchMethod.Name.Substring(Prefix.Length);
                var patchParams = patchMethod.GetParameters().Select(p => p.ParameterType).ToArray();
                var method = typeMethods.FirstOrDefault(m => m.Name == methodName && matchTypes(m.GetParameters().Select(p => p.ParameterType).ToArray(), patchParams));
                
                List<Type> patchParameters = new List<Type>();
                List<Type> genericParameters = new List<Type>();

                if (hasReturnType)
                {
                    genericParameters.Add(method.ReturnType);
                    patchParameters.Add(typeof(object).MakeByRefType());
                }

                genericParameters.Add(typeof(TType));

                string patchName = hasReturnType ? nameof(ForwardMethodPatch) : nameof(ForwardMethodPatchVoid);

                patchParameters.Add(typeof(object));
                patchParameters.Add(typeof(MethodInfo));
                patchParameters.AddRange(patchParams);
                genericParameters.AddRange(patchParams);

                if (AccessTools.GetDeclaredMethods(typeof(SubTypePatcher)).FirstOrDefault(m => m.Name == patchName && m.IsGenericMethod && m.GetParameters().Length == patchParameters.Count) is MethodInfo sMethod)
                {
                    Harmony.Patch(
                        original: method,
                        prefix: new HarmonyMethod(sMethod.MakeGenericMethod(genericParameters.ToArray()))
                        );
                }
            });
        }

        internal static bool matchTypes(Type[] a1, Type[] a2)
        {
            if (a1.Length != a2.Length)
                return false;

            if (a1.Length == 0)
                return true;

            for (int i = 0; i < a1.Length; i++)
                if (a1[i] != a2[i])
                    return false;

            return true;
        }


        internal static bool ForwardMethodVoid<TInstance>(TInstance __instance, MethodInfo __originalMethod, params object[] args)
        {
             if (__instance is IPatchedSubType subType
                && subType.ShouldPatch
                && subType.GetType().GetMethod($"{Prefix}{__originalMethod.Name}", __originalMethod.GetParameters().Select(p => p.ParameterType).ToArray()) is MethodInfo m)
            {
                m.Invoke(__instance, args);
                return false;
            }

            return true;
        }

        internal static bool ForwardMethod<TResult, TInstance>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod, params object[] args)
        {
            if (__instance is IPatchedSubType subType
                && subType.ShouldPatch
                && subType.GetType().GetMethod($"{Prefix}{__originalMethod.Name}", __originalMethod.GetParameters().Select(p => p.ParameterType).ToArray()) is MethodInfo m)
            {
                __result = (TResult) m.Invoke(__instance, args);
                return false;
            }

            return true;
        }

        internal static bool ForwardMethodPatch<TResult, TInstance>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod);
        }

        internal static bool ForwardMethodPatch<TResult, TInstance, T0>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0);
        }
        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1);
        }

        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T1 __0,
            T1 __1,
            T2 __2)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2);
        }

        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3);
        }

        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3, T4>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3, __4);
        }

        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3, T4, T5>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3, __4, __5);
        }

        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3, T4, T5, T6>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
           T0 __0,
           T1 __1,
           T2 __2,
           T3 __3,
           T4 __4,
           T5 __5,
           T6 __6)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6);
        }

        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3, T4, T5, T6, T7>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
           T0 __0,
           T1 __1,
           T2 __2,
           T3 __3,
           T4 __4,
           T5 __5,
           T6 __6,
           T7 __7)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7);
        }
        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8);
        }
        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8,
            T9 __9)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8, __9);
        }
        internal static bool ForwardMethodPatch<TResult, TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(ref TResult __result, TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8,
            T9 __9,
            T10 __10)
        {
            return ForwardMethod(ref __result, __instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8, __9, __10);
        }


        internal static bool ForwardMethodPatchVoid<TInstance>(TInstance __instance, MethodInfo __originalMethod)
        {
            return ForwardMethodVoid(__instance, __originalMethod);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0);
        }

        public static bool ForwardMethodPatchVoid<TInstance, T0, T1>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5, T6>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5, T6, T7>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8,
            T9 __9)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8, __9);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8,
            T9 __9,
            T10 __10)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8, __9, __10);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8,
            T9 __9,
            T10 __10,
            T11 __11)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8, __9, __10, __11);
        }

        internal static bool ForwardMethodPatchVoid<TInstance, T0, T1, T2, T3, T4, T5, T6, T7, T8, T9, T10, T11, T12>(TInstance __instance, MethodInfo __originalMethod,
            T0 __0,
            T1 __1,
            T2 __2,
            T3 __3,
            T4 __4,
            T5 __5,
            T6 __6,
            T7 __7,
            T8 __8,
            T9 __9,
            T10 __10,
            T11 __11,
            T12 __12)
        {
            return ForwardMethodVoid(__instance, __originalMethod, __0, __1, __2, __3, __4, __5, __6, __7, __8, __9, __10, __11, __12);
        }

    }
}
