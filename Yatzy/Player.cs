using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Yatzy
{
    public enum Status
    {
        Waiting,
        Playing,
        AFK
    }

    public class Player
    {
        //[JsonProperty("username")]
        public string UserName { get; set; }

        public Status Status { get; set; } = Status.Waiting;
    }
}
