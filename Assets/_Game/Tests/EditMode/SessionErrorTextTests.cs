using NUnit.Framework;
using PanicMechanics.Core;

namespace PanicMechanics.Tests.EditMode
{
    public sealed class SessionErrorTextTests
    {
        [Test]
        public void SessionNotFoundMapsToInvalidCode()
        {
            Assert.That(SessionErrorText.CodeFor("SessionNotFound"), Is.EqualTo(SessionErrorText.InvalidCode));
        }

        [Test]
        public void InvalidSessionIdentifierMapsToInvalidCode()
        {
            Assert.That(SessionErrorText.CodeFor("InvalidSessionIdentifier"), Is.EqualTo(SessionErrorText.InvalidCode));
        }

        [Test]
        public void ConflictErrorsMapToAlreadyHosting()
        {
            Assert.That(SessionErrorText.CodeFor("SessionConflict"), Is.EqualTo(SessionErrorText.AlreadyHosting));
            Assert.That(SessionErrorText.CodeFor("LobbyAlreadyExists"), Is.EqualTo(SessionErrorText.AlreadyHosting));
            Assert.That(SessionErrorText.CodeFor("AllocationAlreadyExists"), Is.EqualTo(SessionErrorText.AlreadyHosting));
        }

        [Test]
        public void AuthorizationErrorsMapToNotAuthorized()
        {
            Assert.That(SessionErrorText.CodeFor("Forbidden"), Is.EqualTo(SessionErrorText.NotAuthorized));
            Assert.That(SessionErrorText.CodeFor("NotAuthorized"), Is.EqualTo(SessionErrorText.NotAuthorized));
        }

        [Test]
        public void NetworkErrorsMapToNetworkSetup()
        {
            Assert.That(SessionErrorText.CodeFor("NetworkSetupFailed"), Is.EqualTo(SessionErrorText.NetworkSetup));
            Assert.That(SessionErrorText.CodeFor("TransportComponentMissing"), Is.EqualTo(SessionErrorText.NetworkSetup));
            Assert.That(SessionErrorText.CodeFor("TranportComponentMissing"), Is.EqualTo(SessionErrorText.NetworkSetup));
        }

        [Test]
        public void UnknownAndNullMapToUnknown()
        {
            Assert.That(SessionErrorText.CodeFor("SomethingNew"), Is.EqualTo(SessionErrorText.Unknown));
            Assert.That(SessionErrorText.CodeFor(null), Is.EqualTo(SessionErrorText.Unknown));
        }

        [Test]
        public void EveryCodeHasNonEmptyMessage()
        {
            string[] samples =
            {
                "SessionNotFound", "SessionConflict", "Forbidden", "SessionDeleted",
                "RateLimitExceeded", "NetworkSetupFailed", "MissingAssembly", "Mystery"
            };

            foreach (var sample in samples)
            {
                Assert.That(SessionErrorText.MessageFor(sample), Is.Not.Empty, sample);
            }
        }

        [Test]
        public void EmptyCodeDetectionHandlesNullBlankAndWhitespace()
        {
            Assert.That(SessionErrorText.IsEmptyCode(null), Is.True);
            Assert.That(SessionErrorText.IsEmptyCode(string.Empty), Is.True);
            Assert.That(SessionErrorText.IsEmptyCode("   "), Is.True);
            Assert.That(SessionErrorText.IsEmptyCode("abcd"), Is.False);
        }

        [Test]
        public void NormalizeCodeTrimsAndUppercases()
        {
            Assert.That(SessionErrorText.NormalizeCode("  ab12 "), Is.EqualTo("AB12"));
            Assert.That(SessionErrorText.NormalizeCode(null), Is.EqualTo(string.Empty));
        }
    }
}
