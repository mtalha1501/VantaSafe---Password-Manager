using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations.Schema;

namespace Vanta_Safe.Models
{
    public class Credential
    {
        public int Id { get; set; }
        public int UserId { get; set; }  // Links to Users table

        // All fields encrypted with user's AES key
        public byte[] EncryptedSiteName { get; set; }
        public byte[] EncryptedSiteUrl { get; set; }
        public byte[] EncryptedUsername { get; set; }
        public byte[] EncryptedPassword { get; set; }

    }

}
