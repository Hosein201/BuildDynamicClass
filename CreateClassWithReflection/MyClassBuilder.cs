using BenchmarkDotNet.Attributes;
using CreateClassWithReflection.Model;
using System.Reflection;
using System.Reflection.Emit;

namespace CreateClassWithReflection
{
    [MemoryDiagnoser]
    public class MyClassBuilder
    {
        AssemblyName asemblyName;
        public MyClassBuilder()
        {
            this.asemblyName = new AssemblyName("DynamicPerson");
        }

        [Benchmark]
        public object StaticClass()
        {
            return new StaticPerson { Id = 1, Name = "Hossein" };
        }

        [Benchmark]
        public object DynamicClass()
        {

            return CreateDynamicClass(
                  new string[] { "Id", "Name" },
                  new Type[] { typeof(int), typeof(string) },
                  new object[] { 1, "Hossein" });
        }
        /// <summary>
        /// build dynamic class in run time
        /// </summary>
        /// <param name="propertyNames"> names of props </param>
        /// <param name="types"> types of props </param>
        /// <param name="values">values of props</param>
        /// <returns></returns>

        private object CreateDynamicClass(string[] propertyNames, Type[] types, object[] values)
        {
            if (propertyNames.Length != types.Length || propertyNames.Length != values.Length || types.Length != values.Length)
                throw new ArgumentOutOfRangeException("The number of property names should match their corresopnding types number and values number ");

            var dynamicClass = CreateObject(propertyNames, types);

            for (int i = 0; i < propertyNames.Length; i++)
            {
                PropertyInfo propertyInfo = dynamicClass.GetType().GetProperty(propertyNames[i]);
                propertyInfo.SetValue(dynamicClass, values[i], null);
            }

            return dynamicClass;
        }

        private object CreateObject(string[] propertyNames, Type[] types)
        {
            TypeBuilder DynamicClass = CreateClass();
            CreateConstructor(DynamicClass);

            for (int ind = 0; ind < propertyNames.Count(); ind++)
                CreateProperty(DynamicClass, propertyNames[ind], types[ind]);

            Type type = DynamicClass.CreateType();

            return Activator.CreateInstance(type);
        }

        private TypeBuilder CreateClass()
        {
            AssemblyBuilder assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(this.asemblyName, AssemblyBuilderAccess.RunAndCollect);

            ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("DynamicClass");

            TypeBuilder typeBuilder = moduleBuilder.DefineType(this.asemblyName.FullName
                                , TypeAttributes.Public |
                                TypeAttributes.Class |
                                TypeAttributes.AutoClass |
                                TypeAttributes.AnsiClass |
                                TypeAttributes.BeforeFieldInit |
                                TypeAttributes.AutoLayout
                                , null);
            return typeBuilder;
        }

        private void CreateConstructor(TypeBuilder typeBuilder)
        {
            typeBuilder.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);
        }

        private void CreateProperty(TypeBuilder typeBuilder, string propertyName, Type propertyType)
        {
            FieldBuilder fieldBuilder = typeBuilder.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

            PropertyBuilder propertyBuilder = typeBuilder.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
            MethodBuilder getPropMthdBldr = typeBuilder.DefineMethod("get_" + propertyName, MethodAttributes.Public, propertyType, Type.EmptyTypes);

            ILGenerator getIl = getPropMthdBldr.GetILGenerator();
            getIl.Emit(OpCodes.Ldarg_0);
            getIl.Emit(OpCodes.Ldfld, fieldBuilder);
            getIl.Emit(OpCodes.Ret);

            MethodBuilder setPropMthdBldr = typeBuilder.DefineMethod("set_" + propertyName,
                  MethodAttributes.Public |
                  MethodAttributes.SpecialName |
                  MethodAttributes.HideBySig,
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

            propertyBuilder.SetGetMethod(getPropMthdBldr);
            propertyBuilder.SetSetMethod(setPropMthdBldr);
        }
    }
}
