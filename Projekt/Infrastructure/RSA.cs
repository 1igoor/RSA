using Projekt.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Projekt.Infrastructure
{
    class RSA
    {

        // random.Next(0,2) wylosuje 0 lub 1
        const int primeNumbersRangeFrom = 13; //od jakiej liczby (domknięte)
        const int primeNumbersRangeTo = 30; // do jakiej liczby (otwarte)

        public PrivateKey myPrivateKey;
        public PublicKey myPublicKey;
        public PublicKey targetPublicKey;


        public RSA()
        {
            GenerateKeys();
        }


        private void GenerateKeys()
        {
            Random random = new Random();

            // 1. Wybieramy 2 (duze) liczby pierwsze
            int p = GetRandomPrimeNumber(random);
            int q = GetRandomPrimeNumber(random);

            // 2. Obliczamy ich iloczyn
            int n = p * q;

            // 3. Obliczamy funkcje Eulera (fi)
            // Funkcje Eulera obliczamy dla n, fi(n) = (p-1)(q-1)
            // Funkcja Eulera przypisze dla n liczbę liczb wzglednie z nią pierwszych
            // Dwie liczby są względnie pierwsze, gdy ich największy wspólny dzielnik jest równy 1.
            int fi = (p - 1) * (q - 1);

            // 4. Wybieramy losowo liczbę e < n względnie pierwszą z fi
            // Liczba e będzie kluczem szyfrującym
            int e = ReturnE(random, n, fi);

            // 5. Znajdujemy (korzystając z rozszerzonego alg. Euklidesa) liczbę d taką, że d = e^-1(mod y) lub de = 1(mod y), d < y
            int d = ReturnD(e, fi);

            myPrivateKey = new PrivateKey(d, n);
            myPublicKey = new PublicKey(e, n);

            MessageBox.Show("p:" + p + "\n" +
                             "q:" + q + "\n" +
                             "n:" + n + "\n" +
                             "fi:" + fi + "\n" +
                             "e:" + e + "\n" +
                             "d:" + d + "\n");
        }


        private int GetRandomPrimeNumber(Random random)
        {
            int currentNumber = random.Next(primeNumbersRangeFrom, primeNumbersRangeTo);

            bool primeNumberFound = false;
            while (primeNumberFound == false)
            {
                if (CheckPrimeNumber(currentNumber) == true)
                {
                    primeNumberFound = true;
                    continue;
                }
                else
                {
                    if ((currentNumber + 1) <= (primeNumbersRangeTo - 1))
                        currentNumber++;
                    else
                        currentNumber = primeNumbersRangeFrom;
                }
            }

            return currentNumber;
        }


        private bool CheckPrimeNumber(int number)
        {
            // liczba n jest pierwsza, gdy w przedziale od 2 do sqrt(n) nie ma liczby b, która n % b = 0

            if (number < 2)
                return false;

            for (int i = 2; i * i <= number; i++)
                if ((number % i) == 0)
                    return false;

            return true;
        }


        private int ReturnE(Random random, int n, int fi)
        {
            // liczba e musi być mniejsza od n i względnie pierwsza z fi

            // zapewnione e < n (random.Next(2, n)) oraz nie chcemy wylosować 1 (random od 2)
            int currentNumber = random.Next(2, n);

            bool eFound = false;
            while (eFound == false)
            {
                if (CheckE(currentNumber, fi) == true)
                {
                    eFound = true;
                    continue;
                }
                else
                {
                    if ((currentNumber + 1) <= (n - 1))
                        currentNumber++;
                    else
                        currentNumber = 2;
                }
            }

            return currentNumber;
        }


        private bool CheckE(int currentNumber, int fi)
        {
            // liczba e musi być względnie pierwsza z fi (NWD == 1)
            // korzystamy z alg. Euklidesa

            bool e_ok = false;

            while (currentNumber != 0 && fi != 0)
            {
                if (currentNumber > fi)
                    currentNumber %= fi;
                else
                    fi %= currentNumber;
            }

            if (currentNumber == 0)
            {
                if (fi == 1)
                    e_ok = true;
            }
            else if (fi == 0)
            {
                if (currentNumber == 1)
                    e_ok = true;
            }

            return e_ok;
        }


        private int ReturnD(int e, int fi)
        {
            // rozszerzony alg. Euklidesa == odwrotność modulo
            // raczej (e*d) mod fi = 1
            // Dla dwóch liczb naturalnych e i fi znaleźć taką liczbę naturalną d, aby (e*d) mod fi = 1 lub stwierdzić, że d nie istnieje
            // istnienie d mamy zapewnione z pkt 4

            // mamy 2 równania postaci:
            // 1) e*u + fi*v = w
            // 2) e*x + fi*y = z
            // podstawiamy: u=1, v=0, w=e, x=0, y=1, z=fi
            // pętla, powtarzamy, aż w==0
            ////// jeśli w < z to zamieniamy równania miejscami
            ////// obliczamy q = w div z
            ////// podstawiamy za: u -> u-q*x, v -> v-q*x, w -> w-q*z
            ////// wracamy do początku pętli
            // po otrzymaniu w==0 równania mają postać:
            // 1) e*u + fi*v = w (w==0)
            // 2) e*x + fi*y = z (z==NWD(e,fi))
            // jeżeli NWD(e,fi)=1 to istnieje odwrotność modulo fi z liczby e i jest równa x (nasze d)
            // jeżeli x jest ujemne to sprowadzamy do wartości dodatniej przez dodanie fi

            int u = 1, v = 0, w = e, x = 0, y = 1, z = fi;
            while (w != 0)
            {
                if (w < z)
                {
                    int tmp_u = u;
                    int tmp_v = v;
                    int tmp_w = w;
                    u = x;
                    v = y;
                    w = z;
                    x = tmp_u;
                    y = tmp_v;
                    z = tmp_w;
                }
                int q = w / z;
                u -= q * x;
                v -= q * y;
                w -= q * z;
            }

            if (z != 1) // taka sytuacja nie będzie miała miejsca
                throw new RSAException("Odwrotnosc modulo nie istnieje");

            if (x >= 0)
                return x;
            else
                return x + fi;
        }


        public string MakeMyPublicKeyString()
        {
            return myPublicKey.e + "&" + myPublicKey.n;
        }


        public void ReadTargetPublicKeyString(string keyString)
        {

            int separatorIndex = keyString.IndexOf('&');
            if ((separatorIndex == -1) || (separatorIndex == 0) || (separatorIndex == (keyString.Length - 1)))
                throw new RSAException("Klucz publiczny z komputera docelowego ma niewłaściwy format");

            string[] keyElements = keyString.Split('&');
            int e, n;
            try
            {
                e = int.Parse(keyElements[0]);
                n = int.Parse(keyElements[1]);
            }
            catch (Exception)
            {
                throw new RSAException("Klucz publiczny z komputera docelowego ma niewłaściwy format");
            }

            targetPublicKey = new PublicKey(e, n);
        }


        public string EnodeMessage(string message)
        {            

            List<int> charactersEncoded = new List<int>();
            foreach (char c in message)
                charactersEncoded.Add(FastModularExponentiation(c, targetPublicKey.e, targetPublicKey.n));

            string encodedMessage = "";
            foreach (int code in charactersEncoded)
            {
                string codeString = code.ToString();
                int codeStringLength = codeString.Length;
                if (codeStringLength == 3)
                    encodedMessage += codeString;
                else if (codeStringLength == 2)
                    encodedMessage += "0" + codeString;
                else if (codeString.Length == 1)
                    encodedMessage += "00" + codeString;
                //if (codeStringLength == 4)
                //    encodedMessage += codeString;
                //else if (codeStringLength == 3)
                //    encodedMessage += "0" + codeString;
                //else if (codeStringLength == 2)
                //    encodedMessage += "00" + codeString;
                //else if (codeString.Length == 1)
                //    encodedMessage += "000" + codeString;
            }

            return encodedMessage;
        }


        public string DecodeMessage(string message)
        {
            string decodedMessage = "";

            string encodedCharacter_string = "";
            foreach (char c in message)
            {
                encodedCharacter_string += c;
                //if (encodedCharacter_string.Length == 4)
                if (encodedCharacter_string.Length == 3)
                {
                    int encodedCharacter = int.Parse(encodedCharacter_string);
                    int decodedCharacter = FastModularExponentiation(encodedCharacter, myPrivateKey.d, myPrivateKey.n);
                    char character = (char)decodedCharacter;
                    decodedMessage += character;
                    encodedCharacter_string = "";
                }
            }

            return decodedMessage;
        }


        private static int FastModularExponentiation(int podstawa, int wykladnik, int modulo)
        {
            //szybkie potęgowanie modularne

            List<int> potegi = new List<int>();

            if (wykladnik >= 1)
                potegi.Add(1);

            if (wykladnik >= 2)
                for (int i = 2; i <= wykladnik; i *= 2)
                    potegi.Add(i);

            List<int> potegi_tworzace = new List<int>();

            int liczba = 0;
            for (int i = (potegi.Count - 1); i >= 0; i--)
            {
                if (liczba + potegi[i] == wykladnik)
                {
                    liczba += potegi[i];
                    potegi_tworzace.Add(potegi[i]);
                    break;
                }
                else if (liczba + potegi[i] < wykladnik)
                {
                    liczba += potegi[i];
                    potegi_tworzace.Add(potegi[i]);
                    continue;
                }
                else
                    continue;
            }

            Dictionary<int, double> wyniki = new Dictionary<int, double>();

            double poprzedni_wynik = -1;
            foreach (int potega in potegi)
            {
                if (poprzedni_wynik != -1)
                {
                    double wynik = Math.Pow(poprzedni_wynik, 2) % modulo;
                    if (potegi_tworzace.Contains(potega))
                        wyniki.Add(potega, wynik);
                    poprzedni_wynik = wynik;
                }
                else
                {
                    double wynik = Math.Pow(podstawa, 1) % modulo;
                    if (potegi_tworzace.Contains(potega))
                        wyniki.Add(potega, wynik);
                    poprzedni_wynik = wynik;
                }
            }


            double rezultat = 1;
            foreach (KeyValuePair<int, double> para in wyniki)
                rezultat *= para.Value;

            rezultat %= modulo;

            return (int)rezultat;
        }

    }


    class PrivateKey
    {
        public int d, n;

        public PrivateKey(int d, int n)
        {
            this.d = d;
            this.n = n;
        }
    }


    class PublicKey
    {
        public int e, n;

        public PublicKey(int e, int n)
        {
            this.e = e;
            this.n = n;
        }
    }
}


