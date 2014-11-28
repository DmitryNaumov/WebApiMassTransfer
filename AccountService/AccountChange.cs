using System;

namespace AccountService
{
    public class AccountChange
    {
        public int AccountId { get; set; }

        public double Change { get; set; }

        public DateTime When { get; set; }

        public double[] Payload { get; set; }
    }
}