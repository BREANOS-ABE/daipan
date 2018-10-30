//-----------------------------------------------------------------------

// <copyright file="User.cs" company="Breanos GmbH">
// Copyright Notice:
// DAIPAN - This file, program or part of a program is considered part of the DAIPAN framework by Breanos GmbH for Industrie 4.0
// Published in 2018 by Gerhard Eder gerhard.eder@breanos.com and Achim Bernhard achim.bernhard@breanos.com
// To the extent possible under law, the publishers have dedicated all copyright and related and neighboring rights to this software to the public domain worldwide. This software is distributed without any warranty.
// You should have received a copy of the CC0 Public Domain Dedication along with this software. If not, see <http://creativecommons.org/publicdomain/zero/1.0/>.
// <date>Tuesday, October 30, 2018 1:26:47 PM</date>
// </copyright>

//-----------------------------------------------------------------------

using IdentityProvider.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace IdentityProviderModel
{
    [Table("User", Schema = "BIP")]
    public class User
    {
        /// <summary>
        /// Create a new user without a creator.
        /// </summary>
        public User()
        {
            Salt = GetNewSalt();
            CreatedBy = "";
            CreatedAt = DateTime.UtcNow;
            Groups = new HashSet<UserGroup>();
            LoginAttempts = new HashSet<LoginAttempt>();
        }
        /// <summary>
        /// Creates a new user with creator information
        /// </summary>
        /// <param name="createdBy">The user creating this new user</param>
        public User(string createdBy) : this()
        {
            CreatedBy = createdBy;
        }
        public User(string createdBy, string userIdentifier, string password = "") : this(createdBy)
        {
            UserIdentifier = userIdentifier;
            Password = GetPassword(password, Salt);
        }
        [Key]
        [DatabaseGenerated(System.ComponentModel.DataAnnotations.Schema.DatabaseGeneratedOption.Identity)]
        public long UserId { get; private set; }
        [StringLength(128)]
        [Required]
        /* [Index("IX_User_UserIdentifier", IsUnique = true)] */
        public string UserIdentifier { get; set; }
        [StringLength(48)]
        [Required]
        public string Password { get; set; }
        [StringLength(16)]
        [Required]
        public string Salt { get; private set; }
        [StringLength(128)]
        public string FirstName { get; set; }
        [StringLength(128)]
        public string LastName { get; set; }
        [StringLength(128)]
        public string Domain { get; set; }
        [StringLength(128)]
        public string SID { get; set; }
        [Required]
        public DateTime CreatedAt { get; private set; }
        [Required]
        [StringLength(128)]
        public string CreatedBy { get; private set; }
        public DateTime FirstLogin { get; private set; }
        public DateTime LastLogin { get; private set; }
        public DateTime LastModifiedAt { get; private set; }
        public string LastModifiedBy { get; private set; }
        public virtual ICollection<UserGroup> Groups { get; set; }
        public virtual ICollection<LoginAttempt> LoginAttempts { get; set; }
        public List<Group> GetGroups()
        {
            return Groups.Select(ug => ug.Group).ToList();
        }

        

        public bool SetPassword(string newCleartext)
        {
            Password = GetPassword(newCleartext, Salt);
            return true;
        }

        /// <summary>
        /// Get a 16-char string consisting of a-z
        /// </summary>
        /// <returns>The salt string</returns>
        private static string GetNewSalt()
        {
            string ret = "";
            Random rnd = new Random();
            byte[] bytes = new byte[16];
            for (int i = 0; i < 16; i++)
            {
                bytes[i] = (byte)rnd.Next(0x61, 0x7a); // a - z
            }
            ret = ASCIIEncoding.ASCII.GetString(bytes);
            return ret;
        }

        public void UpdateLoginTimestamps()
        {
            var now = DateTime.UtcNow;
            LastLogin = now;
            if (FirstLogin == null) FirstLogin = now;
        }

        

        /// <summary>
        /// calculates a password hash from a cleartext password and salt
        /// </summary>
        /// <param name="cleartext">the cleartext password</param>
        /// <param name="salt">the salt</param>
        /// <returns>the 48-char ascii representation of the 384-bit SHA checksum</returns>
        public static string GetPassword(string cleartext, string salt)
        {
            return ASCIIEncoding.ASCII.GetString(SHA384.Create().ComputeHash(ASCIIEncoding.ASCII.GetBytes(cleartext + salt)));
        }
    }
}
