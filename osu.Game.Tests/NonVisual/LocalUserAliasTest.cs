// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using NUnit.Framework;
using osu.Game.Online.API;
using osu.Game.Online.API.Requests;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Users;

namespace osu.Game.Tests.NonVisual
{
    [TestFixture]
    public class LocalUserAliasTest
    {
        private readonly APIUser account = new APIUser { Id = 1001, Username = "GTMars" };
        private readonly APIUser other = new APIUser { Id = 1002, Username = "someone" };

        [Test]
        public void TestDisplayUsername()
        {
            Assert.That(LocalUserAlias.GetDisplayUsername(account), Is.EqualTo("anny"));
            Assert.That(LocalUserAlias.GetDisplayUsername(new APIUser { Username = "gtmars" }), Is.EqualTo("anny"));
            Assert.That(LocalUserAlias.GetDisplayUsername(other), Is.EqualTo("someone"));
            Assert.That(LocalUserAlias.GetDisplayUsername(null), Is.Empty);
        }

        [Test]
        public void TestProfileUser()
        {
            var profileUser = LocalUserAlias.GetProfileUser(account);

            Assert.That(profileUser.Username, Is.EqualTo("anny"));
            Assert.That(profileUser.OnlineID, Is.LessThanOrEqualTo(1));
            Assert.That(LocalUserAlias.GetProfileUser(other), Is.SameAs(other));
        }

        [Test]
        public void TestAvatarUser()
        {
            var anny = new APIUser { Id = 2002, Username = "anny", CoverUrl = "https://assets.ppy.sh/user-cover-presets/1/anny.jpg" };
            var api = new DummyAPIAccess();

            api.HandleRequest = request =>
            {
                if (request is GetUserRequest userRequest && userRequest.Lookup == "anny")
                {
                    userRequest.TriggerSuccess(anny);
                    return true;
                }

                return false;
            };

            Assert.That(LocalUserAlias.GetDisplayUser(other, api), Is.SameAs(other));
            Assert.That(LocalUserAlias.GetDisplayUser(account, api), Is.SameAs(anny));
            Assert.That(((IHasCover)LocalUserAlias.GetDisplayUser(account, api)!).CoverUrl, Is.EqualTo(anny.CoverUrl));
        }
    }
}
