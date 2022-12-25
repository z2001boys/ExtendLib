using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using System.Dynamic;

namespace ExtendLib
{

    public static class TypeBuilderHelp
    {
        
        public static TypeBuilder GetTypeBuilder(string inputAsmName, string typeName)
        {
            var asm = new AssemblyName(inputAsmName);
            AssemblyBuilder assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(asm, AssemblyBuilderAccess.Run);
            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("TemplateModule");
            TypeBuilder tb = moduleBuilder.DefineType(typeName,
                    TypeAttributes.Public |
                    TypeAttributes.Class |
                    TypeAttributes.AutoClass |
                    TypeAttributes.AnsiClass |
                    TypeAttributes.BeforeFieldInit |
                    TypeAttributes.AutoLayout,
                    null);

            return tb;
        }

        public static void CreateProperty(TypeBuilder inputType, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = inputType.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            //the decaired attr
            var attr = MethodAttributes.Public |
                    MethodAttributes.HideBySig |
                    MethodAttributes.SpecialName |
                    MethodAttributes.Virtual;               //for property:get,set

            //--------------------------------------------
            //  getter
            //--------------------------------------------
            PropertyBuilder propertyBuilder = inputType.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = inputType.DefineMethod("get_" + propertyName, attr, propertyType, Type.EmptyTypes);
            ILGenerator getIl = getPropMthdBldr.GetILGenerator();

            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            //--------------------------------------------
            //  setter
            //--------------------------------------------
            MethodBuilder setPropMthdBldr =
                inputType.DefineMethod("set_" + propertyName,
                  attr,
                  null, new[] { propertyType });

            ILGenerator setIl = setPropMthdBldr.GetILGenerator();
            Label modifyProperty = setIl.DefineLabel();
            Label exitSet = setIl.DefineLabel();

            setIl.MarkLabel(modifyProperty);
            setIl.Emit(OpCodes.Ldarg_0);
            setIl.Emit(OpCodes.Ldarg_1);
            setIl.Emit(OpCodes.Stfld, fieldBuilder);

            setIl.Emit(OpCodes.Nop);
            setIl.MarkLabel(exitSet);
            setIl.Emit(OpCodes.Ret);
            //--------------------------------------------
            //  set property
            //--------------------------------------------
            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }

    }


    public class DynaTyper
    {
        public TypeBuilder Typer;

        Dictionary<string, object> _dataToBeInsert = new Dictionary<string, object>();

        public DynaTyper(string moduleName,string typeName)
        {
            Typer = TypeBuilderHelp.GetTypeBuilder(moduleName,typeName);
            ConstructorBuilder constructor = Typer.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        }

        public void InsertField(string insertName, Type insertType, object value)
        {
            TypeBuilderHelp.CreateProperty(Typer, insertName, insertType);
            if (value != null)
            {
                _dataToBeInsert.Add(insertName, value);
            }
        }

        public void InsertInterface(Type targetInterface)
        {
            Typer.AddInterfaceImplementation(targetInterface);
        }

        public object CreateObject()
        {
            var objectType = Typer.CreateType();

            var result = Activator.CreateInstance(objectType);

            foreach (var pair in _dataToBeInsert)
            {
                var insertMethod = objectType.GetProperty(pair.Key);
                insertMethod.SetValue(result, pair.Value);
            }

            return result;
        }

    }



}
