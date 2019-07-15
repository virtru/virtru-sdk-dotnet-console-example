using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Virtru.Sdk;
using Virtru.Sdk.Implementations.Builders;
namespace virtru_sdk_dotnet_console_example
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                var userId = "<userId smtpAddress>"; //e.g. user@domain.com
                var appId = "<appId guid>";          //e.g. C006AD5E-68C2-4130-B193-BD6789C185EA

                //one-time setup/configure the VirtruClient
                var develop01Config = new TDFConfigurationBuilder()
                                      .WithAcmUri("https://api-develop01.develop.virtru.com/acm/")
                                      .WithAccountsUri("https://api-develop01.develop.virtru.com/accounts/")
                                      .WithUserId(userId)
                                      .WithAppId(appId)
                                      .WithUserAgent("virtru_netsdk_console_sample")
                                      .Build();

                //only one instance of the VirtruClient is needed
                var virtruClient = new VirtruClient(develop01Config);

                //create TDF/encrypt sample text
                var Sample1TxtBytes = Encoding.UTF8.GetBytes("Hello world! This is my sample text to encrypt.");

                var plainSource = new MemoryStream(Sample1TxtBytes);
                var encryptParams = new EncryptFileParamBuilder()
                                    .WithOwner(userId)
                                    .WithAuthorizedUsers(new[] { userId }.ToList())
                                    .WithFileName("testing.txt")
                                    .WithSource(plainSource)
                                    .WithDestination(new MemoryStream())
                                    .Build();

                var cipherStream = virtruClient.Encrypt(encryptParams).GetAwaiter().GetResult();


                //decrypt TDF/decrypt sample text
                var decryptParams = new DecryptFileParamBuilder()
                                    .WithSource(cipherStream)
                                    .WithDestination(new MemoryStream())
                                    .Build();

                var decryptedStream = virtruClient.Decrypt(decryptParams).GetAwaiter().GetResult();


                //verify the decrypted data is identical to the source data
                using (var md5 = MD5.Create())
                {
                    var sourceHash = md5.ComputeHash(plainSource);
                    var destHash = md5.ComputeHash(decryptedStream);

                    if (Convert.ToBase64String(sourceHash) == Convert.ToBase64String(destHash))
                    {
                        Console.WriteLine("The TDF created and then decrypted successfully!");
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
            }

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
    }
}
