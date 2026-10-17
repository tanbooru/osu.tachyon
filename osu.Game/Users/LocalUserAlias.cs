// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;

namespace osu.Game.Users
{
    public static class LocalUserAlias
    {
        public const string ACCOUNT_USERNAME = @"GTMars";

        public const string DISPLAY_USERNAME = @"anny";

        private static readonly object display_user_lock = new object();

        private static APIUser? displayUser;

        public static bool AppliesTo(IUser? user) => string.Equals(user?.Username, ACCOUNT_USERNAME, StringComparison.OrdinalIgnoreCase);

        public static string GetDisplayUsername(IUser? user) => AppliesTo(user) ? DISPLAY_USERNAME : user?.Username ?? string.Empty;

        public static IUser GetProfileUser(IUser user) => AppliesTo(user) ? new APIUser { Id = 0, Username = DISPLAY_USERNAME } : user;

        public static int CachedDisplayUserId
        {
            get
            {
                lock (display_user_lock)
                    return displayUser?.Id ?? -1;
            }
        }

        public static IUser? GetDisplayUser(IUser? user, IAPIProvider? api)
        {
            if (!AppliesTo(user))
                return user;

            lock (display_user_lock)
            {
                if (displayUser == null && api?.State.Value == APIState.Online)
                {
                    var request = new GetUserRequest(DISPLAY_USERNAME);

                    try
                    {
                        api.Perform(request);
                        displayUser = request.Response;
                    }
                    catch
                    {
                    }
                }

                return displayUser;
            }
        }
    }
}
