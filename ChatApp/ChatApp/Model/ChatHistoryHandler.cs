using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace ChatApp.Model
{
    internal class ChatHistoryHandler
    {

        public List<ChatSummary> LoadHistoricalChats(string username)
        {
            List<ChatSummary> _historicChatSummary = new List<ChatSummary>();

            string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string chatHistoryFolder = "TDDD49-chats";
            string dirPath = Path.Combine(documentPath, chatHistoryFolder);

            var chatFiles = Directory.GetFiles(dirPath, "*_and_*.json");

            foreach (var chatFile in chatFiles)
            {
                string filename = Path.GetFileName(chatFile);

                var parts = filename.Split(new[] { "_and_", "_", ".json" }, StringSplitOptions.RemoveEmptyEntries);

                bool containsUsername = parts.Any(part => part.Equals(username));
                if (!containsUsername)
                {
                    continue;
                }


                string friendUsername = username == parts[0] ? parts[1] : parts[0];
                string dateString = parts[2];
                DateTime lastMessageDate;

                DateTime.TryParseExact(dateString, "yyyyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.None, out lastMessageDate);

                var chatSummary = new ChatSummary
                {
                    FriendUsername = friendUsername,
                    LastMessageDate = lastMessageDate,
                    Filename = filename
                };

                _historicChatSummary.Add(chatSummary);

            }
            System.Diagnostics.Debug.WriteLine("am in chathistoryhandler end");
            var sortedSummaries = _historicChatSummary.OrderByDescending(summary => summary.LastMessageDate).ToList();
            return sortedSummaries;

        }

        public async Task<List<Data>> LoadChatHistoryAsync(ChatSummary chatSummary)
        {
            List<Data> chatHistory = new List<Data>();

            string friendUsername = chatSummary.FriendUsername;
            string chatDate = chatSummary.LastMessageDate.ToString("yyyyMMddHHmmss");
            string documentPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            string chatHistoryFolder = "TDDD49-chats";
            string dirPath = Path.Combine(documentPath, chatHistoryFolder);

            var chatFiles = Directory.GetFiles(dirPath, "*_and_*.json");

            foreach (var chatFile in chatFiles)
            {
                string filename = Path.GetFileName(chatFile);

                if (filename.Equals(chatSummary.Filename))
                {
                    string fullPath = Path.Combine(dirPath, filename);
                    string jsonContent = await File.ReadAllTextAsync(fullPath);
                    chatHistory = JsonSerializer.Deserialize<List<Data>>(jsonContent);
                    break;
                }
            }
            return chatHistory;
        }
    }
}
