using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using TagLib.Id3v2;

namespace mik2artist {

    class Program {
        static void Main(string[] args) {
            var dir = args.FirstOrDefault();
            if(String.IsNullOrEmpty(dir) || !Directory.Exists(dir))
                throw new ArgumentException();

            foreach(var path in Directory.GetFiles(dir, "*.mp3", SearchOption.TopDirectoryOnly)) {
                File.Copy(path, path + ".bak", true);

                using(var file = TagLib.File.Create(path)) {
                    var id3v2 = file.GetTag(TagLib.TagTypes.Id3v2) as Tag;

                    // https://resource.dopus.com/t/forward-slashes-in-artist-name-tag-show-as-semi-colon/13672/2
                    var artist = String.Join("/", id3v2.Performers);
                    artist = StripArtist(artist);
                    Console.WriteLine(artist);

                    var key = id3v2.GetFrames<TextInformationFrame>("TKEY").Single().Text.Single();

                    var energy = id3v2.GetFrames<UserTextInformationFrame>()
                        .Where(i => i.Description == "EnergyLevel")
                        .Single()
                        .Text
                        .Single();

                    var openKey = ToOpenKey(key);
                    if(openKey.Length < 3)
                        openKey = "0" + openKey;

                    id3v2.Performers = new[] { openKey + " - Energy " + energy + " - " + artist };
                    id3v2.AlbumArtists = new string[0];

                    file.RemoveTags(TagLib.TagTypes.Id3v1);

                    file.Save();
                }
            }
        }

        static string StripArtist(string text) {
            const string keyRe = @"(\d\d?[dmAB]|[A-G][#b]?m?)";
            return Regex.Replace(text, $@"^{keyRe}(/{keyRe})? - (Energy \d - )", "");
        }

        static string ToOpenKey(string key) {
            if(Regex.IsMatch(key, @"^\d\d?[dm]$"))
                return key;

            switch(key) {

                case "A": return "4d";
                case "A#": return "11d";
                case "B": return "6d";
                case "C": return "1d";
                case "C#": return "8d";
                case "D": return "3d";
                case "D#": return "10d";
                case "E": return "5d";
                case "F": return "12d";
                case "F#": return "7d";
                case "G": return "2d";
                case "G#": return "9d";

                case "Am": return "1m";
                case "A#m": return "8m";
                case "Bm": return "3m";
                case "Cm": return "10m";
                case "C#m": return "5m";
                case "Dm": return "12m";
                case "D#m": return "7m";
                case "Em": return "2m";
                case "Fm": return "9m";
                case "F#m": return "4m";
                case "Gm": return "11m";
                case "G#m": return "6m";

                default:
                    throw new NotSupportedException();
            }

        }
    }
}
