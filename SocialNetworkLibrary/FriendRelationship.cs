using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Represents a friendship relation between two members.
    /// </summary>
    public class FriendRelationship
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="FriendRelationship"/> class.
        /// </summary>
        /// <param name="member1">First member.</param>
        /// <param name="member2">Second member.</param>
        public FriendRelationship(Member member1, Member member2)
        {
            Member1 = member1 ?? throw new ArgumentNullException(nameof(member1));
            Member2 = member2 ?? throw new ArgumentNullException(nameof(member2));
        }

        /// <summary>Gets the first participant of the relation.</summary>
        public Member Member1 { get; }

        /// <summary>Gets the second participant of the relation.</summary>
        public Member Member2 { get; }

        /// <summary>
        /// Gets the opposite participant in the relation.
        /// </summary>
        /// <param name="member">Known participant.</param>
        /// <returns>The other participant.</returns>
        internal Member GetOther(Member member) =>
            member.Id == Member1.Id ? Member2 : Member1;

        /// <summary>
        /// Checks whether a member participates in this relation.
        /// </summary>
        /// <param name="member">Member to test.</param>
        /// <returns><c>true</c> if the member participates; otherwise, <c>false</c>.</returns>
        internal bool Involves(Member member) =>
            member.Id == Member1.Id || member.Id == Member2.Id;
    }
}
