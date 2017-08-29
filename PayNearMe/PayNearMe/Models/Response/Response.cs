using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace PayNearMe.Content.Helper
{

    public class Response
    {
        public Int32 respcode { get; set; }
        public String message { get; set; }
        public String activationCode { get; set; }

    }

    public class SecurityQuestion
    {
        public int questionId { get; set; }
        public string question { get; set; }
        public object questionHint { get; set; }
    }
}

