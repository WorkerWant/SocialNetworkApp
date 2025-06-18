using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Base class that supplies identity, name and e-mail.
    /// </summary>
    public abstract class Person
    {
        /// <summary>Gets or sets the unique identifier.</summary>
        public int Id { get; set; }

        /// <summary>Gets or sets the person’s display name.</summary>
        public string Name { get; set; } = string.Empty;

        /// <summary>Gets or sets the person’s e-mail address.</summary>
        public string Email { get; set; } = string.Empty;
    }
}
