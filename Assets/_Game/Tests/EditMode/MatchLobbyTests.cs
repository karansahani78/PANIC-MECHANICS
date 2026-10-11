using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Lobby;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class MatchLobbyTests
    {
        [Test]
        public void JoinRespectsCapacityAndDuplicates()
        {
            var lobby = new MatchLobby { MinPlayers = 2, MaxPlayers = 3 };

            Assert.That(lobby.TryJoin(1), Is.True);
            Assert.That(lobby.TryJoin(1), Is.False, "duplicate client rejected");
            Assert.That(lobby.TryJoin(2), Is.True);
            Assert.That(lobby.TryJoin(3), Is.True);
            Assert.That(lobby.TryJoin(4), Is.False, "over capacity rejected");
            Assert.That(lobby.Count, Is.EqualTo(3));
        }

        [Test]
        public void CanStartNeedsMinimumPlayers()
        {
            var lobby = new MatchLobby { MinPlayers = 2 };
            Assert.That(lobby.CanStart, Is.False);

            lobby.TryJoin(1);
            Assert.That(lobby.CanStart, Is.False);

            lobby.TryJoin(2);
            Assert.That(lobby.CanStart, Is.True);
        }

        [Test]
        public void LeaveRemovesPlayer()
        {
            var lobby = new MatchLobby { MinPlayers = 2 };
            lobby.TryJoin(1);
            lobby.TryJoin(2);

            Assert.That(lobby.TryLeave(1), Is.True);
            Assert.That(lobby.Contains(1), Is.False);
            Assert.That(lobby.Count, Is.EqualTo(1));
        }

        [Test]
        public void AssignRolesCoversAllPlayers()
        {
            var lobby = new MatchLobby { MinPlayers = 2, MaxPlayers = 4 };
            lobby.TryJoin(10);
            lobby.TryJoin(20);
            lobby.TryJoin(30);

            var roles = lobby.AssignRoles();
            Assert.That(roles.Count, Is.EqualTo(3));
            Assert.That(roles.ContainsKey(10), Is.True);
            Assert.That(roles.ContainsKey(30), Is.True);
        }
    }
}
