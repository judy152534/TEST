using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace TEST.DTOs
{
    public class RegisterDto
    {
        [Required][EmailAddress]
        public string Email { get; set; }
        [Required][RegularExpression("a-zA-Z0-9")]
        public string Password { get; set; }
    }
}
