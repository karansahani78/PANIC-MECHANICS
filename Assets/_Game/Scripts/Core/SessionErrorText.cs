namespace PanicMechanics.Core
{
    public static class SessionErrorText
    {
        public const string InvalidCode = "invalid_code";
        public const string AlreadyHosting = "already_hosting";
        public const string NotAuthorized = "not_authorized";
        public const string SessionEnded = "session_ended";
        public const string RateLimited = "rate_limited";
        public const string NetworkSetup = "network_setup";
        public const string Unavailable = "unavailable";
        public const string Unknown = "unknown";

        public static string CodeFor(string sessionError)
        {
            switch (sessionError)
            {
                case "SessionNotFound":
                case "InvalidSessionIdentifier":
                    return InvalidCode;
                case "SessionConflict":
                case "LobbyAlreadyExists":
                case "SessionTypeAlreadyExists":
                case "AllocationAlreadyExists":
                    return AlreadyHosting;
                case "Forbidden":
                case "NotAuthorized":
                    return NotAuthorized;
                case "SessionDeleted":
                    return SessionEnded;
                case "RateLimitExceeded":
                    return RateLimited;
                case "NetworkManagerNotInitialized":
                case "NetworkManagerStartFailed":
                case "NetworkSetupFailed":
                case "TransportComponentMissing":
                case "TranportComponentMissing":
                case "TransportInvalid":
                case "InvalidNetworkConfig":
                    return NetworkSetup;
                case "MissingAssembly":
                case "InvalidOperation":
                    return Unavailable;
                default:
                    return Unknown;
            }
        }

        public static string MessageFor(string sessionError)
        {
            switch (CodeFor(sessionError))
            {
                case InvalidCode:
                    return "That join code is invalid or the session no longer exists.";
                case AlreadyHosting:
                    return "A session already exists. Leave it before hosting again.";
                case NotAuthorized:
                    return "You are not authorized to join that session.";
                case SessionEnded:
                    return "The host ended the session.";
                case RateLimited:
                    return "Too many requests. Please wait a moment and retry.";
                case NetworkSetup:
                    return "Network setup failed. Check your connection and try again.";
                case Unavailable:
                    return "Session services are unavailable.";
                default:
                    return "Session operation failed.";
            }
        }

        public static bool IsEmptyCode(string joinCode)
        {
            return string.IsNullOrEmpty(joinCode) || joinCode.Trim().Length == 0;
        }

        public static string NormalizeCode(string joinCode)
        {
            return joinCode == null ? string.Empty : joinCode.Trim().ToUpperInvariant();
        }
    }
}
