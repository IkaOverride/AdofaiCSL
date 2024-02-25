using ADOFAI;
using GDMiniJSON;
using System.Collections.Generic;
using System.IO;

namespace AdofaiCSL.API.Extensions {

    public static class LevelDataCLSExtensions {

        /// <summary>
        /// Try to setup a level's data.
        /// </summary>
        /// <param name="data">The <see cref="LevelDataCLS"/>.</param>
        /// <param name="path">The path to the level.</param>
        /// <returns>If the data has successfully been set up.</returns>
        public static bool TrySetup(this LevelDataCLS data, string path) {
            data.Setup();
            return data.Decode(Json.DeserializePartially(RDFile.ReadAllText(Path.Combine(path, "main.adofai")), "actions") as Dictionary<string, object>);
        }
    }
}
