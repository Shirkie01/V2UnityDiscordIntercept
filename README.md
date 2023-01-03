# V2UnityDiscordIntercept

V2Unity is a recreation of Vigilante 8: 2nd Offense in Unity3D. The recreation depended on the Discord Game SDK, which is being discontinued. As the source of version 1.43 has not been uploaded, it cannot be rewritten to account for the Discord Game SDK shutdown.

This project is an attempt to rewrite the bits of the game that used the Discord Game SDK to use a standard networking library, allowing direct connections.

## Tech Stack
BepInEx is used to run third-party libraries.

Harmony is used to provide code modification.

Lidgren Networking is used to provide networking.

## Setup
Download BepInEx 6.0.0-pre.1. Extract the contents to the v2unity folder root.

![image](https://user-images.githubusercontent.com/6500749/210292566-64c0fa39-d22c-44a1-9af5-f693837ac084.png)

Open the BepInEx folder. Create a new folder named `plugins`.

![image](https://user-images.githubusercontent.com/6500749/210292633-e7a6a18c-30fb-4075-84a3-440bc2188626.png)

Download and extract the latest zip of V2UnityDiscordIntercept to the plugins folder.

![image](https://user-images.githubusercontent.com/6500749/210292718-ea2d2b9c-14b5-4e5f-b0de-bee48a1732cc.png)

## Use

When hosting a game, ensure the level is selected before anyone else joins.

When joining a game, a window will pop up to allow entering an IP address and port. The host must have the [port forwarded](https://www.wikihow.com/Set-Up-Port-Forwarding-on-a-Router) through the router.

![image](https://user-images.githubusercontent.com/6500749/210292893-317ba1a3-235e-4a10-b6f0-33c27b48276a.png)

The client can then select the vehicle they want and wait for the host to start the session.
