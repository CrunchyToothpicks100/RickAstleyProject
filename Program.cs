namespace RickAstleyProject
{
    class Program
    {
        static void Main(String[] args)
        {
            var bot = new Bot();
            bot.RunAsync().GetAwaiter().GetResult();
        }
    }
}