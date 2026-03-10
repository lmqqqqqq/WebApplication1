namespace WebApplication1.Services
{
    public static class ShortUrlGenerator
    {
        private const string chars =
            "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

        public static string Generate(int length = 6)
        {
            var random = new Random();
           
             return new string(Enumerable.Repeat(chars, length)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
    }
}