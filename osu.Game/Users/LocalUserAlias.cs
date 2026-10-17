// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users
{
    public static class LocalUserAlias
    {
        public const string ACCOUNT_USERNAME = @"GTMars";

        public const string DISPLAY_USERNAME = @"anny";

        public static bool AppliesTo(IUser? user) => string.Equals(user?.Username, ACCOUNT_USERNAME, StringComparison.OrdinalIgnoreCase);

        public static string GetDisplayUsername(IUser user) => AppliesTo(user) ? DISPLAY_USERNAME : user.Username;

        public static IUser GetProfileUser(IUser user) => AppliesTo(user) ? new APIUser { Id = 0, Username = DISPLAY_USERNAME } : user;
    }
}
