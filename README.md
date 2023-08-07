# Ubiq Avatars (ReadyPlayerMe)

![rpm-avatar](https://github.com/UCL-VR/ubiq-avatars-readyplayerme/assets/33021110/ee4851cd-bf62-4b27-9dd9-4938f2ce87c6)

Compatibility package for Ready Player Me avatars and Ubiq. Currently support is only good for `HalfBody` avatars.

## Installation

1. Add git packages in the Unity package manager (UPM). UPM can't yet support hierarchical dependencies in git packages, so you'll need to install dependencies manually. For more info see UPM instructions [here](https://docs.unity3d.com/Manual/upm-ui-giturl.html).
```
https://github.com/UCL-VR/ubiq.git#v0.5.0-upm
https://github.com/readyplayerme/rpm-unity-sdk-core.git#v1.3.3
```
2. In the UPM window, select Ubiq from the package list and import the Samples. Note that you must do this now as this package depends on some features of the core Ubiq samples.
3. Add this git package through UPM.
```
https://github.com/UCL-VR/ubiq-avatars-readyplayerme.git#v1.0.0-upm
```

## Using a Ready Player Me avatar in Ubiq

The `RPM-Avatar` prefab can be added to the `AvatarCatalogue` on your `AvatarManager` to make it spawnable. To also make it the default, replace `avatarPrefab` on the `AvatarManager` with `RPM-Avatar`. See the Example scene in this package's Samples to see this already configured. Samples can be installed through the UPM window (select this package).

Unity will not allow you to change the prefab directly as it comes from a package which may be cached. Should you want to make changes to the prefab, make a [prefab variant](https://docs.unity3d.com/Manual/PrefabVariants.html).

## Changing the avatar model

Avatar models can be designed with Ready Player Me's web interface. An url is provided once the model has been created. Note before you start that this package currently only supports `HalfBody` avatars! The web interface can be found [here](https://demo.readyplayer.me).

Avatar loading is done at runtime by the `UbiqReadyPlayerMeAvatarLoader` script on the `RPM-Avatar` prefab. Different models can be loaded by changing the `avatarUrl` variable on this script. Urls can also be supplied at runtime by calling the `Load` method on the script.
