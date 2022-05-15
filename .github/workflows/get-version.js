var fs = require("fs");
console.log(fs.readFileSync("Version.props", "utf8").match(/(?<=^[ \t]*<Version>)([0-9]+\.[0-9]+\.[0-9]+)/m)?.[0]);
