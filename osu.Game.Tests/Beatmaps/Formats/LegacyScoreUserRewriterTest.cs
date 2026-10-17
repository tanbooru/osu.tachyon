// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.Collections.Generic;
using System.IO;
using NUnit.Framework;
using osu.Game.Online.API.Requests.Responses;
using osu.Game.Replays;
using osu.Game.Rulesets.Osu;
using osu.Game.Rulesets.Osu.Replays;
using osu.Game.Rulesets.Osu.UI;
using osu.Game.Rulesets.Replays;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Tests.Beatmaps;
using osu.Game.Tests.Resources;

namespace osu.Game.Tests.Beatmaps.Formats
{
    [TestFixture]
    public class LegacyScoreUserRewriterTest
    {
        [Test]
        public void TestRewriteUser()
        {
            var ruleset = new OsuRuleset().RulesetInfo;

            var scoreInfo = TestResources.CreateTestScoreInfo(ruleset);
            scoreInfo.User = new APIUser { Id = 1001, Username = "GTMars" };
            scoreInfo.OnlineID = 123123;
            scoreInfo.TotalScore = 500_000;

            var score = new Score
            {
                ScoreInfo = scoreInfo,
                Replay = new Replay
                {
                    Frames = new List<ReplayFrame>
                    {
                        new OsuReplayFrame(2000, OsuPlayfield.BASE_SIZE / 2, OsuAction.LeftButton),
                        new OsuReplayFrame(3000, OsuPlayfield.BASE_SIZE / 3),
                    }
                }
            };

            var encoded = new MemoryStream();
            new LegacyScoreEncoder(score, new TestBeatmap(ruleset)).Encode(encoded, true);
            encoded.Position = 0;

            var rewritten = new MemoryStream();
            LegacyScoreUserRewriter.Rewrite(encoded, rewritten, "anny", 2002);
            rewritten.Position = 0;

            var original = new LegacyScoreDecoderTest.TestLegacyScoreDecoder().Parse(new MemoryStream(encoded.ToArray()));
            var decoded = new LegacyScoreDecoderTest.TestLegacyScoreDecoder().Parse(rewritten);

            Assert.Multiple(() =>
            {
                Assert.That(decoded.ScoreInfo.User.Username, Is.EqualTo("anny"));
                Assert.That(original.ScoreInfo.RealmUser.OnlineID, Is.EqualTo(1001));
                Assert.That(decoded.ScoreInfo.RealmUser.OnlineID, Is.EqualTo(2002));
                Assert.That(decoded.ScoreInfo.OnlineID, Is.EqualTo(original.ScoreInfo.OnlineID));
                Assert.That(decoded.ScoreInfo.TotalScore, Is.EqualTo(original.ScoreInfo.TotalScore));
                Assert.That(decoded.ScoreInfo.MaxCombo, Is.EqualTo(original.ScoreInfo.MaxCombo));
                Assert.That(decoded.ScoreInfo.Date, Is.EqualTo(original.ScoreInfo.Date));
                Assert.That(decoded.ScoreInfo.Statistics, Is.EquivalentTo(original.ScoreInfo.Statistics));
                Assert.That(decoded.Replay.Frames, Has.Count.EqualTo(original.Replay.Frames.Count));

                for (int i = 0; i < original.Replay.Frames.Count; i++)
                {
                    var expected = (OsuReplayFrame)original.Replay.Frames[i];
                    var actual = (OsuReplayFrame)decoded.Replay.Frames[i];

                    Assert.That(actual.Time, Is.EqualTo(expected.Time));
                    Assert.That(actual.Position, Is.EqualTo(expected.Position));
                    Assert.That(actual.Actions, Is.EquivalentTo(expected.Actions));
                }
            });
        }

        [Test]
        public void TestInvalidDataIsCopiedUnchanged()
        {
            byte[] data = { 1, 2, 3, 4, 5 };

            var output = new MemoryStream();
            LegacyScoreUserRewriter.Rewrite(new MemoryStream(data), output, "anny", 2002);

            Assert.That(output.ToArray(), Is.EqualTo(data));
        }
    }
}
