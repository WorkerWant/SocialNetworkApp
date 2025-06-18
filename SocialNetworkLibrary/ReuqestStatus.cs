using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Defines possible states of a friend request.
    /// </summary>
    public enum RequestStatus
    {
        /// <summary>The request is waiting for the receiver’s action.</summary>
        Pending,

        /// <summary>The receiver accepted the request.</summary>
        Accepted,

        /// <summary>The receiver declined the request.</summary>
        Declined
    }
}
