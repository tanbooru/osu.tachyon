// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users
{
    /// <summary>
    /// A purely cosmetic, client-side display alias for a single account.
    /// </summary>
    /// <remarks>
    /// This only changes what is drawn locally. The actual account, anything sent to the server,
    /// and anything stored in scores or replays is left untouched, as is all gameplay.
    /// </remarks>
    public static class LocalUserAlias
    {
        /// <summary>
        /// The username of the account which should be displayed under an alias.
        /// </summary>
        public const string ACCOUNT_USERNAME = @"GTMars";

        /// <summary>
        /// The username to display instead, and whose profile is shown when opening the account's profile.
        /// </summary>
        public const string DISPLAY_USERNAME = @"anny";

        public static bool AppliesTo(IUser? user) => string.Equals(user?.Username, ACCOUNT_USERNAME, StringComparison.OrdinalIgnoreCase);

        public static string GetDisplayUsername(IUser user) => AppliesTo(user) ? DISPLAY_USERNAME : user.Username;

        /// <summary>
        /// Returns the user whose profile should be shown in place of <paramref name="user"/>.
        /// </summary>
        public static IUser GetProfileUser(IUser user) => AppliesTo(user) ? new APIUser { Id = 0, Username = DISPLAY_USERNAME } : user;
    }
}
