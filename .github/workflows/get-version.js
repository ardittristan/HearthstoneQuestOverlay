var fs = require("fs");
console.log(fs.readFileSync("QuestOverlayPlugin/Properties/AssemblyInfo.cs", "utf8").match(/(?<=^[^\/]+AssemblyVersion\(")([0-9]+\.[0-9]+\.[0-9]+)/m)?.[0]);
