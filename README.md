# Axie Infinity IP - Tool Kit 2D

## New update
- Starter: Xia, Bing, Noir, Rouge
- Axie NFT lv 2

## Axie
The shared feature of all Axies to distinguish them from other creatures is that they always have 6 fixed parts on their body: `eyes`, `mouth`, `horns`, `ears`, `back` and `tail`. These body parts' looks are often very random.
Based on biological characteristics, the Axies are divided into 6 main races including: `Beast`, `Aquatic`, `Plant`, `Bird`, `Bug` , `Reptile`

### Starter axies
Starter axie is mascot with fixed genes and can't breed. We recommend to used this characters if gameplay not depend on axie parts
![Starter axies](images/starter.png?raw=false "Starter axies")

### NFT axies
`Axie Generator Tool Kit 2D` is a tool provided by Sky Mavis, which transform the data of Axie's binary gene data decoder into body information, parts, colors, ..
- For Usage please follow this repo [mixer-unity](https://github.com/axieinfinity/mixer-unity) from github
![Axie Generator Tool Kit 2D](images/ntf-axie.png?raw=true "Axie Generator Tool Kit 2D")
![Axie LV2](images/axie-lv2.png?raw=true "Axie LV2")

## Chimera
Like the Axies, the Chimeras also have many characteristics that are a combination of many creatures, however, they can not be divided into specific classes like the Axies. Compared to the Axies, the Chimera's appearance seems more chaotic and not following any rules.
![Chimera](images/chimera.png?raw=true "Chimera")

## Other

### Axie part cards
Skill cards from axie battle v2 (classic). Game used `mouth`, `horns`, `back` and `tail` to design skills. We have 6 axie classes, each class have 4 parts, each part have 6 variants (except mouth only have 4).
![Axie part cards](images/axie-part-cards.png?raw=true "Axie part cards")

### Land items
Common materials and items that can be used for many kind of games.
![Land items](images/land-item.png?raw=true "Land items")


# Frequently Asked Questions

### Which `Spine Runtime Library` version should be used?
- All spine assets used [(spine-unity 3.8 2021-11-10)](https://esotericsoftware.com/files/runtimes/unity/spine-unity-3.8-2021-11-10.unitypackage). You need to download it manualy, and put it on Plugins folder.
- In unity 2020 or above, spine import may not compatibility (It require export correct spine version is 3.8.79). You can solve it by import data in unity 2019 project then copy generated assets to your project.

### Why generated axie be wrong color?
- Please set color space to `Gamma` 

### How to solve error `The type or namespace name 'Newtonsoft' could be not found`?
- Sometimes the project has imported another version of Newtonsoft and will be conflicted. You can solve it by open `Packages/manifest.json` then edit version of `"com.unity.nuget.newtonsoft-json": "2.0.0",` to `2.0.2` or `3.0.2`

### How to solve error `Axies missing' when building?
- Navigate to Project Settings, select Graphics, then Shader Loading, and adjust the Preloaded Shaders size to 1. Subsequently, insert the AxieMixerShaderVariants as Element 0.
