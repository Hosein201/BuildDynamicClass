using CreateClassWithReflection;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            MyClassBuilder myClassBuilder = new MyClassBuilder("Person");

            var dynamicClass = myClassBuilder.BuildDynamicClass(
                new string[] { "id", "name" },
                new Type[] { typeof(int) },
                new object[] { 1, "Hossein" });

            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}