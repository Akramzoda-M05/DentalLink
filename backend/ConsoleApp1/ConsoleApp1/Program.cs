namespace ConsoleApp1
{
    internal class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("main");
            GetData();
            GetDataB();
            Console.WriteLine("Stop");

        }

        static async Task GetData()
        {
            GetDataB();
            Console.WriteLine("A");
        }
        static async Task GetDataB()
        {
            Thread.Sleep(2000);
            Console.WriteLine("B");
        }
    }
}
