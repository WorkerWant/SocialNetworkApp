using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Represents a community inside the social network.
    /// </summary>
    public class Group
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class.
        /// </summary>
        /// <param name="id">Unique group identifier.</param>
        /// <param name="name">Group name.</param>
        /// <param name="owner">Member who owns the group.</param>
        public Group(int id, string name, Member owner)
        {
            Id = id;
            Name = name ?? throw new ArgumentNullException(nameof(name));
            Owner = owner ?? throw new ArgumentNullException(nameof(owner));

            _members = new List<Member> { owner };
            owner.Groups.Add(this);
        }

        /// <summary>Gets the group's unique identifier.</summary>
        public int Id { get; }

        /// <summary>Gets the group's name.</summary>
        public string Name { get; }

        /// <summary>Gets the member who owns the group.</summary>
        public Member Owner { get; }

        private readonly List<Member> _members;

        /// <summary>Gets the current group members.</summary>
        public IReadOnlyCollection<Member> Members => _members.AsReadOnly();

        /// <summary>
        /// Adds a member to the group.
        /// </summary>
        /// <param name="member">Member to add.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="member"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the member is already in the group.</exception>
        public void AddMember(Member member)
        {
            _ = member ?? throw new ArgumentNullException(nameof(member));

            if (_members.Any(m => m.Id == member.Id))
                throw new InvalidOperationException("Member already in the group.");

            _members.Add(member);
            member.Groups.Add(this);
        }

        /// <summary>
        /// Removes a member from the group.
        /// </summary>
        /// <param name="member">Member to remove.</param>
        /// <exception cref="ArgumentNullException">If <paramref name="member"/> is <c>null</c>.</exception>
        /// <exception cref="InvalidOperationException">If the member is not found in the group.</exception>
        public void RemoveMember(Member member)
        {
            _ = member ?? throw new ArgumentNullException(nameof(member));

            if (!_members.Remove(member))
                throw new InvalidOperationException("Member not found in the group.");

            member.Groups.Remove(this);
        }
    }
}
