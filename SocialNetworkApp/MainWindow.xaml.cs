using SocialNetworkLibrary;
using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Group = SocialNetworkLibrary.Group;

namespace SocialNetworkApp
{
    public partial class MainWindow : Window
    {
        private readonly SocialNetwork _network;
        private int _nextMemberId = 1;

        public MainWindow()
        {
            InitializeComponent();
            _network = new SocialNetwork();
        }

        /// <summary>Adds a new member.</summary>
        private void AddMember_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            string name = MemberNameTextBox.Text.Trim();
            string email = MemberEmailTextBox.Text.Trim();

            try
            {
                if (!Regex.IsMatch(name, @"^[a-zA-Z0-9_]+$"))
                    throw new ArgumentException("Username may contain only letters, digits and '_'.");

                if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                    throw new ArgumentException("Invalid e-mail format.");

                if (_network.Members.Any(m => m.Name.Equals(name, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("Username already exists.");

                if (_network.Members.Any(m => m.Email.Equals(email, StringComparison.OrdinalIgnoreCase)))
                    throw new ArgumentException("E-mail already exists.");

                var member = new Member(_nextMemberId++, name, email);
                _network.AddMember(member);

                UpdateMembersUI();
                ShowOk($"User '{name}' added.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Creates a new group.</summary>
        private void CreateGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            string name = GroupNameTextBox.Text.Trim();
            if (GroupOwnerComboBox.SelectedItem is not Member owner)
            {
                ShowError("Select a group owner.");
                return;
            }

            try
            {
                if (string.IsNullOrWhiteSpace(name))
                    throw new ArgumentException("Group name cannot be empty.");

                var group = _network.CreateGroup(name, owner);
                UpdateGroupsUI();
                GroupsListBox.SelectedItem = group;
                ShowOk($"Group '{name}' created.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Sends a friend request.</summary>
        private void SendRequest_Click(object _, RoutedEventArgs e)
        {
            ClearStatus();

            if (SenderComboBox.SelectedItem is not Member sender ||
                ReceiverComboBox.SelectedItem is not Member receiver)
            {
                ShowError("Select sender and receiver.");
                return;
            }

            if (sender.Id == receiver.Id)
            {
                ShowError("Cannot send request to yourself.");
                return;
            }

            try
            {
                sender.SendFriendRequest(receiver);
                RefreshIncomingRequests(receiver);
                ShowOk($"Request sent from '{sender.Name}' to '{receiver.Name}'.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Accepts the selected friend request.</summary>
        private void AcceptRequest_Click(object sender, RoutedEventArgs e) =>
            HandleRequest(RequestStatus.Accepted);

        /// <summary>Declines the selected friend request.</summary>
        private void DeclineRequest_Click(object sender, RoutedEventArgs e) =>
            HandleRequest(RequestStatus.Declined);

        /// <summary>Removes a friend.</summary>
        private void RemoveFriend_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            if (MembersListBox.SelectedItem is not Member owner ||
                FriendsListBox.SelectedItem is not Member friend)
            {
                ShowError("Select a user and a friend.");
                return;
            }

            try
            {
                owner.RemoveFriend(friend);
                FriendsListBox.ItemsSource = owner.GetFriends();
                RemoveFriendButton.IsEnabled = false;
                ShowOk($"'{friend.Name}' removed from friends.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Adds the selected member to the selected group.</summary>
        private void AddToGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            if (GroupsListBox.SelectedItem is not Group group ||
                AddToGroupComboBox.SelectedItem is not Member target ||
                MembersListBox.SelectedItem is not Member current)
            {
                ShowError("Select an active user, a group, and a member.");
                return;
            }

            if (current.Id != group.Owner.Id)
            {
                ShowError("Only the group owner can add members.");
                return;
            }

            try
            {
                group.AddMember(target);
                RefreshGroupMembers(group);
                ShowOk($"'{target.Name}' was added to '{group.Name}'.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Removes the highlighted member from the selected group.</summary>
        /// <summary>Removes a member from the selected group.</summary>
        private void RemoveFromGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            if (GroupsListBox.SelectedItem is not Group group ||
                GroupMembersListBox.SelectedItem is not Member target ||
                MembersListBox.SelectedItem is not Member current)
            {
                ShowError("Select an active user, a group, and its member.");
                return;
            }

            bool ownerRemoving = current.Id == group.Owner.Id && target.Id != current.Id;
            bool selfRemoving = current.Id == target.Id;

            if (!ownerRemoving && !selfRemoving)
            {
                ShowError("Only the group owner can remove another member, or a member can leave the group themselves.");
                return;
            }

            try
            {
                group.RemoveMember(target);
                RefreshGroupMembers(group);
                RemoveFromGroupButton.IsEnabled = false;
                ShowOk($"'{target.Name}' was removed from '{group.Name}'.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Deletes the selected group (owner only).</summary>
        private void DeleteGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            if (GroupsListBox.SelectedItem is not Group group)
            {
                ShowError("Select a group.");
                return;
            }

            if (MembersListBox.SelectedItem is not Member current ||
                current.Id != group.Owner.Id)
            {
                ShowError("Only the owner can delete the group.");
                return;
            }

            _network.Groups.Remove(group);
            foreach (var m in group.Members.ToList())
                group.RemoveMember(m);

            UpdateGroupsUI();
            GroupMembersListBox.ItemsSource = null;
            DeleteGroupButton.IsEnabled = false;
            ShowOk($"Group '{group.Name}' deleted.");
        }

        /// <summary>Shows friends of the selected member.</summary>
        private void ShowFriends_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            if (MembersListBox.SelectedItem is not Member member)
            {
                ShowError("Select a user.");
                return;
            }

            FriendsListBox.ItemsSource = member.GetFriends();
            RemoveFriendButton.IsEnabled = false;
            ShowOk($"Friends of '{member.Name}'.");
        }

        /// <summary>Handles member selection.</summary>
        private void MembersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearStatus();

            Member member = null!;

            if (MembersListBox.SelectedItem is Member)
            {
                member = (Member)MembersListBox.SelectedItem;
                RefreshIncomingRequests(member);
            }

            if (GroupsListBox.SelectedItem is Group g)
                DeleteGroupButton.IsEnabled = member?.Id == g.Owner.Id;
        }

        /// <summary>Enables friend remove button.</summary>
        private void FriendsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
            RemoveFriendButton.IsEnabled = FriendsListBox.SelectedItem is Member;

        /// <summary>Handles group selection.</summary>
        private void GroupsListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ClearStatus();

            if (GroupsListBox.SelectedItem is Group group)
            {
                RefreshGroupMembers(group);
                DeleteGroupButton.IsEnabled = MembersListBox.SelectedItem is Member member &&
                                              member.Id == group.Owner.Id;
            }
            else
            {
                GroupMembersListBox.ItemsSource = null;
                DeleteGroupButton.IsEnabled = false;
            }
        }

        /// <summary>Enables remove-from-group button.</summary>
        private void GroupMembersListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            RemoveFromGroupButton.IsEnabled = GroupMembersListBox.SelectedItem is Member;

            LeaveGroupButton.IsEnabled =
                GroupsListBox.SelectedItem is Group g &&
                MembersListBox.SelectedItem is Member current &&
                g.Members.Any(m => m.Id == current.Id) &&
                g.Owner.Id != current.Id;
        }

        /// <summary>Updates all controls that show members.</summary>
        private void UpdateMembersUI()
        {
            MembersListBox.ItemsSource = null;
            MembersListBox.ItemsSource = _network.Members;

            SenderComboBox.ItemsSource = null;
            SenderComboBox.ItemsSource = _network.Members;

            ReceiverComboBox.ItemsSource = null;
            ReceiverComboBox.ItemsSource = _network.Members;

            GroupOwnerComboBox.ItemsSource = null;
            GroupOwnerComboBox.ItemsSource = _network.Members;

            AddToGroupComboBox.ItemsSource = null;
            AddToGroupComboBox.ItemsSource = _network.Members;
        }

        /// <summary>Updates the groups list.</summary>
        private void UpdateGroupsUI()
        {
            GroupsListBox.ItemsSource = null;
            GroupsListBox.ItemsSource = _network.Groups;
        }

        /// <summary>Refreshes pending requests for a member.</summary>
        private void RefreshIncomingRequests(Member member)
        {
            IncomingRequestsListBox.ItemsSource = null;
            IncomingRequestsListBox.ItemsSource =
                member.ReceivedRequests.Where(r => r.Status == RequestStatus.Pending).ToList();
        }

        /// <summary>Refreshes members shown for a group.</summary>
        private void RefreshGroupMembers(Group group)
        {
            GroupMembersListBox.ItemsSource = null;
            GroupMembersListBox.ItemsSource = group.Members;
        }

        /// <summary>Processes accept or decline of a request.</summary>
        private void HandleRequest(RequestStatus result)
        {
            ClearStatus();

            if (MembersListBox.SelectedItem is not Member receiver ||
                IncomingRequestsListBox.SelectedItem is not FriendRequest request)
            {
                ShowError("Select user and request.");
                return;
            }

            try
            {
                if (result == RequestStatus.Accepted)
                    receiver.AcceptFriendRequest(request);
                else
                    receiver.DeclineFriendRequest(request);

                RefreshIncomingRequests(receiver);
                ShowOk($"Request #{request.Id} {result.ToString().ToLower()}.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Leaves the selected group (non-owner).</summary>
        private void LeaveGroup_Click(object sender, RoutedEventArgs e)
        {
            ClearStatus();

            if (GroupsListBox.SelectedItem is not Group group ||
                MembersListBox.SelectedItem is not Member member)
            {
                ShowError("Select active user and a group.");
                return;
            }

            if (group.Owner.Id == member.Id)
            {
                ShowError("Owner cannot leave the group. Use Delete Group.");
                return;
            }

            try
            {
                member.LeaveGroup(group);
                RefreshGroupMembers(group);
                LeaveGroupButton.IsEnabled = false;
                ShowOk($"'{member.Name}' left '{group.Name}'.");
            }
            catch (Exception ex)
            {
                ShowError(ex.Message);
            }
        }

        /// <summary>Clears the status line.</summary>
        private void ClearStatus() => StatusTextBlock.Text = string.Empty;

        /// <summary>Displays an error in red.</summary>
        private void ShowError(string message)
        {
            StatusTextBlock.Foreground = Brushes.Red;
            StatusTextBlock.Text = message;
        }

        /// <summary>Displays a normal message in green.</summary>
        private void ShowOk(string message)
        {
            StatusTextBlock.Foreground = Brushes.Green;
            StatusTextBlock.Text = message;
        }
    }
}
