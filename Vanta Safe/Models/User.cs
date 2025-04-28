using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Vanta_Safe.Models
{
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; }  // Unique login identifier
        public string MasterKeyHash { get; set; }  // BCrypt hash of (masterPassword + DeviceSecret)
        public string DeviceSecret { get; set; }  // Random 16-char string
        public int FailedAttempts { get; set; } = 0;

    }
}
