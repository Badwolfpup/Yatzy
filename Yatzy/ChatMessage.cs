using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yatzy
{
    public class ChatMessage
    {
        public DateTime TimeStamp { get; } = DateTime.Now;

        public string Sender { get; set; }

        public string Message { get; set; }
    }
}
