var fs = require("fs");

var version = fs.readFileSync("Version.props", "utf8").match(/(?<=^[ \t]*<Version>)([0-9]+\.[0-9]+\.[0-9]+)/m)?.[0];
if (fs.existsSync(process.env.GITHUB_OUTPUT))
  fs.appendFileSync(process.env.GITHUB_OUTPUT, `\nversion=${version}`);
