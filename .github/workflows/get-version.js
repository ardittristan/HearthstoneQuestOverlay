var fs = require("fs");
console.log(fs.readFileSync("module.json", "utf8").match(/(?<=^[^\/]+AssemblyVersion\(")([0-9]+\.[0-9]+\.[0-9]+)/m)?.[0]);