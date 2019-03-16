using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace WindowsHello.Domain.Models
{
    public class User
    {
        [Key, Required]
        public Guid UserId { get; set; }

        

        [Required]
        public string Username { get; set; }
        public string Password { get; set; }
        public List<PassportDevice> PassportDevices = new List<PassportDevice>();

       
    }
}
