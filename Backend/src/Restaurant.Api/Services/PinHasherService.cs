using Restaurant.Api.Interfaces;
using Restaurant.Api.Models;
using System.Runtime.Intrinsics.X86;
using System.Security.Cryptography;
using System.Text;

namespace Restaurant.Api.Services
{
    public class PinHasherService : IPinHasher
    {
        private readonly byte[] _key;  
        public PinHasherService(IConfiguration configuration)
        {
            _key = Convert.FromBase64String(configuration["Pin:PinHashingKey"]!);

        }

        public string HashPin(string pin)
        {
            var hash = HMACSHA256.HashData(_key, Encoding.UTF8.GetBytes(pin));
            return Convert.ToHexString(hash);

        }
    }
}
