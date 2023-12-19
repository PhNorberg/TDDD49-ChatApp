using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ChatApp.Model
{   
    public enum RequestType
    {   
        None,
        ConnectionRequestAccepted,
        ConnectionRequestDenied,
        CloseConnection,
        ChatMessage,
        Buzz
    }
    public class Data
    {
        private string message;
        private RequestType requestType;
        private string name;
        private DateTime date;
        public Data() { }   

        public string Message
        {
            get { return message; }
            set { message = value; }
        }
        public string Name
        {
            get { return name; }
            set { name = value; }
        }
        
        public RequestType RequestType { get { return requestType; } set { requestType = value; } }
        public DateTime Date { get { return date; } set {  date = value; } }
    }
}
