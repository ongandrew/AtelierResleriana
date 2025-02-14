# Atelier Resleriana
Atelier Resleriana is a Unity IL2CPP game made by Koei Tecmo. This project aims to provide localization support for the game through a BepInEx plugin.

## AtelierResleriana.Plugin.Localization
This BepInEx plugin enables custom text localization for Atelier Resleriana (JP) on Steam. It works by intercepting the game's text asset loading process and replacing the original text with localized versions based on a mapping configuration from the global version of the game.

### Features
- Preserves the original text format and structure
- Non-invasive patching that maintains game stability

### How It Works
The plugin uses several key components to achieve localization:
1. **Text Asset Interception**: Uses Harmony to patch the `ProvideHandle.Complete` method, intercepting text assets as they're loaded
2. **Text Format**: Handles the game's PackedText format, which stores text entries with unique IDs
3. **Localization Mapping**: Uses a JSON configuration (`LocalizationMap.json`) to map original text assets to their localized versions
4. **Dynamic Replacement**: Preserves the original text structure while replacing only the necessary content

### Installation
A separate pre-packaged zip is provided as a release on Github so extract into the appropriate directory.
Download the zip's contents and extract it to C:\Program Files (x86)\Steam\steamapps\common\AtelierResleriana

### Configuration
The plugin uses a JSON mapping file that defines the relationship between original and localized text assets. The structure is:
```json
{
    "original_asset_key": {
        "en": "localized_asset_key"
    }
}
```

### Credits
Special thanks to:
* [resleriana-db](https://github.com/theBowja/resleriana-db/tree/main) - for providing insight on the text storage format used by the game.
* [resleriana_sos](https://github.com/CatClighed/resleriana_sos) - highlighting the actual textual assets that exist.

### Disclaimer
This project is not affiliated with or endorsed by Koei Tecmo Games or any of its subsidiaries. All game assets, including text content, artwork, and other media, are the property of their respective owners. This plugin is intended for personal use only and should be used in accordance with all applicable laws and terms of service.

The localization content used by this plugin is derived from officially licensed versions of the game. Users are responsible for ensuring they own legitimate copies of the relevant game versions before using this plugin.

All trademarks, registered trademarks, product names, and company names or logos mentioned herein are the property of their respective owners.

### License
This project's code is provided "as is", without warranty of any kind. Users may fork and modify the code for personal use, but must respect all intellectual property rights related to Atelier Resleriana and its assets.