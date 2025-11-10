using System.Collections.Generic;

namespace RagApi.Auth
{
    /// <summary>
    /// Configuration options for Azure AD authentication
    /// </summary>
    public class AzureAdOptions
    {
        /// <summary>
        /// Azure AD instance URL
        /// </summary>
        public string Instance { get; set; }

        /// <summary>
        /// Azure AD tenant ID
        /// </summary>
        public string TenantId { get; set; }

        /// <summary>
        /// Application (client) ID
        /// </summary>
        public string ClientId { get; set; }

        /// <summary>
        /// Application client secret (for server-to-server operations)
        /// </summary>
        public string ClientSecret { get; set; }

        /// <summary>
        /// Scope for API access
        /// </summary>
        public string Scope { get; set; }

        /// <summary>
        /// Token expiration time in minutes
        /// </summary>
        public int TokenExpirationInMinutes { get; set; } = 120;

        /// <summary>
        /// Whether to check app permissions
        /// </summary>
        public bool CheckAppPermissions { get; set; } = false;

        /// <summary>
        /// Azure AD groups configuration
        /// </summary>
        public GroupsConfig Groups { get; set; }

        /// <summary>
        /// Azure AD groups configuration
        /// </summary>
        public class GroupsConfig
        {
            /// <summary>
            /// Admin group ID
            /// </summary>
            public string Admin { get; set; }

            /// <summary>
            /// Regular user group ID
            /// </summary>
            public string User { get; set; }
        }
    }
}
