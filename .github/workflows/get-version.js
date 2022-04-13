var fs = require("fs");
console.log(fs.readFileSync("QuestOverlayPlugin/QuestOverlayPlugin.csproj", "utf8").match(/(?<=^[ \t]*<Version>)([0-9]+\.[0-9]+\.[0-9]+)/m)?.[0]);
