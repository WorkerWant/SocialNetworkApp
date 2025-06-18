using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Represents a friend request between two members.
    /// </summary>
    public class FriendRequest
    {
        private static int _nextId = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="FriendRequest"/> class.
        /// </summary>
        /// <param name="sender">Sending member.</param>
        /// <param name="receiver">Receiving member.</param>
        public FriendRequest(Member sender, Member receiver)
        {
            Id = _nextId++;
            Sender = sender ?? throw new ArgumentNullException(nameof(sender));
            Receiver = receiver ?? throw new ArgumentNullException(nameof(receiver));
            Status = RequestStatus.Pending;
        }

        /// <summary>Gets the unique request identifier.</summary>
        public int Id { get; }

        /// <summary>Gets the request sender.</summary>
        public Member Sender { get; }

        /// <summary>Gets the request receiver.</summary>
        public Member Receiver { get; }

        /// <summary>Gets or sets the current request status.</summary>
        public RequestStatus Status { get; set; }
    }
}
