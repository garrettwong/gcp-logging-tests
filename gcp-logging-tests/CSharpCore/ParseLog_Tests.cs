using gcp_logging_tests.API;
using gcp_logging_tests.Utilities;
using Newtonsoft.Json.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using RestSharp;
using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using Xunit;

namespace gcp_logging_tests.CSharpCore
{
    /// <summary>
    /// dotnet test --filter gcp_logging_tests.CSharpCore.ParseLog_Tests 
    /// </summary>
    public class ParseLog_Tests
    {
        public ParseLog_Tests()
        {

        }

        public class JsonHelper
        {
            public static string GetString(string text, string propertyName)
            {
                JsonElement json = JsonSerializer.Deserialize<JsonElement>(text);
                var propertyValue = "";

                if (json.TryGetProperty(propertyName, out JsonElement messageElement) &&
                    messageElement.ValueKind == JsonValueKind.String)
                {
                    propertyValue = messageElement.GetString();
                }

                return propertyValue;
            }

            // public static dynamic Get(string text, string propertyName)
            // {
            //     JsonElement json = JsonSerializer.Deserialize<JsonElement>(text);
            //     dynamic propertyValue;
            //     if (json.TryGetProperty(propertyName, out JsonElement messageElement) &&
            //         messageElement.ValueKind == JsonValueKind.String)
            //     {
            //         propertyValue = messageElement();
            //     }
            //     return propertyValue;
            // }
        }

        [Fact]
        public void ParseLog()
        {
            string x = "null";

            var contentDir = $"{Directory.GetCurrentDirectory()}/Content";
            var fvizJsonPath = $"{contentDir}/fviz-package.json";
            var text = File.ReadAllText(fvizJsonPath);

            // var jsonObject = JsonSerializer.Serialize(text);
            JsonElement json = JsonSerializer.Deserialize<JsonElement>(text);
            var name = JsonHelper.GetString(text, "name");

            // .name
            Assert.Equal("forseti-visualizer-ui", name);
            Assert.NotNull(contentDir);
        }

        [Fact]
        public void ParseLogJObject()
        {

            var contentDir = $"{Directory.GetCurrentDirectory()}/Content";
            var fvizJsonPath = $"{contentDir}/fviz-package.json";
            var text = File.ReadAllText(fvizJsonPath);

            var root = JObject.Parse(text);
            var version = (string)root.SelectToken("packages.node_modules/@akryum/winattr.version");
            Assert.Equal("3.0.0", version);

            var dependenciesList = root.SelectToken("packages.node_modules/@akryum/winattr.dependencies").Select(s => (string)s).ToList();
            Assert.Equal("^2.17.1227", dependenciesList[0]);

            var packagesList = root.SelectTokens("packages['']").ToList();

            foreach (JObject item in packagesList)
            {
                Assert.Equal("Apache-2.0", item.SelectToken(".license").ToString());
            }
            var deps = root.SelectTokens("packages[''].dependencies").ToList();

            foreach (JObject item in deps)
            {
                Assert.Equal("^5.15.1", item.SelectToken(".d3").ToString());
                Assert.Equal("^1.4.3", item.SelectToken(".eslint-utils").ToString());
                // Assert.Equal("^2.17.1227", item.SelectToken(".license").ToString());
            }

            var paramsArray = root.SelectTokens("packages.node_modules/npm.bundleDependencies");

            foreach (JToken param in paramsArray)
            {
                var bundleDependencies = param.Values<string>();
                
                var list = new string[] { "abbrev", "ansicolors", "ansistyles", 
                "aproba",
        "archy",
        "bin-links",
        "bluebird",
        "byte-size",
        "cacache",
        "call-limit",
        "chownr",
        "ci-info",
        "cli-columns",
        "cli-table3",
        "cmd-shim",
        "columnify",
        "config-chain",
        "debuglog",
        "detect-indent",
        "detect-newline",
        "dezalgo",
        "editor",
        "figgy-pudding",
        "find-npm-prefix",
        "fs-vacuum",
        "fs-write-stream-atomic",
        "gentle-fs",
        "glob",
        "graceful-fs",
        "has-unicode",
        "hosted-git-info",
        "iferr",
        "imurmurhash",
        "infer-owner",
        "inflight",
        "inherits",
        "ini",
        "init-package-json",
        "is-cidr",
        "json-parse-better-errors",
        "JSONStream",
        "lazy-property",
        "libcipm",
        "libnpm",
        "libnpmaccess",
        "libnpmhook",
        "libnpmorg",
        "libnpmsearch",
        "libnpmteam",
        "libnpx",
        "lock-verify",
        "lockfile",
        "lodash._baseindexof",
        "lodash._baseuniq",
        "lodash._bindcallback",
        "lodash._cacheindexof",
        "lodash._createcache",
        "lodash._getnative",
        "lodash.clonedeep",
        "lodash.restparam",
        "lodash.union",
        "lodash.uniq",
        "lodash.without",
        "lru-cache",
        "meant",
        "mississippi",
        "mkdirp",
        "move-concurrently",
        "node-gyp",
        "nopt",
        "normalize-package-data",
        "npm-audit-report",
        "npm-cache-filename",
        "npm-install-checks",
        "npm-lifecycle",
        "npm-package-arg",
        "npm-packlist",
        "npm-pick-manifest",
        "npm-profile",
        "npm-registry-fetch",
        "npm-user-validate",
        "npmlog",
        "once",
        "opener",
        "osenv",
        "pacote",
        "path-is-inside",
        "promise-inflight",
        "qrcode-terminal",
        "query-string",
        "qw",
        "read-cmd-shim",
        "read-installed",
        "read-package-json",
        "read-package-tree",
        "read",
        "readable-stream",
        "readdir-scoped-modules",
        "request",
        "retry",
        "rimraf",
        "safe-buffer",
        "semver",
        "sha",
        "slide",
        "sorted-object",
        "sorted-union-stream",
        "ssri",
        "stringify-package",
        "tar",
        "text-table",
        "tiny-relative-date",
        "uid-number",
        "umask",
        "unique-filename",
        "unpipe",
        "update-notifier",
        "uuid",
        "validate-npm-package-license",
        "validate-npm-package-name",
        "which",
        "worker-farm",
        "write-file-atomic" };
                foreach (var bd in bundleDependencies)
                {
                    if (!list.Contains(bd)) Assert.Equal("ansistyles", bd);

                    Assert.True(list.Contains(bd));
                }
            }

            // /Users/garrettwong/Git/gcp-logging-tests/gcp-logging-tests/Content/fviz-package.json
            // https://learn.microsoft.com/en-us/answers/questions/942819/how-to-get-values-from-jobject-using-selecttoken-w
        }
    }
}
