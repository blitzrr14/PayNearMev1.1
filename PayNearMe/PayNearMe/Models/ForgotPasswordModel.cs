using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.ComponentModel.DataAnnotations;

namespace PayNearMe.Models
{
    public class ForgotPasswordModel
    {
        [Required]
        public String email { get; set; }

        [Required]
        public String fpNewPassword { get; set; }

        [Required]
        public String fpConfirmPassword { get; set; }

        [Required]
        public String securityCode { get; set; }
    }

    public class ForgotPasswordModelResponse
    {
        public ForgotPasswordModel userData { get; set; }
        public Int32 code { get; set; }
        public String message { get; set; }
        public String encCID { get; set; }
        public String encFN { get; set; }
        public Boolean isForgot { get; set; }
    }
}