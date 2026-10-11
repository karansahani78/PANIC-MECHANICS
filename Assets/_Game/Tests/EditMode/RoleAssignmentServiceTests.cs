using System;
using System.Collections.Generic;
using NUnit.Framework;
using PanicMechanics.Core;
using PanicMechanics.Gameplay.Lobby;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class RoleAssignmentServiceTests
    {
        [Test]
        public void TwoPlayersGetDistinctRoles()
        {
            var map = RoleAssignmentService.Assign(new List<ulong> { 1, 2 });

            Assert.That(map.Count, Is.EqualTo(2));
            Assert.That(map[1], Is.Not.EqualTo(map[2]));
            Assert.That(map[1], Is.EqualTo(PlayerRole.Engineer));
            Assert.That(map[2], Is.EqualTo(PlayerRole.Electrician));
        }

        [Test]
        public void FourPlayersCoverAllRoles()
        {
            var map = RoleAssignmentService.Assign(new List<ulong> { 10, 20, 30, 40 });

            var assigned = new HashSet<PlayerRole>(map.Values);
            Assert.That(assigned.Count, Is.EqualTo(4));
        }

        [Test]
        public void MorePlayersThanRolesThrows()
        {
            var ids = new List<ulong> { 1, 2, 3, 4, 5 };
            Assert.Throws<ArgumentException>(() => RoleAssignmentService.Assign(ids));
        }

        [Test]
        public void CustomRoleOrderIsRespected()
        {
            var map = RoleAssignmentService.Assign(new List<ulong> { 7 }, new[] { PlayerRole.Dispatcher });
            Assert.That(map[7], Is.EqualTo(PlayerRole.Dispatcher));
        }
    }
}
