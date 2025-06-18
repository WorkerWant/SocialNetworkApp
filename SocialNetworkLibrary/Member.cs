using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Represents a registered user of the social network.
    /// </summary>
    public class Member : Person, IFriendManager
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Member"/> class.
        /// </summary>
        /// <param name="id">Unique identifier.</param>
        /// <param name="name">Display name.</param>
        /// <param name="email">E-mail address.</param>
        public Member(int id, string name, string email)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Email = email ?? throw new ArgumentNullException(nameof(email));

            RegistrationDate = DateTime.UtcNow;
            FriendRelationships = new List<FriendRelationship>();
            SentRequests = new List<FriendRequest>();
            ReceivedRequests = new List<FriendRequest>();
            Groups = new List<Group>();
            IsActive = true;
        }

        /// <summary>Gets the UTC date and time when the member registered.</summary>
        public DateTime RegistrationDate { get; }

        /// <summary>Gets a value indicating whether the member account is active.</summary>
        public bool IsActive { get; private set; }

        /// <summary>Gets established friendship relations.</summary>
        public List<FriendRelationship> FriendRelationships { get; }

        /// <summary>Gets outgoing friend requests.</summary>
        public List<FriendRequest> SentRequests { get; }

        /// <summary>Gets incoming friend requests.</summary>
        public List<FriendRequest> ReceivedRequests { get; }

        /// <summary>Gets groups in which the member participates.</summary>
        public List<Group> Groups { get; }

        #region IFriendManager implementation

        /// <inheritdoc/>
        public void AddFriend(Member friend)
        {
            _ = friend ?? throw new ArgumentNullException(nameof(friend));

            if (GetFriends().Any(f => f.Id == friend.Id))
                throw new InvalidOperationException("A friendship with this user already exists.");

            FriendRelationships.Add(new FriendRelationship(this, friend));
            friend.FriendRelationships.Add(new FriendRelationship(friend, this));
        }

        /// <inheritdoc/>
        public void RemoveFriend(Member friend)
        {
            _ = friend ?? throw new ArgumentNullException(nameof(friend));

            var relation = FriendRelationships.FirstOrDefault(r => r.Involves(friend));
            if (relation is null)
                throw new InvalidOperationException("The friendship does not exist.");

            FriendRelationships.Remove(relation);

            var reciprocal = friend.FriendRelationships.FirstOrDefault(r => r.Involves(this));
            if (reciprocal is not null)
                friend.FriendRelationships.Remove(reciprocal);
        }

        /// <inheritdoc/>
        public List<Member> GetFriends() =>
            FriendRelationships
                .Select(r => r.GetOther(this))
                .Distinct()
                .ToList();

        #endregion

        #region Friend-request logic

        /// <summary>
        /// Sends a friend request to another member.
        /// </summary>
        /// <param name="receiver">Receiver of the request.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="receiver"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If users are already friends or a pending request exists.</exception>
        public void SendFriendRequest(Member receiver)
        {
            _ = receiver ?? throw new ArgumentNullException(nameof(receiver));

            if (GetFriends().Any(f => f.Id == receiver.Id))
                throw new InvalidOperationException("A friendship with this user already exists.");

            if (SentRequests.Any(r => r.Receiver.Id == receiver.Id && r.Status == RequestStatus.Pending))
                throw new InvalidOperationException("A friend request has already been sent.");

            var request = new FriendRequest(this, receiver);
            SentRequests.Add(request);
            receiver.ReceivedRequests.Add(request);
        }

        /// <summary>
        /// Accepts an incoming friend request.
        /// </summary>
        /// <param name="request">Request to accept.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the request cannot be processed.</exception>
        public void AcceptFriendRequest(FriendRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            if (!ReceivedRequests.Contains(request))
                throw new InvalidOperationException("The request was not found among incoming requests.");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("The request has already been processed.");

            request.Status = RequestStatus.Accepted;
            AddFriend(request.Sender);
        }

        /// <summary>
        /// Declines an incoming friend request.
        /// </summary>
        /// <param name="request">Request to decline.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="request"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the request cannot be processed.</exception>
        public void DeclineFriendRequest(FriendRequest request)
        {
            _ = request ?? throw new ArgumentNullException(nameof(request));

            if (!ReceivedRequests.Contains(request))
                throw new InvalidOperationException("The request was not found among incoming requests.");

            if (request.Status != RequestStatus.Pending)
                throw new InvalidOperationException("The request has already been processed.");

            request.Status = RequestStatus.Declined;
        }

        #endregion

        #region Group helpers

        /// <summary>Joins the specified group.</summary>
        /// <param name="group">Group to join.</param>
        public void JoinGroup(Group group) => group?.AddMember(this);

        /// <summary>Leaves the specified group.</summary>
        /// <param name="group">Group to leave.</param>
        public void LeaveGroup(Group group)
        {
            _ = group ?? throw new ArgumentNullException(nameof(group));

            if (group.Owner.Id == Id)
                throw new InvalidOperationException("Owner cannot leave the group.");

            group.RemoveMember(this);
        }

        #endregion

        public override string ToString()
        {
            return $"Користувач: {this.Name}";
        }
    }
}
