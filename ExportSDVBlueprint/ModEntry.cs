using Microsoft.CSharp;
using Microsoft.Xna.Framework.Input;
using StardewModdingAPI;
using StardewModdingAPI.Events;
using StardewValley;
using System;
using System.CodeDom;
using System.CodeDom.Compiler;
using System.Collections.Generic;
using System.Reflection;

namespace ExportSDVBlueprint
{
    public class ModEntry : Mod
    {
        public override void Entry(IModHelper helper)
        {


        }

        static CodeNamespace createNamepace(string name, CodeCompileUnit compileUnit)
        {
            var ns = new CodeNamespace(name);
            compileUnit.Namespaces.Add(ns);

            return ns;
        }

        static void addProperties(Dictionary<string,Type> props, bool get, bool set, CodeTypeDeclaration classType)
        {
            foreach (var prop in props)
            {
                var fieldName = "_" + prop.Key;
                var field = new CodeMemberField(prop.Value, fieldName);
                classType.Members.Add(field);

                var property = new CodeMemberProperty();
                property.Attributes = MemberAttributes.Public | MemberAttributes.Final;
                property.Type = new CodeTypeReference(prop.Value);
                property.Name = prop.Key;
                if(get)
                    property.GetStatements.Add(new CodeMethodReturnStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName)));
                if(set)
                    property.SetStatements.Add(new CodeAssignStatement(new CodeFieldReferenceExpression(new CodeThisReferenceExpression(), fieldName), new CodePropertySetValueReferenceExpression()));
                classType.Members.Add(property);
            }
        }

        static CodeTypeDeclaration createType(string name, MemberAttributes ma, CodeNamespace ns)
        {
            ns.Imports.Add(new CodeNamespaceImport("System"));

            var classType = new CodeTypeDeclaration(name);
            classType.Attributes = ma;
            ns.Types.Add(classType);

            return classType;
        }
    }
}

    }
}
