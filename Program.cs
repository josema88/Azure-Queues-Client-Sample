using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Azure.Storage.Queues;
using Azure.Storage.Queues.Models;

namespace QueueClientApp
{
    public static class Program
    {
        static async Task Main(string[] args)
        {
            //var connectionString = Environment.GetEnvironmentVariable("AZURE_STORAGE_CONNECTION_STRING");
            //var connectionString = "UseDevelopmentStorage=true";
            var connectionString = "YOUR_CONNECTION_STRING";
            
            QueueClient queueClient = new QueueClient(connectionString, "pending-orders");

            //Insert messages
            await InsertMessagesToQueue(queueClient);

            //Dequeue messages
            //await DequeueMessage(queueClient);

         }

        static async Task InsertMessagesToQueue(QueueClient queueClient)
        {
            string path = @"Assets/Messages.txt";
            var readText = await GetTextFromFile(path);
            var counter = 0;
            foreach (string newMessage in readText)
            {

                //await Task.Delay(3000);
                await queueClient.SendMessageAsync(newMessage.EncodeTo64(), default, TimeSpan.FromSeconds(-1), default);
                counter++;
                Console.WriteLine("Message Sent: " + counter);
            }
        }

        static async Task PeekMessage(QueueClient queueClient)
        {

            if (queueClient.Exists())
            {
                // Peek at the next message
                PeekedMessage[] peekedMessage = await queueClient.PeekMessagesAsync();

                // Display the message
                Console.WriteLine($"Peeked message: '{peekedMessage[0].MessageText.DecodeFrom64()}'");
            }
        }

        static async Task DequeueMessage(QueueClient queueClient)
        {

            if (queueClient.Exists())
            {
                // Get the next message
                QueueMessage[] retrievedMessage = await queueClient.ReceiveMessagesAsync();

                // Process (i.e. print) the message in less than 30 seconds
                Console.WriteLine($"Dequeued message: '{retrievedMessage[0].MessageText.DecodeFrom64()}'");

                // Delete the message
                await queueClient.DeleteMessageAsync(retrievedMessage[0].MessageId, retrievedMessage[0].PopReceipt);
            }
        }

        static public string EncodeTo64(this string toEncode)
        {

            byte[] toEncodeAsBytes = System.Text.ASCIIEncoding.ASCII.GetBytes(toEncode);

            string returnValue = System.Convert.ToBase64String(toEncodeAsBytes);

            return returnValue;

        }

        static public string DecodeFrom64(this string toDecode)
        {
            byte[] data = System.Convert.FromBase64String(toDecode);
            var base64Decoded = System.Text.ASCIIEncoding.ASCII.GetString(data);
            return base64Decoded;
        }

        static async Task<string[]> GetTextFromFile(string path)
        {

            // Open the file to read from.
            return File.ReadAllLines(path);
        }

        static async Task InsertMessageAsync(QueueClient theQueue, string newMessage)
        {
            if (null != await theQueue.CreateIfNotExistsAsync())
            {
                Console.WriteLine("The queue was created.");
            }

            await theQueue.SendMessageAsync(newMessage);
        }
    }
}
