using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace SocialNetworkLibrary.Tests
{
    #region Member tests

    [TestClass]
    public class MemberTests
    {
        /// <summary>Checks that two different members can be added successfully.</summary>
        [TestMethod]
        public void AddMember_AllowsUniqueId()
        {
            var net = new SocialNetwork();
            net.AddMember(new Member(1, "Alice", "alice@mail.com"));
            net.AddMember(new Member(2, "Bob", "bob@mail.com"));

            Assert.AreEqual(2, net.Members.Count);
        }

        /// <summary>Adding a member with a duplicate Id must throw an exception.</summary>
        [TestMethod]
        public void AddMember_DuplicateId_Throws()
        {
            var net = new SocialNetwork();
            net.AddMember(new Member(1, "Alice", "alice@mail.com"));

            Assert.ThrowsException<InvalidOperationException>(
                () => net.AddMember(new Member(1, "Another", "x@y.z")));
        }

        /// <summary>Sends a friend request and leaves it in the Pending state.</summary>
        [TestMethod]
        public void SendFriendRequest_CreatesPendingRequest()
        {
            var alice = new Member(1, "Alice", "a@a.com");
            var bob = new Member(2, "Bob", "b@b.com");

            alice.SendFriendRequest(bob);

            Assert.AreEqual(1, alice.SentRequests.Count);
            Assert.AreEqual(RequestStatus.Pending, alice.SentRequests.First().Status);
            Assert.AreEqual(1, bob.ReceivedRequests.Count);
        }

        /// <summary>Sends the same request twice and expects an exception.</summary>
        [TestMethod]
        public void SendFriendRequest_DuplicatePending_Throws()
        {
            var alice = new Member(1, "Alice", "a@a.com");
            var bob = new Member(2, "Bob", "b@b.com");
            alice.SendFriendRequest(bob);

            Assert.ThrowsException<InvalidOperationException>(() =>
                alice.SendFriendRequest(bob));
        }

        /// <summary>Accepting a request creates a friendship on both sides.</summary>
        [TestMethod]
        public void AcceptFriendRequest_AddsFriendship()
        {
            var alice = new Member(1, "Alice", "a@a.com");
            var bob = new Member(2, "Bob", "b@b.com");
            alice.SendFriendRequest(bob);

            var request = bob.ReceivedRequests.Single();
            bob.AcceptFriendRequest(request);

            Assert.AreEqual(RequestStatus.Accepted, request.Status);
            Assert.AreEqual(1, alice.GetFriends().Count);
            Assert.AreEqual("Bob", alice.GetFriends().Single().Name);
            Assert.AreEqual(1, bob.GetFriends().Count);
        }

        /// <summary>Declining a request leaves both users without friendship.</summary>
        [TestMethod]
        public void DeclineFriendRequest_LeavesNoFriendship()
        {
            var alice = new Member(1, "Alice", "a@a.com");
            var bob = new Member(2, "Bob", "b@b.com");
            alice.SendFriendRequest(bob);

            var request = bob.ReceivedRequests.Single();
            bob.DeclineFriendRequest(request);

            Assert.AreEqual(RequestStatus.Declined, request.Status);
            Assert.AreEqual(0, alice.GetFriends().Count);
            Assert.AreEqual(0, bob.GetFriends().Count);
        }

        /// <summary>Removing a friend wipes the relation for both users.</summary>
        [TestMethod]
        public void RemoveFriend_RemovesFromBothSides()
        {
            var alice = new Member(1, "Alice", "a@a.com");
            var bob = new Member(2, "Bob", "b@b.com");

            alice.AddFriend(bob);                     // one call is enough
            Assert.AreEqual(1, alice.GetFriends().Count);

            alice.RemoveFriend(bob);

            Assert.AreEqual(0, alice.GetFriends().Count);
            Assert.AreEqual(0, bob.GetFriends().Count);
        }
    }

    #endregion

    #region Group & network tests

    [TestClass]
    public class GroupAndNetworkTests
    {
        /// <summary>Creating a group adds it to the network and joins the owner.</summary>
        [TestMethod]
        public void CreateGroup_AddsGroupAndOwner()
        {
            var net = new SocialNetwork();
            var alice = new Member(1, "Alice", "alice@mail.com");
            net.AddMember(alice);

            var group = net.CreateGroup("Chess Club", alice);

            Assert.AreEqual(1, net.Groups.Count);
            Assert.AreSame(alice, group.Owner);
            Assert.IsTrue(group.Members.Contains(alice));
            Assert.IsTrue(alice.Groups.Contains(group));
        }

        /// <summary>Attempting to create a group with a taken name must fail.</summary>
        [TestMethod]
        public void CreateGroup_DuplicateName_Throws()
        {
            var net = new SocialNetwork();
            var alice = new Member(1, "Alice", "a@a.com");
            net.AddMember(alice);
            net.CreateGroup("Chess", alice);

            Assert.ThrowsException<InvalidOperationException>(() =>
                net.CreateGroup("Chess", alice));
        }

        /// <summary>Adding a member via Group.AddMember updates both collections.</summary>
        [TestMethod]
        public void Group_AddMember_AddsBothSides()
        {
            var net = new SocialNetwork();
            var owner = new Member(1, "Owner", "o@o.com");
            var bob = new Member(2, "Bob", "b@b.com");
            net.AddMember(owner);
            net.AddMember(bob);

            var club = net.CreateGroup("Club", owner);
            club.AddMember(bob);

            Assert.IsTrue(club.Members.Contains(bob));
            Assert.IsTrue(bob.Groups.Contains(club));
        }

        /// <summary>Adding the same member twice throws an exception.</summary>
        [TestMethod]
        public void Group_AddMember_Duplicate_Throws()
        {
            var owner = new Member(1, "Owner", "o@o.com");
            var bob = new Member(2, "Bob", "b@b.com");
            var club = new Group(1, "Club", owner);

            club.AddMember(bob);

            Assert.ThrowsException<InvalidOperationException>(() =>
                club.AddMember(bob));
        }

        /// <summary>Removing a member clears references on both sides.</summary>
        [TestMethod]
        public void Group_RemoveMember_RemovesBothSides()
        {
            var owner = new Member(1, "Owner", "o@o.com");
            var bob = new Member(2, "Bob", "b@b.com");
            var club = new Group(1, "Club", owner);
            club.AddMember(bob);

            club.RemoveMember(bob);

            Assert.IsFalse(club.Members.Contains(bob));
            Assert.IsFalse(bob.Groups.Contains(club));
        }

        /// <summary>Removing a non-member throws an exception.</summary>
        [TestMethod]
        public void Group_RemoveMember_NotFound_Throws()
        {
            var owner = new Member(1, "Owner", "o@o.com");
            var club = new Group(1, "Club", owner);
            var stranger = new Member(2, "Stranger", "s@s.com");

            Assert.ThrowsException<InvalidOperationException>(() =>
                club.RemoveMember(stranger));
        }

        /// <summary>Removing a user from the network cleans friendships and groups.</summary>
        [TestMethod]
        public void RemoveMember_CleansFriendshipsAndGroups()
        {
            var net = new SocialNetwork();
            var alice = new Member(1, "Alice", "a@a.com");
            var bob = new Member(2, "Bob", "b@b.com");
            net.AddMember(alice);
            net.AddMember(bob);

            alice.AddFriend(bob);
            var club = net.CreateGroup("Club", alice);
            club.AddMember(bob);

            net.RemoveMember(bob.Id);

            Assert.IsFalse(net.Members.Contains(bob));
            Assert.AreEqual(0, alice.GetFriends().Count);
            Assert.IsFalse(club.Members.Contains(bob));
        }

        /// <summary>An unknown Id passed to RemoveMember must raise an exception.</summary>
        [TestMethod]
        public void RemoveMember_NotFound_Throws()
        {
            var net = new SocialNetwork();

            Assert.ThrowsException<InvalidOperationException>(() =>
                net.RemoveMember(42));
        }
    }

    #endregion
}
