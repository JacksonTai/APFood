﻿using System.ComponentModel.DataAnnotations;

namespace APFood.Models.Register
{
    public class CustomerRegistrationModel : RegistrationModel
    {
        [Required]
        [Display(Name = "Full Name")]
        public required string FullName { get; set; }

    }

}
