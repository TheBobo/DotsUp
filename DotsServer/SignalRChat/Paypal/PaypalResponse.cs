using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace SignalRChat.Paypal
{
    public class PaypalResponse
    {
        public Responce Response { get; set; }
        public Client client { get; set; }
        public PaypalResponse()
        {

        }
    }
}