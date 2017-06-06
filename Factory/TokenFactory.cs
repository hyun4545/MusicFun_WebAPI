using Jose;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Script.Serialization;
using MusicFun_WebAPI.Models;

namespace MusicFun_WebAPI.Factory
{
    public class TokenFactory
    {
        private static readonly byte[] cypherKey;
        private static readonly byte[] cypherIV;
        private static readonly byte[] JWT_key;
        private const string CYPHER_KEY_STRING = "RFGWEGFW4G4WGFT4RG4W3GFT43GT34wfeewfewfqewfeqw";
        private const string CYPHER_IV_STRING = "DDWFQWFQFWEFDEQWFRQ3Rwfcweqfwadf";
        private const string JWT_KEY_STRING = "SDFAWFWAEFWAEFWDDwadfergfergferwgfwergfwergfwer";
        static TokenFactory()
        {
            JWT_key = Encoding.UTF8.GetBytes(JWT_KEY_STRING);
            cypherKey = new byte[32];
            Array.Copy(Encoding.UTF8.GetBytes(CYPHER_KEY_STRING), cypherKey, 32);
            cypherIV = new byte[16];
            Array.Copy(Encoding.UTF8.GetBytes(CYPHER_IV_STRING), cypherIV, 16);

        }
        private static string TokenEncryptor(string token)
        {
            return TokenEncryptor(token, cypherKey, cypherIV);
        }
        private static string TokenDecryptor(string ciphertxt)
        {
            return TokenDecryptor(ciphertxt, cypherKey, cypherIV);
        }
        private static string TokenEncryptor(string token, byte[] key, byte[] iv)
        {
            byte[] data = ASCIIEncoding.ASCII.GetBytes(token);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            string ciphertxt = Convert.ToBase64String(aes.CreateEncryptor(key, iv).TransformFinalBlock(data, 0, data.Length));
            ciphertxt = HttpUtility.UrlEncode(ciphertxt);
            return ciphertxt;

        }

        private static string TokenDecryptor(string ciphertxt, byte[] key, byte[] iv)
        {
            ciphertxt = HttpUtility.UrlDecode(ciphertxt);
            byte[] data = Convert.FromBase64String(ciphertxt);
            AesCryptoServiceProvider aes = new AesCryptoServiceProvider();
            string token = ASCIIEncoding.ASCII.GetString(aes.CreateDecryptor(key, iv).TransformFinalBlock(data, 0, data.Length));
            return token;
        }



        public static string TokenEncoder(UserInfo userInfo)
        {
            string token = ObjToToken(userInfo);
            string ciphertxt = TokenEncryptor(token);
            return ciphertxt;
        }

        public static UserInfo TokenDecoder(string cipher_token)
        {
            string token = TokenDecryptor(cipher_token);
            UserInfo userInfo = TokenToObj(token);
            return userInfo;
        }

        private static UserInfo TokenToObj(string token)
        {
            string user_json = JWT.Decode(token, JWT_key, JwsAlgorithm.HS256);
            JavaScriptSerializer jss = new JavaScriptSerializer();
            UserInfo user_info = (UserInfo)jss.Deserialize(user_json, typeof(UserInfo));
            return user_info;
        }

        private static string ObjToToken(UserInfo userInfo)
        {
            string token = JWT.Encode(userInfo, JWT_key, JwsAlgorithm.HS256);
            return token;
        }
    }
}