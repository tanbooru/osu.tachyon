// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System;
using System.Collections.Generic;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Rulesets;

namespace osu.Game.Users
{
    public static class LocalUserAlias
    {
        public const string ACCOUNT_USERNAME = @"GTMars";

        public const string DISPLAY_USERNAME = @"anny";

        private static readonly object display_user_lock = new object();

        private static APIUser? displayUser;

        private static readonly Dictionary<string, UserStatistics> display_statistics = new Dictionary<string, UserStatistics>();

        private static readonly HashSet<string> pending_statistics = new HashSet<string>();

        public static event Action? DisplayStatisticsLoaded;

        public static UserStatistics? GetDisplayStatistics(IRulesetInfo ruleset, IAPIProvider api)
        {
            lock (display_user_lock)
            {
                if (display_statistics.TryGetValue(ruleset.ShortName, out var cached))
                    return cached;

                if (api.State.Value != APIState.Online || !pending_statistics.Add(ruleset.ShortName))
                    return null;
            }

            var request = new GetUserRequest(DISPLAY_USERNAME, ruleset);

            request.Success += user =>
            {
                lock (display_user_lock)
                {
                    pending_statistics.Remove(ruleset.ShortName);

                    if (user.Statistics != null)
                        display_statistics[ruleset.ShortName] = user.Statistics;
                }

                DisplayStatisticsLoaded?.Invoke();
            };

            request.Failure += _ =>
            {
                lock (display_user_lock)
                    pending_statistics.Remove(ruleset.ShortName);
            };

            api.Queue(request);
            return null;
        }

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
