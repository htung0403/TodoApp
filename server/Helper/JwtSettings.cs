using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Server.Helper
{
    public class JwtSettings
    {
        public string Key { get; set; } = string.Empty;
        public string Issuer { get; set; } = string.Empty;
        public string Audience { get; set; } = string.Empty;
        public int ExpirationHours { get; set; } = 24;
        public int RefreshTokenExpirationDays { get; set; } = 7;
        
        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Key) && 
                   !string.IsNullOrEmpty(Issuer) && 
                   !string.IsNullOrEmpty(Audience) &&
                   Key.Length >= 32; // Minimum 256 bits
        }
        
        public TimeSpan GetExpirationTimeSpan()
        {
            return TimeSpan.FromHours(ExpirationHours);
        }
        
        public DateTime GetExpirationDateTime()
        {
            return DateTime.UtcNow.Add(GetExpirationTimeSpan());
        }
        
        public TimeSpan GetRefreshTokenExpirationTimeSpan()
        {
            return TimeSpan.FromDays(RefreshTokenExpirationDays);
        }
    }
}