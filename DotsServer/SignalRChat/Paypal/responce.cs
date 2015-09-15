using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRChat.Paypal
{
    public class Responce
    {
        public string State { get; set; }
        public string Id { get; set; }
        public DateTime Create_Time { get; set; }
        public string Intent { get; set; }

        public Responce()
        {
                
        }
    }
}