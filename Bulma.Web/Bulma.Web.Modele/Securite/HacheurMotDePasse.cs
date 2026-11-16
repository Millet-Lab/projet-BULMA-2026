using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace Bulma.Web.Modele.Securite
{
    public static class HacheurMotDePasse
    {
        private const int TailleSel = 16;
        private const int TailleHash = 32;
        private const int Iterations = 100000;

        // Résultat : "iterations.sel.hash" (base64), environ 76 caractères
        public static string Hacher(string motDePasse)
        {
            if (string.IsNullOrEmpty(motDePasse))
            {
                throw new ArgumentException("Le mot de passe est obligatoire.", nameof(motDePasse));
            }

            byte[] sel = new byte[TailleSel];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(sel);
            }

            byte[] hash = Deriver(motDePasse, sel, Iterations);
            return Iterations + "." + Convert.ToBase64String(sel) + "." + Convert.ToBase64String(hash);
        }

        public static bool Verifier(string motDePasse, string hashStocke)
        {
            if (string.IsNullOrEmpty(motDePasse) || string.IsNullOrEmpty(hashStocke))
            {
                return false;
            }

            string[] parties = hashStocke.Split('.');

            if (parties.Length != 3 || !int.TryParse(parties[0], out int iterations)
                || iterations < 1 || iterations > 1000000)
            {
                return false;
            }

            byte[] sel;
            byte[] attendu;

            try
            {
                sel = Convert.FromBase64String(parties[1]);
                attendu = Convert.FromBase64String(parties[2]);
            }
            catch (FormatException)
            {
                return false;
            }

            return ComparerTempsConstant(Deriver(motDePasse, sel, iterations), attendu);
        }

        private static byte[] Deriver(string motDePasse, byte[] sel, int iterations)
        {
            using (var pbkdf2 = new Rfc2898DeriveBytes(motDePasse, sel, iterations, HashAlgorithmName.SHA256))
            {
                return pbkdf2.GetBytes(TailleHash);
            }
        }

        // comparaison sans sortie anticipée, pour ne pas révéler où les valeurs diffèrent
        private static bool ComparerTempsConstant(byte[] a, byte[] b)
        {
            if (a.Length != b.Length)
            {
                return false;
            }

            int difference = 0;

            for (int i = 0; i < a.Length; i++)
            {
                difference |= a[i] ^ b[i];
            }

            return difference == 0;
        }
    }
}