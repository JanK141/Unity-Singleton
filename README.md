# Unity-Singleton
Over engineered Singleton base class for Unity

After creating first instance of it on the scene it will automatically create a prefab
being instantiated every time when reffered to, while there is no instance present.
With that if for some reason (????) you would delete your singleton from a scene, worry no more! It will be created on demand.
We love our singletons, and you know what?
If your singleton isn't persistant, or you are testing some scene directly without loading a scene in which this singleton should be loaded, it will be there for you no matter what! Isn't that a true love?

Do you need it? ...propably no.
Do you WANT it? ...tbh I didnt even test that fully so I doubt that.
BUT! Will it help you? Let me tell you my friend, I just solved a problem you never had (and neither did I), so we will never know that for sure.

Remember to call base implementation of methods like: Awake, Enable/Disable, ApplicationQuit, Reset, when overriding them.
