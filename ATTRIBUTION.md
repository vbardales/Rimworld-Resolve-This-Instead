# Attribution

Resolve This Instead is not affiliated with or endorsed by Mlie.

## Use This Instead (Mlie) — the data, read at runtime

The replacement rules come from [Use This Instead](https://steamcommunity.com/sharedfiles/filedetails/?id=3396308787) by Mlie
([source](https://github.com/emipa606/UseThisInstead), MIT licence, Copyright (c) 2020 Mlie). This library is designed to require that mod and to read the
`replacements.json` it keeps in its own folder; it does not copy or redistribute the data.

## MiniJSON — `Source/ThirdParty/MiniJSON.cs`

The JSON reader is the MiniJSON parser as shipped in Use This Instead's `Source/UseThisInstead/Json.cs` (Copyright (c) 2013 Calvin Rien, based on the JSON parser by
Patrick van Bergen), MIT licence; the full notice is kept at the top of the file. Changes: the namespace is `ResolveThisInstead.ThirdParty` and the class is `internal`
(so that it cannot clash with another mod that embeds MiniJSON). The parsing code is unchanged. Found while testing: it throws on a truncated file despite its header,
so the only call is wrapped in `UtiData.Parse`.
