// Copyright (c) ppy Pty Ltd <contact@ppy.sh>. Licensed under the MIT Licence.
// See the LICENCE file in the repository root for full licence text.

using System.IO;
using System.Linq;
using System.Threading;
using osu.Framework.Platform;
using osu.Game.Extensions;
using osu.Game.Overlays.Notifications;
using osu.Game.Scoring;
using osu.Game.Scoring.Legacy;
using osu.Game.Users;

namespace osu.Game.Database
{
    public class LegacyScoreExporter : LegacyExporter<ScoreInfo>
    {
        public LegacyScoreExporter(Storage storage)
            : base(storage)
        {
        }

        protected override string GetFilename(ScoreInfo score)
        {
            string scoreString = score.GetDisplayString();
            string filename = $"{scoreString} ({score.Date.LocalDateTime:yyyy-MM-dd_HH-mm})";

            return filename;
        }

        protected override string FileExtension => @".osr";

        public override void ExportToStream(ScoreInfo model, Stream outputStream, ProgressNotification? notification, CancellationToken cancellationToken = default)
        {
            var file = model.Files.SingleOrDefault();
            if (file == null)
                return;

            using (var inputStream = UserFileStorage.GetStream(file.File.GetStoragePath()))
            {
                if (LocalUserAlias.AppliesTo(model.User))
                    LegacyScoreUserRewriter.Rewrite(inputStream, outputStream, LocalUserAlias.DISPLAY_USERNAME, LocalUserAlias.CachedDisplayUserId);
                else
                    inputStream.CopyTo(outputStream);
            }
        }
    }
}
