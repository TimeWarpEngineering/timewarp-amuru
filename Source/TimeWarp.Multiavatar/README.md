# TimeWarp.Multiavatar

A .NET port of [Multiavatar](https://github.com/multiavatar/Multiavatar) by [Gie Katon](https://github.com/giekaton).

## .NET Installation

```bash
dotnet add package TimeWarp.Multiavatar
```

## .NET Usage

```csharp
using TimeWarp.Multiavatar;

// Generate avatar SVG from any text
string svg = MultiavatarGenerator.Generate("user@example.com");

// Generate without environment circle
string svgNoEnv = MultiavatarGenerator.Generate("username", sansEnv: true);

// Save to file
File.WriteAllText("avatar.svg", svg);
```

## Command Line Tool

Install the TimeWarp.Amuru.Tool:

```bash
dotnet tool install --global TimeWarp.Amuru.Tool
timewarp multiavatar "user@example.com" --output avatar.svg
```

## Standalone Executable

```bash
./multiavatar "user@example.com" --output avatar.svg
./multiavatar "username" --no-env
./multiavatar "test" --output-hash
```

---

# Original Multiavatar README

The following is the original README from https://github.com/multiavatar/Multiavatar

---

# Multiavatar #

<img src="https://raw.githubusercontent.com/multiavatar/Multiavatar/main/logo.png?v=001" width="65">

[Multiavatar](https://multiavatar.com) is a multicultural avatar maker.

Multiavatar represents people from multiple races, multiple cultures, multiple age groups, multiple worldviews and walks of life.

In total, it is possible to generate **12,230,590,464** unique avatars.



### Installation and usage ###

Include the script and pass any string to the 'multiavatar' function. It will return the SVG code for the avatar.


Using npm: 

`npm i @multiavatar/multiavatar`

CommonJS:
```
const multiavatar = require('@multiavatar/multiavatar')
let svgCode = multiavatar('Binx Bond')
```

ES Module:
```
import multiavatar from '@multiavatar/multiavatar/esm'
let svgCode = multiavatar('Binx Bond')
```


Using the script tag:

```
<script src="multiavatar.min.js"></script>

<script>
  var avatarId = 'Binx Bond';
  var svgCode = multiavatar(avatarId);
</script>
```


Include from CDN:

```
<script src="https://cdn.jsdelivr.net/npm/@multiavatar/multiavatar/multiavatar.min.js"></script>
```



### Info ###

The initial unique 48 (16x3) avatar characters are designed to work as the source from which all 12 billion avatars are generated.

You can find them in the `svg` folder. These initial characters can be edited with a vector drawing program, such as Inkscape. They are in grayscale, since the color is applied later by the script, and grayscale is better for shapes design.

Every avatar consists of 6 parts:
- Environment
- Clothes
- Head
- Mouth
- Eyes
- Top

Also, there are different versions of different parts. In some final avatars, additional parts are added or replaced.

### API ###

To get an avatar as SVG code, add the avatar's ID to the URL:

```
https://api.multiavatar.com/Binx Bond
```

To get an avatar as SVG file, add .svg to the URL:

```
https://api.multiavatar.com/Binx Bond.svg
```

To get an avatar as PNG file, add .png to the URL:

```
https://api.multiavatar.com/Binx Bond.png
```

### Avatar Viewer ###

To see all 48 default avatars and their versions, use the Avatar Viewer:

```
https://multiavatar.com/avatar-viewer
```

### License ###

You can use Multiavatar for free, as long as the conditions described in the [LICENSE](https://multiavatar.com/license) are followed.

### More info ###

For more information and extended functionality, visit [multiavatar.com](https://multiavatar.com)

---

## License for this .NET Port

We make no claim to Multiavatar in any way.  
See the Multiavatar license here: https://multiavatar.com/license

The original Multiavatar project allows free use for both commercial and non-commercial purposes. Please review their license for complete terms.