﻿using APFood.Constants;

namespace APFood.Models.DeliveryTask
{
    public class DeliveryTaskViewModel
    {
        public required List<DeliveryTaskListViewModel> DeliveryTaskList { get; set; }
        public required Dictionary<DeliveryStatus, int> DeliveryTaskCounts { get; set; }
        public required DeliveryStatus CurrentStatus { get; set; }
        public required decimal TotalPointsEarned { get; set; } = 0;
        public bool IsAdmin { get; set; }
    }
}
