using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SocialNetworkLibrary
{
    /// <summary>
    /// Provides basic operations for managing a friend list.
    /// </summary>
    public interface IFriendManager
    {
        /// <summary>Adds a member to the friend list.</summary>
        /// <param name="friend">Member to add.</param>
        void AddFriend(Member friend);

        /// <summary>Removes a member from the friend list.</summary>
        /// <param name="friend">Member to remove.</param>
        void RemoveFriend(Member friend);

        /// <summary>Gets an up-to-date list of friends.</summary>
        /// <returns>Collection of friends.</returns>
        List<Member> GetFriends();
    }
}
