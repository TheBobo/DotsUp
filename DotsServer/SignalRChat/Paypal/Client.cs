using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRChat.Paypal
{
    public class Client
    {
        public string Platform { get; set; }
        public string Paypal_Sdk_Version { get; set; }
        public string Product_Name { get; set; }
        public string Environment { get; set; }

        public Client()
        {

        }
            
    }
}