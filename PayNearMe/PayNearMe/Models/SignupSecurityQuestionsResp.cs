using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace MLUniteller.Content.Helper
{

    public class SignupSecurityQuestionsResp
    {
        public string requestTimestamp { get; set; }
        public string responseTimestamp { get; set; }
        public int executionTimestamp { get; set; }
        public string interactionId { get; set; }
        public ErrorCode errorCode { get; set; }
        public object extraFields { get; set; }
        public List<SecurityQuestion> securityQuestion { get; set; }
    }

    public class SecurityQuestion
    {
        public int questionId { get; set; }
        public string question { get; set; }
        public object questionHint { get; set; }
    }
}

