﻿using APFood.Constants.Order;
using System.ComponentModel.DataAnnotations;

namespace APFood.Models.Cart
{
    public class CartFormModel
    {
        public DineInOption DineInOption { get; set; } = DineInOption.Pickup;

        [Required(ErrorMessage = "Location is required for delivery.")]
        public string? Location { get; set; }
        public bool IsUsingRunnerPoints { get; set; }
    }
}
