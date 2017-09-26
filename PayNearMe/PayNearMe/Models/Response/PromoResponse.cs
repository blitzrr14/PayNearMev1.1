using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Models.Response
{
    public class PromoResponse
    {
        public Int32 respcode { get; set; }
        public String message { get; set; }

        public String promoName { get; set; }
        public Double newCharge { get; set; }
        public Double newTotal { get; set; }
        public Double discount { get; set; }
    }
}