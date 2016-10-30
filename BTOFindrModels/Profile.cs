using System;

namespace BTOFindr.Models
{
    /// <summary>
    /// This describes a user's Profile.
    /// </summary>
    public class Profile
    {
        public String postalCode { get; set; }
        public decimal income { get; set; }
        public decimal currentCpf { get; set; }
        public decimal monthlyCpf { get; set; }
        public int loanTenure { get; set; }
    }
}