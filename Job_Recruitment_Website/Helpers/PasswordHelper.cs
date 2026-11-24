using System;
using System.Security.Cryptography;
using System.Text;

namespace Job_Recruitment_Website.Helpers
{
    public class PasswordHelper
    {
        private const int SaltSize = 16;      // 128-bit Salt ngẫu nhiên
        private const int HashSize = 32;      // 256-bit Key
        private const int Iterations = 10000; // Chuẩn PBKDF2

        // Băm mật khẩu mới kèm Salt ngẫu nhiên riêng biệt cho từng tài khoản
        public static string Hash(string input)
        {
            string password = input ?? "";
            byte[] salt = new byte[SaltSize];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(salt);
            }

            using (var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations))
            {
                byte[] hash = pbkdf2.GetBytes(HashSize);
                return "PBKDF2$" + Iterations + "$" + Convert.ToBase64String(salt) + "$" + Convert.ToBase64String(hash);
            }
        }

        // Xác thực mật khẩu (Hỗ trợ cả chuẩn PBKDF2 có Salt mới & tương thích ngược tài khoản mẫu cũ)
        public static bool Verify(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(inputPassword) || string.IsNullOrEmpty(storedHash))
                return false;

            // 1. Kiểm tra định dạng PBKDF2 có Salt mới
            if (storedHash.StartsWith("PBKDF2$"))
            {
                string[] parts = storedHash.Split('$');
                if (parts.Length == 4)
                {
                    int iterations;
                    if (int.TryParse(parts[1], out iterations))
                    {
                        byte[] salt = Convert.FromBase64String(parts[2]);
                        byte[] expectedHash = Convert.FromBase64String(parts[3]);

                        using (var pbkdf2 = new Rfc2898DeriveBytes(inputPassword, salt, iterations))
                        {
                            byte[] actualHash = pbkdf2.GetBytes(expectedHash.Length);
                            int diff = actualHash.Length ^ expectedHash.Length;
                            for (int i = 0; i < actualHash.Length && i < expectedHash.Length; i++)
                            {
                                diff |= actualHash[i] ^ expectedHash[i];
                            }
                            return diff == 0;
                        }
                    }
                }
                return false;
            }

            // 2. Tương thích ngược với SHA-256 cũ (cho tài khoản đã đăng ký trước đó)
            string legacySha = LegacySha256(inputPassword);
            if (string.Equals(storedHash, legacySha, StringComparison.OrdinalIgnoreCase))
                return true;

            // 3. Tương thích dữ liệu mẫu ban đầu trong SQL (mật khẩu 123456)
            if (storedHash == inputPassword || (inputPassword == "123456" && storedHash.StartsWith("$2b$12$HashPass")))
                return true;

            return false;
        }

        private static string LegacySha256(string input)
        {
            using (var sha = SHA256.Create())
            {
                byte[] bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(input));
                return BitConverter.ToString(bytes).Replace("-", "");
            }
        }
    }
}