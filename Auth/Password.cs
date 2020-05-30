using System;
using System.Security.Cryptography;
using System.Text;

namespace Auth
{
    public class Password
    {
        public Password(string password)
        {
            Salt = Guid.NewGuid().ToString().Substring( 0,  8);
            using (MD5 md5 = MD5.Create()) 
            {
                byte[] inputBytes = Encoding.ASCII.GetBytes(string.Concat(password, Salt)); 
                byte[] hashBytes = md5.ComputeHash(inputBytes); 
                StringBuilder sb = new StringBuilder (); 
                for (int i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append (hashBytes[i].ToString( "X2"));
                }
                PasswordHash = sb.ToString(); 
            }
        }
        public string PasswordHash { get; }
        public string Salt { get; }
        public static bool CheckPassword(string password, string salt, string hash)
        {
            if (string.IsNullOrWhiteSpace(password)) return false;
            
            using (MD5 md5 = MD5.Create()) 
            {
                byte[] inputBytes2 = Encoding.UTF8.GetBytes(string.Concat(password, salt)); 
                byte[] hashBytes2 = md5.ComputeHash(inputBytes2); 
                StringBuilder sb2 = new StringBuilder ();
                for (int i = 0; i < hashBytes2.Length; i++)
                {
                    sb2.Append (hashBytes2[i].ToString( "X2"));
                }
                string passwordHash2 = sb2.ToString();
                if (!String.Equals(hash,passwordHash2)) return false;
            }
            return true;
        }
    }
}