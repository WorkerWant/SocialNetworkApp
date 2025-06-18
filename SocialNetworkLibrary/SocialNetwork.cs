using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Root entity that aggregates members and groups.
    /// </summary>
    public class SocialNetwork
    {
        private int _nextGroupId = 1;

        /// <summary>
        /// Initializes a new instance of the <see cref="SocialNetwork"/> class.
        /// </summary>
        public SocialNetwork()
        {
            Members = new List<Member>();
            Groups = new List<Group>();
        }

        /// <summary>Gets registered members.</summary>
        public List<Member> Members { get; }

        /// <summary>Gets existing groups.</summary>
        public List<Group> Groups { get; }

        /// <summary>
        /// Registers a new member in the network.
        /// </summary>
        /// <param name="member">Member to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="member"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the identifier is already used.</exception>
        public void AddMember(Member member)
        {
            _ = member ?? throw new ArgumentNullException(nameof(member));

            if (Members.Any(m => m.Id == member.Id))
                throw new InvalidOperationException("A member with this identifier already exists.");

            Members.Add(member);
        }

        /// <summary>
        /// Creates a new group in the network.
        /// </summary>
        /// <param name="name">Group name.</param>
        /// <param name="owner">Group owner.</param>
        /// <returns>Created group instance.</returns>
        /// <exception cref="ArgumentNullException">If <paramref name="name"/> or <paramref name="owner"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the name is already taken.</exception>
        public Group CreateGroup(string name, Member owner)
        {
            _ = name ?? throw new ArgumentNullException(nameof(name));
            _ = owner ?? throw new ArgumentNullException(nameof(owner));

            if (Groups.Any(g => g.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                throw new InvalidOperationException("Group name already taken.");

            var group = new Group(_nextGroupId++, name, owner);
            Groups.Add(group);
            return group;
        }

        /// <summary>
        /// Removes a member (and cleans up all related data).
        /// </summary>
        /// <param name="memberId">Identifier of the member to remove.</param>
        /// <exception cref="InvalidOperationException">If the member is not found.</exception>
        public void RemoveMember(int memberId)
        {
            var member = Members.FirstOrDefault(m => m.Id == memberId);
            if (member is null)
                throw new InvalidOperationException("Member not found.");

            foreach (var g in member.Groups.ToList())
                g.RemoveMember(member);

            foreach (var f in member.GetFriends().ToList())
                member.RemoveFriend(f);

            Members.Remove(member);
        }
    }
}
