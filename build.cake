const string appId = "org.nickvision.cavalier";
const string projectName = "NickvisionCavalier";
const string shortName = "cavalier";
readonly string[] projectsToBuild = new string[] { "GNOME" };

if (FileExists("CakeScripts/main.cake"))
{
    #load local:?path=CakeScripts/main.cake
}
else
{
    throw new CakeException("Failed to load main script.");
}