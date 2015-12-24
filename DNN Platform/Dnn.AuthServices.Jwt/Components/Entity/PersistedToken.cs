using System;

namespace Dnn.AuthServices.Jwt.Components.Entity
{
    [Serializable]
    public class PersistedToken
    {
        public string TokenId { get; set; }
        public int UserId { get; set; }
        public int RenewCount { get; set; }
        public DateTime TokenExpiry { get; set; }
        public DateTime RenewalExpiry { get; set; }
        public string TokenHash { get; set; }
        public string RenewalHash { get; set; }
    }
}