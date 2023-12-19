using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Model
{
    class ChatSummary
    {
        public string FriendUsername { get; set; }
        public DateTime LastMessageDate { get; set; }
        public string Filename { get; set; }

        public string DisplayTextHistoricChat
        {
            get
            {
                return $"{FriendUsername} - {LastMessageDate.ToString("yyyy-MM-dd-HH-mm-ss")}";
            }
        }
    }
}
