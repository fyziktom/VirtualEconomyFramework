using System;
using System.IO;
using System.Text;

namespace VECryptographyUtilities
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("VE Cryptography Utility 1.0 ");
            Console.WriteLine("---------------------");

            var end = false;

            while (!end)
            {
                Console.WriteLine("---------------------");
                Console.WriteLine("Select option:");
                Console.WriteLine("1. Decrypt Key");
                Console.WriteLine("2. Encrypt Key");
                Console.WriteLine("4. Exit");
                Console.WriteLine("Please type number of option and hit enter:");
                var opt = Console.ReadLine();

                if (opt.Contains('1'))
                {
                    DecryptKey();
                }
                else if (opt.Contains('2'))
                {
                    EncryptKey();
                }
                else if (opt.Contains('4'))
                {
                    end = true;
                }
            }
        }

        static void DecryptKey()
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("Decrypting the Key");
            Console.WriteLine("---------------------");
            Console.WriteLine("Please path to the file with encrypted key:");
            var keyfile = Console.ReadLine();
            Console.WriteLine("Please input your password:");
            var password = Console.ReadLine();

            if (!string.IsNullOrEmpty(keyfile) && !string.IsNullOrWhiteSpace(password))
            {
                try
                {
                    var key = FileHelpers.ReadTextFromFile(keyfile);

                    if (key == null)
                    {
                        throw new Exception("Cannot read the file!");
                    }

                    var deckey = SymetricProvider.DecryptString(password, key);
                    if (deckey != null)
                    {
                        Console.WriteLine("----------------------");
                        Console.WriteLine("Your Decrypted Key is:");
                        Console.WriteLine();
                        Console.WriteLine(deckey);
                        var pth = Path.GetDirectoryName(keyfile);
                        FileHelpers.WriteTextToFile(Path.Combine(pth, "Deckey-" + FileHelpers.GetDateTimeString() + ".txt"), deckey);
                        Console.WriteLine("----------------------");
                    }
                    else
                    {
                        Console.WriteLine("---------!!!!---------");
                        Console.WriteLine("Cannot Decrypt the Key");
                        Console.WriteLine("----------------------");
                    }
                }
                catch(Exception ex)
                {
                    Console.WriteLine("---------!!!!---------");
                    Console.WriteLine("Cannot Decrypt the Key");
                    Console.WriteLine("----------------------");
                }

            }
            else
            {
                Console.WriteLine("Wrong input, please try it again.");
            }
        }

        static void EncryptKey()
        {
            Console.WriteLine("---------------------");
            Console.WriteLine("Encrypting the Key");
            Console.WriteLine("---------------------");
            Console.WriteLine("Please input path to the file with decrypted key:");
            var keyfile = Console.ReadLine();
            Console.WriteLine("Please input your password:");
            var password = Console.ReadLine();

            if (!string.IsNullOrEmpty(keyfile) && !string.IsNullOrWhiteSpace(password))
            {

                try
                {

                    var key = FileHelpers.ReadTextFromFile(keyfile);

                    if (key == null)
                    {
                        throw new Exception("Cannot read the file!");
                    }

                    var enckey = SymetricProvider.EncryptString(password, key);
                    if (enckey != null)
                    {
                        Console.WriteLine("----------------------");
                        Console.WriteLine("Your Encrypted Key is:");
                        Console.WriteLine();
                        Console.WriteLine(enckey);
                        var pth = Path.GetDirectoryName(keyfile);
                        FileHelpers.WriteTextToFile(Path.Combine(pth, "Enckey-" + FileHelpers.GetDateTimeString() + ".txt"), enckey);
                        Console.WriteLine("----------------------");
                    }
                    else
                    {
                        Console.WriteLine("---------!!!!---------");
                        Console.WriteLine("Cannot Encrypt the Key");
                        Console.WriteLine("----------------------");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("---------!!!!---------");
                    Console.WriteLine("Cannot Encrypt the Key");
                    Console.WriteLine("----------------------");
                }

            }
            else
            {
                Console.WriteLine("Wrong input, please try it again.");
            }
        }
    }
}
