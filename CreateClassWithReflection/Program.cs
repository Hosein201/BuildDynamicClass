using BenchmarkDotNet.Running;
using CreateClassWithReflection;

public class Program
{
    public static void Main(string[] args)
    {
        try
        {
            var summary = BenchmarkRunner.Run<MyClassBuilder>();
            Console.WriteLine(summary);

            Console.ReadKey();
        }
        catch (Exception ex)
        {
            Console.WriteLine(ex.Message);
        }
    }
}